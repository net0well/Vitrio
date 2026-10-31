# Plano: Busca de prestadores

**Status:** Em andamento
**Spec:** [004-busca-de-prestadores](../specs/004-busca-de-prestadores.md) (Approved)
**Arquitetura:** Clean Architecture com Controllers ([03-arquitetura.md](../03-arquitetura.md))
**Camadas afetadas:**
- Domain: `Favorites` (agregado `Favorite`).
- Application:
  - `Search`: busca, Início, área da busca e repository de listagem;
  - `Favorites`: salvar, remover e listar;
  - `Locations`: localizar a cidade com cache, reaproveitado pela busca;
  - `Providers`: `isFavorite` no perfil público;
  - `Accounts`: favoritos apagados na exclusão.
- Shared: `UniqueConstraintException`, ao lado de `ConcurrencyConflictException` (recorte 3).
- Infrastructure:
  - colunas e índices de busca em `providers`, e o interceptor que mantém o índice;
  - tabela `favorites`;
  - pesos da relevância por configuração.
- Api: busca em `ProvidersController`, `HomeController` e `FavoritesController`.
- Infra: script de carga sintética em `infra/search-load/`.
- App Flutter: Início, folha "Categorias", Busca, folha "Filtros", favoritos e distância no perfil.

## Recortes

Todos na branch `feat/busca-de-prestadores`, em vários commits. O PR só abre quando todos estiverem prontos e testados de ponta a ponta.

| # | Recorte | Critérios |
| --- | --- | --- |
| 1 | Índice e busca na API: colunas e índices, modo GPS ou cidade, filtros, ordenações e relevância | AC-1 a AC-7, AC-8 (busca), AC-9 a AC-19 |
| 2 | Início na API: categorias, "Bem avaliados" com cache por célula, `hasProviders` | AC-8 (Início), AC-10, AC-20, AC-21 |
| 3 | Favoritos na API: salvar, remover, listar, `isFavorite` e exclusão da conta | AC-10, AC-22 a AC-28 |
| 4 | Carga sintética de 50 mil vitrines e medição da busca | Requisito de p95 ≤ 400 ms |
| 5 | App: Início completa e folha "Categorias" | AC-30 |
| 6 | App: Busca, chips, ordenação, folha "Filtros" e cidade sem prestadores | — |
| 7 | App: coração no perfil, distância no perfil e tela "Favoritos" | AC-29 |
| 8 | Fechamento: revisões de back e de front, testes no emulador e PR | Todos |

## Decisões técnicas

| Tema | Decisão |
| --- | --- |
| Ponto público indexado | Coluna gerada `public_location geography(point, 4326)` a partir de `public_latitude` e `public_longitude`, com índice GiST. É propriedade de sombra (NTS) só na Infrastructure: o Domain continua com latitude e longitude. |
| Índice de texto | Três colunas de texto mantidas pela Infrastructure, já sem acento e minúsculas (`SearchText.Normalize`): `search_primary` (profissão, categorias e títulos dos serviços), `search_name` (nome de exibição) e `search_secondary` (descrições e "Sobre"). Delas saem `search_vector` (coluna gerada com a configuração `portuguese` nativa e pesos A, B e C, índice GIN) e `search_terms` (primária mais nome, índice GIN trigram). O termo buscado passa pela mesma normalização. Sem configuração de texto própria no banco, o esquema também sai do modelo (`EnsureCreated`) igual ao das migrações. |
| Manutenção do índice | `ProviderSearchIndexInterceptor` (`SaveChangesInterceptor`, como a outbox) recalcula as colunas de busca de toda vitrine adicionada ou alterada, a partir do agregado carregado inteiro pelo Writable repository e dos nomes das categorias. A migração preenche as vitrines que já existem. Não há evento novo: a busca não tem cache e lê o estado atual. |
| Modo da busca | `ISearchAreaService` (Search/Services) resolve o modo (GPS ou cidade) e a cidade do contexto. Ele usa um `ICityLocationService` novo em Locations, que concentra o "localizar com cache" do `LocateCityQueryHandler`, para os dois não duplicarem a regra nem a chave de cache. Com ponto e código juntos, vale GPS só quando o ponto cai dentro do limite daquela cidade. |
| Consulta | `IProviderListingSearchableRepository` (Search/Repositories) com `SearchAsync`, `ListTopRatedAsync` e `HasAnyServingAsync`. SQL parametrizado montado por `ProviderListingSql` (Infrastructure), porque a relevância e os filtros opcionais ficam legíveis e conferíveis com `EXPLAIN`; nenhum valor do request entra no texto do SQL. Texto: `websearch_to_tsquery` + similaridade de palavra do `pg_trgm` (a partir de 0,5); termo só com stopwords não filtra. A projeção (URL da foto, iniciais, arredondamento pela regra do Domain) é reaproveitada pela listagem dos favoritos. |
| Filtros de área | Modo GPS: pré-filtro de 50 km pelo índice, depois `ST_DWithin(public_location, ponto, raio)` e `maxDistanceKm`. Modo cidade: `city_ibge_code = cidade` ou `ST_DWithin(public_location, cities.boundary, raio)`. Nunca o endereço base. Sempre vitrine `Published` e conta ativa, como no perfil público. |
| Relevância | Calculada no SQL com os componentes e pesos da spec. Os pesos ficam em `Search:Relevance` (appsettings), lidos pela Infrastructure com `ValidateOnStart`, e o componente ausente (sem termo ou sem GPS) é redistribuído. Texto: o maior entre o `ts_rank` normalizado e a similaridade trigram de palavra. |
| Termo | Normalizado com `SearchText.Normalize` (Domain). Vazio, com uma letra ou só com stopwords vira busca sem texto. Nunca vai para log nem para chave de cache. |
| Início | `GetHomeQuery` devolve as categorias (com o cache que já existe), `topRated` e `hasProviders`. `topRated` usa `IReadCache` (60 s Redis, 30 s local) com chave `home:cell:{celula2km}` ou `home:city:{ibge}`. Guarda até 30 candidatos com nota que atendem o centro da célula com folga de 1,5 km. A cada requisição, o handler confere o raio a partir do ponto do cliente, calcula a distância e "Disponível hoje" pelas regras do Domain e corta em 10. `hasProviders` é um `EXISTS` com índice, sem cache. |
| Favoritos | `Favorite` em `Domain/Favorites` (fábrica com `IClock`, limite de 200, erros em `FavoriteErrors`), tabela `favorites` com único em (`user_id`, `provider_id`) e índice (`user_id`, `created_at desc`). Repositories: `IFavoriteWritableRepository` (existe, conta, adiciona, remove e remove todos da conta) e `IFavoriteSearchableRepository` (lista paginada projetada em `ProviderSearchItem`, só vitrines publicadas de contas ativas; existe para o `isFavorite`). |
| Corrida ao salvar favorito | O `UnitOfWork` traduz violação de índice único (`23505`) em `UniqueConstraintException` (Shared). O handler de salvar trata como sucesso, porque o favorito já existe. |
| Perfil público | `ProviderPublicResponse` ganha `isFavorite`, calculado fora do cache pelo `ICurrentUser` quando autenticado. |
| Exclusão da conta | `DeleteAccountCommandHandler` apaga os favoritos da conta na mesma transação, como faz com endereços e aparelhos. Os favoritos que outros salvaram da vitrine removida continuam no banco e saem da lista pela consulta. |
| API | `ProvidersController` ganha `GET /providers` com `[EnableRateLimiting(Search)]` e `SearchProvidersRequest` (`[FromQuery]`, `ToQuery()`). `HomeController` (`GET /home`, `search`). `FavoritesController` em `me/favorites`: autenticado, `GET` com `profile`, `PUT` e `DELETE` no limitador `write`. Todas as respostas `no-store`. |
| Testes da API | Classe de testes própria com fábrica própria, porque a busca enxerga todas as vitrines do banco. O `FakeCepLookup` ganha CEPs registráveis com cidade e ponto, para criar vitrines publicadas onde cada teste precisa. As cidades falsas (Jaboticabal, São Paulo, Santos e outras) cobrem os casos de cidade grande e de cidade vizinha. Nota e avaliações são gravadas direto no banco nos testes até a spec 007. |
| Carga sintética | `infra/search-load/seed.sql` gera 50 mil contas e vitrines publicadas espalhadas pelas cidades de SP, com categorias, serviços e texto variado. `measure.ps1` roda as buscas típicas na API local e mostra p50 e p95. `cleanup.sql` apaga tudo, sem tocar na conta fixa de teste. |
| App | `features/search` (data, domain e presentation) com um controller Riverpod paginado (até 10 páginas) e filtros num estado imutável. A busca pede o ponto atual do aparelho só no modo GPS e não o guarda. O card usa o `VitrioProviderCard`, e a Início ganha a variante compacta dele, como na prancheta. Folhas "Filtros", "Ordenar" e "Categorias", e um widget de convite de cidade sem prestadores. O coração usa o `VitrioIconButton` (`soft` quando salvo). Sem login, ele registra a intenção de favoritar aquele prestador e, depois do login, volta ao perfil salvando. Tela "Favoritos" e linha na Conta. O perfil público passa o ponto no modo GPS para mostrar a distância. |

## Recorte 1: índice e busca na API

**Situação:** pronto. 34 testes de integração da busca; a regra de nota mínima (`RatingRule`) e o localizador de cidade com cache (`ICityLocationService`) foram extraídos para a busca e o perfil público usarem os mesmos.

1. Infrastructure:
   - migração `AddProviderSearch`: `public_location`, colunas e vetor de busca, índices e preenchimento das vitrines existentes;
   - `ProviderSearchIndexInterceptor`;
   - opções `Search:Relevance`.
2. Locations: `ICityLocationService` extraído do `LocateCityQueryHandler`.
3. Search:
   - `ISearchAreaService`;
   - `IProviderListingSearchableRepository` e implementação;
   - `SearchProvidersQuery`, validador e handler;
   - DTOs (`ProviderSearchItem`, `ProviderSearchPage`, `SearchContextResponse`, `SearchSort`, `SearchMode`).
4. Api: `GET /providers` no `ProvidersController` com a política `search`.
5. Testes:
   - integração da busca (critérios do recorte, 429 com `Retry-After`, nada de telefone, endereço ou coordenadas);
   - domínio e aplicação do que for regra pura;
   - arquitetura verde.

## Recorte 2: Início na API

**Situação:** pronto. 11 testes de integração. Os candidatos de "Bem avaliados" ficam no cache por célula de 0,02° (até 50, com folga de 2 km) ou por cidade; raio, distância e "Disponível hoje" são recalculados a cada requisição pelas regras do Domain. A lista de categorias com cache virou o `ICategoryListService`, usado também por `GET /categories`, e a validação do ponto ou da cidade é comum à busca e à Início (`SearchLocationValidator`).

1. `GetHomeQuery`, validador e handler, com o cache por célula ou cidade e a conferência por requisição.
2. `ListTopRatedAsync` e `HasAnyServingAsync` no repository de listagem.
3. `HomeController` com a política `search`.
4. Testes: nota só de quem tem 3+ avaliações, cidade vazia, vitrine pausada fora, cache sem congelar distância e "Disponível hoje".

## Recorte 3: favoritos na API

**Situação:** pronto. 13 testes de integração, inclusive a corrida de três gravações ao mesmo tempo e o limite de 200 (semeado direto no banco). A leitura e o mapeamento da listagem ficaram num leitor interno (`ProviderListingReader`), usado pela busca, pela Início e pelos favoritos.

1. Shared: `UniqueConstraintException`; `UnitOfWork` traduz `23505`.
2. Domain: `Favorite` e `FavoriteErrors`.
3. Application: `AddFavorite`, `RemoveFavorite` e `ListFavorites`, repositories e `isFavorite` no perfil público.
4. Infrastructure: tabela, configuração, repositories e migração `AddFavorites`.
5. Exclusão da conta apagando os favoritos.
6. Api: `FavoritesController`.
7. Testes:
   - 401 nas três rotas;
   - favorito de outra conta nunca aparece;
   - própria vitrine e vitrine pausada com 404;
   - limite de 200;
   - idempotência e corrida;
   - vitrine pausada some e volta;
   - `isFavorite` com e sem token;
   - exclusão da conta.

## Recorte 4: carga sintética

**Situação:** pronto. 50 mil vitrines publicadas espalhadas pelos 645 municípios de SP, com peso pela população (bem mais denso que o cenário da spec, de 50 mil no Brasil todo). p95 pela API local, 55 requisições por cenário:

| Cenário | p50 | p95 |
| --- | --- | --- |
| GPS com texto (São Paulo, "eletricista") | 62 ms | 84 ms |
| GPS sem texto (São Paulo) | 110 ms | 125 ms |
| GPS com erro de digitação (Campinas) | 49 ms | 57 ms |
| GPS, categoria e mais perto (Campinas) | 57 ms | 70 ms |
| Cidade grande sem GPS (São Paulo, "pintor") | 126 ms | 148 ms |
| Cidade grande sem GPS e sem filtro (São Paulo) | 478 ms | 532 ms |
| Cidade sem GPS, menor preço (Ribeirão Preto) | 72 ms | 93 ms |
| Início com GPS (São Paulo) | 62 ms | 75 ms |

O que a medição mudou na consulta:
- **JIT desligado na conexão:** o custo estimado alto disparava o JIT do PostgreSQL, que levava de 1,1 a 1,8 s compilando a consulta.
- **Cidade pela chave primária:** o planejador subestimava as vitrines filtradas e varria `cities` (com os limites) uma vez por vitrine; a cidade virou subconsulta lateral com `offset 0`.
- **Texto pelo índice:** a similaridade trigram passou ao operador `<%`, que usa o índice; o limiar de 0,5 vai na configuração da sessão (`pg_trgm.word_similarity_threshold`).
- **Área por cidade indexável:** a cidade base pelo índice de `city_ibge_code` ou as vizinhas pelo GiST, antes da conta exata do raio até o limite.
- **Ordenar antes de completar:** completude e menor preço por serviço ficam gravados na vitrine (`search_completeness` e `search_service_price_cents`, pelo interceptor); a consulta ordena e corta a página antes de buscar MEI e o preço do card.

A cidade de São Paulo sem nenhum filtro (cerca de 31 mil candidatas nesta carga) fica acima de 400 ms; o critério da spec (GPS com texto) passa com folga. Se a densidade real chegar perto disso, entra a decisão adiada do motor de busca dedicado.

1. Scripts em `infra/search-load/` e instruções no README de `infra/`.
2. Medição com GPS e texto, com GPS sem texto, por cidade grande e por categoria. O resultado (`EXPLAIN ANALYZE` e p95) vai na descrição do PR.
3. Ajuste de índices ou da consulta se o p95 passar de 400 ms.

## Recorte 5: Início no app

1. `GET /home` no app (repository, domínio e controller).
2. Início igual à prancheta:
   - card "Disponíveis agora" abrindo a Busca com "Atende hoje";
   - categorias abrindo a Busca filtrada;
   - "Ver todas" e "Mais" abrindo a folha "Categorias";
   - "Bem avaliados perto de você" com a variante compacta do card;
   - convite de cidade sem prestadores.
3. Testes de widget dos estados (carregando, com dados, sem nota, cidade vazia, erro).

## Recorte 6: Busca no app

1. Busca igual à prancheta:
   - campo com voltar, limpar e filtros;
   - chips (categoria, Perto de mim, Nota 4,5+, MEI verificado, Atende hoje, Menor preço);
   - linha "Profissionais perto de você" com o botão de ordem;
   - cards de resultado;
   - rolagem paginada até 10 páginas.
2. Folhas "Filtros" e "Ordenar".
3. Estados: skeleton, vazio com filtros, cidade sem prestadores, erro com "Tentar de novo".
4. Testes de widget e o fluxo de busca no emulador contra a API local.

## Recorte 7: favoritos no app

1. Coração no perfil público, com o estado salvo e a volta depois do login.
2. Distância no perfil no modo GPS.
3. Tela "Favoritos", rota e linha na Conta.
4. Testes de widget e fluxo no emulador (favoritar sem login, entrar, voltar salvo, ver na lista, remover).

## Recorte 8: fechamento

1. Revisão de back:
   - `/code-review`, `/security-scan` e `/arch-check`;
   - API no ar testada endpoint por endpoint, com os casos de sucesso e de erro, usando a conta fixa de teste.
2. Revisão de front: cada tela no emulador, por captura, contra a prancheta (Início, Busca, Perfil) e o design system; código do app contra os padrões e antipadrões.
3. Docs:
   - `03-arquitetura.md` §5 (colunas geradas e consulta);
   - `flutter.md` (variante compacta do card);
   - este plano como concluído.
4. PR com o resultado das revisões, dos testes e da medição.

## Riscos

| Risco | Mitigação |
| --- | --- |
| Tradução do LINQ para o SQL da relevância ficar ruim ou lenta | `EXPLAIN` desde o recorte 1; cair para SQL parametrizado no repository |
| Limiar do trigram não achar "eletrisista" ou "pintor" × "Pintura" | Testes com os termos da spec contra PostGIS real; limiar ajustado no SQL, não no GUC global |
| Modo cidade em cidade grande varrer muitas linhas | Índice em `city_ibge_code` + GiST; medir na carga sintética com São Paulo |
| Testes de busca interferindo entre si | Fábrica própria e pontos distintos por teste |
| Coluna gerada de `geography` e EF Core | Propriedade de sombra com `HasComputedColumnSql(stored: true)`; migração revisada |
