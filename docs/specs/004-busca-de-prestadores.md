# Spec 004 — Busca de prestadores

- **Status:** Approved (2026-10-30)
- **Data:** 2026-10-28 (revisada em 2026-10-30)
- **Responsável:** Wellington

## Problem

O cliente precisa achar rápido quem atende onde ele está (ou na cidade que escolheu), comparar nota, distância e preço de referência, e chegar ao perfil certo. A busca tem de funcionar com e sem GPS, tolerar acentos e erros de digitação e resistir a raspagem.

## Scope

### In

- Busca por texto (profissão, categoria, serviço, nome) com tolerância a acento e pequenos erros.
- Filtro por categoria, distância máxima, nota mínima, MEI verificado, atende hoje, preço máximo "a partir de" e cidade.
- Ordenação: relevância (padrão), mais perto (só com GPS), melhor avaliado, menor preço.
- Contexto de localização da spec 002: ponto do GPS ou cidade escolhida.
- Início: categorias, card "Disponíveis agora", "Bem avaliados perto de você" e o convite de cidade sem prestadores.
- Favoritos: salvar, remover e listar (até 200 por conta), e o estado de favorito no perfil público.
- Índice de busca da vitrine (texto e ponto público), mantido a cada mudança da vitrine.
- Script de carga sintética para medir a busca com 50 mil prestadores.
- App:
  - Início completa, com a folha "Categorias";
  - Busca e resultados, com chips, ordenação, folha "Filtros", paginação e estados;
  - coração "Salvar nos favoritos" e distância no perfil público;
  - tela "Favoritos", aberta pela Conta.

### Out

- Mapa, buscas recentes, anúncios patrocinados.
- Busca por disponibilidade em horário específico.
- "Seu último pedido" na Início (spec 005).
- Nota e avaliações (spec 007): até lá ninguém tem nota, e a seção "Bem avaliados perto de você" fica escondida.
- Reindexar as vitrines quando o admin renomear uma categoria (spec 008).
- Personalização por histórico.

### Diferenças aprovadas do protótipo do app

Aprovadas pelo usuário em 2026-10-30. Fora delas, as telas seguem a prancheta do [protótipo](https://claude.ai/artifact/Letmw6AR9zfrFK7gaA79xt).

- **Busca, linha acima da lista:** sem número de resultados, porque a busca pública não devolve total (antirraspagem). Com GPS, "Profissionais perto de você"; com cidade escolhida, "Profissionais em Jaboticabal, SP". Com categoria filtrada, o nome dela entra no lugar de "Profissionais" ("Elétrica perto de você").
- **Card de resultado:** a cidade entra no lugar do bairro ("4,9 (127) · 1,2 km · Jaboticabal"; sem GPS, "4,9 (127) · Jaboticabal"), pela mesma privacidade do perfil público. O selo "Orçamento sem custo" da prancheta não tem dado por trás e não aparece.
- **Chips da Busca:** "Perto de mim" e "Menor preço" mudam a ordenação (um desliga o outro, e o botão de ordem mostra a atual). Sem GPS, o chip "Perto de mim" e a opção "Mais perto" não aparecem.
- **Categoria escolhida:** aparece como chip ligado no começo da fila de chips; tocar nele tira o filtro. O campo de busca fica para texto.
- **Início:**
  - o card "Disponíveis agora" sempre aparece, e "Ver disponíveis" abre a Busca com "Atende hoje" ligado (não há lista na Início);
  - "Ver todas" e o tile "Mais" abrem a folha "Categorias", com as 15 em grade de 4 colunas;
  - "Bem avaliados perto de você" mostra só quem tem nota; sem ninguém com nota, a seção não aparece;
  - cidade sem prestadores: o card de convite entra no lugar de "Bem avaliados" (também na Busca, no lugar da lista);
  - "Seu último pedido" entra com a spec 005.
- **Perfil público:** o coração "Salvar nos favoritos" fica ao lado de "Compartilhar", como na prancheta. Salvo, usa o `VitrioIconButton` `solid` com o rótulo "Remover dos favoritos".
- **Telas sem prancheta**, montadas com os componentes do design system:
  - folha "Filtros";
  - folha "Categorias";
  - tela "Favoritos" (linha "Favoritos" na Conta, com o ícone `heart`).

## Domain Model

- `Favorite` (agregado pequeno, sem eventos): `UserId`, `ProviderId`, `CreatedAt`.
  - Único por par.
  - Até 200 por conta.
  - Não vale para a própria vitrine.
  - Apagado junto com a conta, na exclusão (como os endereços).
- Índice de busca da vitrine, recalculado a cada mudança de nome, profissão, "Sobre", categorias ou serviços:
  - peso alto: profissão, nomes das categorias e títulos dos serviços;
  - peso médio: nome de exibição;
  - peso baixo: descrições dos serviços e "Sobre".
  - Texto em português, sem acento, com `unaccent` e `pg_trgm`.
- Ponto público indexado (`public_location`, `geography`), derivado do ponto público da spec 003.
- Projeção de busca (leitura) `ProviderSearchItem`:
  - `id`, `displayName`, `headline`, `initials`, `avatarUrl?`;
  - `rating { score, count }` ou `isNew` (menos de 3 avaliações), como no perfil público;
  - `cityName`;
  - `distanceKm?`: arredondada, do ponto público; só no modo GPS;
  - `badges { meiVerified, availableToday }`, com "Disponível hoje" calculado na consulta (ligado hoje no fuso da cidade base);
  - `responseTime?`: nulo até a spec 005;
  - `price { from, unit }` ou nulo, com `from` em número como todo dinheiro da API: "a partir de" conforme [02-regras-de-negocio.md §3](../02-regras-de-negocio.md); nulo quando todos os serviços são "Sob orçamento".
- **Modo da busca:**
  - só `lat`/`lng`: modo GPS, e `context.city` é a cidade do ponto;
  - só `cityIbgeCode`: modo cidade;
  - os dois: modo GPS se o ponto está dentro do limite da cidade informada; senão, modo cidade.
- **Área de atendimento na busca:**
  - modo GPS: o ponto do cliente precisa estar dentro do raio medido do **ponto público**, e o ponto público a até `maxDistanceKm` (padrão 20 km);
  - modo cidade: entram os prestadores com cidade base na cidade escolhida e os de fora cujo raio, do ponto público, alcança o limite do município.
  - O endereço base nunca entra no filtro.
- **Relevância:** soma ponderada de componentes de 0 a 1, com pesos configuráveis (appsettings) e sem patrocínio:

  | Componente | Peso inicial | Valor |
  | --- | --- | --- |
  | Texto | 0,35 | Correspondência do termo (full-text e trigram), pelos pesos do índice |
  | Nota | 0,25 | Nota bayesiana em 0–1; sem nota (menos de 3 avaliações), 0,5 |
  | Volume de avaliações | 0,15 | `log(1 + n) / log(1 + 100)`, até 1 |
  | Proximidade | 0,15 | `1 − distância / distância máxima`; só no modo GPS |
  | Completude | 0,05 | Foto de perfil, 3 ou mais fotos de trabalho ativas, disponibilidade semanal e anos de experiência, um quarto cada |
  | Disponível hoje | 0,05 | 1 com o selo, 0 sem |

  Sem termo de busca ou fora do modo GPS, o peso do componente que falta é redistribuído proporcionalmente entre os outros.
- **Ordenações:**
  - `relevance`: maior relevância primeiro;
  - `distance`: mais perto primeiro, depois relevância; só no modo GPS;
  - `rating`: maior nota primeiro, sem nota no fim, depois número de avaliações e relevância;
  - `price`: primeiro os que têm preço por serviço, do menor para o maior; depois os demais (só m², hora, dia, unidade ou "Sob orçamento"), por relevância.
- **Início:**
  - `topRated`: até 10 prestadores com nota (3 ou mais avaliações) que atendem o contexto, pela nota e depois pela distância (modo GPS) ou pelo número de avaliações;
  - `hasProviders`: se há ao menos um prestador publicado que atende o contexto.

## API Contract

| Verbo | Rota | Request | Response | Erros |
| --- | --- | --- | --- | --- |
| GET | `/api/v1/providers` | `q?` (até 60), `categoryId?`, `lat?`, `lng?`, `cityIbgeCode?`, `maxDistanceKm?` (1–50; só no modo GPS; padrão 20), `minRating?` (3–5), `meiVerified?`, `availableToday?`, `maxPriceFrom?` (1–100.000; só preços por serviço), `sort?` (`relevance`, `distance`, `rating`, `price`), `page?` (1–10), `pageSize?` (1–30, padrão 20) | `{ items: [ProviderSearchItem], page, pageSize, hasMore, context: { mode: "nearMe" \| "city", city } }` | 400, 429 |
| GET | `/api/v1/home` | `lat?`, `lng?`, `cityIbgeCode?` (mesmas regras de modo) | `{ city, categories, topRated: [..10], hasProviders }` | 400, 429 |
| GET | `/api/v1/me/favorites` | `page?` (20 por página) | `{ items: [ProviderSearchItem], page, pageSize, hasMore }`, do mais recente para o mais antigo | 401 |
| PUT | `/api/v1/me/favorites/{providerId}` | — | 204 (repetir também responde 204) | 401, 404 (inexistente, não publicada ou a própria vitrine), 409 limite de 200 |
| DELETE | `/api/v1/me/favorites/{providerId}` | — | 204 (também quando não estava salvo) | 401 |
| GET | `/api/v1/providers/{id}` (já existe) | `?lat=&lng=` | `providerPublic` + `isFavorite` (false sem login) | sem mudança |

Erros de validação da busca e da Início (400):

- nem `lat/lng` nem `cityIbgeCode`;
- só um de `lat` e `lng`, ou ponto fora do Brasil;
- `cityIbgeCode` inexistente;
- `sort=distance` ou `maxDistanceKm` fora do modo GPS;
- valores fora das faixas acima, inclusive `page` acima de 10.

## Authorization

- `GET /providers` e `/home`: anônimos, política de rate limit `search`.
- `GET /providers/{id}`: anônimo como antes; com token válido, traz `isFavorite` da própria conta, calculado fora do cache.
- Favoritos: autenticado, sempre filtrado pelo usuário do token. `GET /me/favorites` usa a política `profile`; `PUT` e `DELETE` usam o limitador `write` (60/min).
- Respostas da busca, da Início e dos favoritos nunca têm telefone, endereço, bairro base, CNPJ nem coordenadas.
- Quem busca sendo prestador vê a própria vitrine nos resultados, como qualquer cliente.

## Edge Cases & Failure Modes

- Sem `lat/lng` e sem `cityIbgeCode`: 400 (o app sempre tem um dos dois).
- Com `lat/lng` e `cityIbgeCode` de outra cidade: modo cidade para a cidade escolhida; a distância não é devolvida.
- Cidade grande sem GPS (ex.: São Paulo): prestador da Zona Sul com raio de 5 km aparece para quem escolheu São Paulo, porque a cidade base é a escolhida.
- Prestador só com preço por m² ou por hora: o card mostra o valor com a unidade; ele não entra no filtro `maxPriceFrom` e, com `sort=price`, vai para depois dos que têm preço por serviço.
- Prestador só com serviços "Sob orçamento": `price` nulo, e o card mostra "Sob orçamento".
- Termo com erro ("eletrisista"): o trigram encontra "eletricista".
- Termo vazio após normalização, só com stopwords ou com uma letra: busca só pelos filtros.
- Termo com mais de 60 caracteres: 400.
- Cidade sem prestadores: lista vazia + `context.city`; na Início, `hasProviders: false`. O app mostra o convite de cidade sem prestadores.
- `page` acima de 10 ou `pageSize` acima de 30: 400 (antirraspagem).
- Vitrine pausada, em rascunho ou removida, ou conta suspensa ou excluída: nunca aparece na busca nem na Início; os favoritos dela somem da lista, continuam guardados e voltam se ela for publicada de novo.
- Nota com menos de 3 avaliações: `rating` nulo e `isNew`; o app mostra "Novo no Vitrio".
- Mesmo cliente buscando muitas vezes por segundo: 429 com `Retry-After`.
- Salvar o mesmo favorito duas vezes, ou em dois aparelhos ao mesmo tempo: 204 nas duas, um registro só.
- Favoritar a própria vitrine: 404, como recurso inexistente.
- 201º favorito: 409 com "Você chegou a 200 favoritos. Remova algum para salvar outro."
- Conta excluída: os favoritos dela são apagados na exclusão.
- Mudança na vitrine (nome, profissão, "Sobre", categorias, serviços, área, publicação, pausa): a busca reflete na hora, porque não há cache.
- O termo buscado e as coordenadas nunca vão para log.

## Non-Functional Requirements

- p95 ≤ 400 ms para buscas com GPS e texto, com 50 mil prestadores ativos. Medido com carga sintética no banco local (script em `infra/`), com `EXPLAIN ANALYZE` e tempos de resposta registrados na descrição do PR.
- Índices:
  - GiST em `providers.public_location` e `cities.boundary`;
  - GIN no vetor de busca;
  - trigram nos termos normalizados.
  - Pré-filtro espacial de 50 km no modo GPS.
- Cache:
  - `GET /providers` não tem cache (sempre do banco) e responde `no-store`.
  - `GET /home` tem cache de 60 s no Redis e 30 s local, por célula de ~2 km (modo GPS) ou por cidade ([09-cache.md](../09-cache.md)). Raio, distância e "Disponível hoje" são conferidos a cada requisição a partir do ponto do cliente, para a Início seguir a mesma regra da busca.
- Filtro de raio, filtro de distância máxima e distância exibida usam o mesmo ponto público, arredondado conforme [02-regras-de-negocio.md §2](../02-regras-de-negocio.md): o card nunca mostra "10,3 km" com `maxDistanceKm=10`.

## Integrations

Nenhuma externa. Usa PostGIS, `pg_trgm` e a busca de texto em português do PostgreSQL sobre o texto normalizado sem acento. Não há evento novo: favorito não gera evento, e a busca lê o estado atual da vitrine.

## Acceptance Criteria

### Busca

- **AC-1** Dados prestadores publicados a 2 km e a 12 km de um ponto, ambos com raio de 15 km, quando o cliente busca com esse ponto e `sort=distance`, então o de 2 km vem primeiro, a distância do primeiro vem com uma casa decimal e a do segundo como inteiro (calculadas a partir do ponto público).
- **AC-2** Dado um prestador cujo ponto público está a 8 km do cliente, com raio de 5 km, quando o cliente busca a partir desse ponto, então ele não aparece, mesmo que o endereço base esteja a menos de 5 km.
- **AC-3** Dado o termo "eletrisista", quando o cliente busca, então prestadores de Elétrica aparecem.
- **AC-4** Dado o termo "pintor" sem acento e um prestador com "Pintura" no título, quando busca, então ele aparece.
- **AC-5** Dado `cityIbgeCode` sem coordenadas, quando busca, então os resultados vêm ordenados por relevância, sem `distanceKm`.
- **AC-6** Dado `sort=distance` sem coordenadas, ou com coordenadas de outra cidade e `cityIbgeCode`, quando busca, então recebe 400.
- **AC-7** Dado `minRating=4.5`, quando busca, então nenhum resultado tem `rating` abaixo de 4,5 nem nulo.
- **AC-8** Dado um prestador pausado, ou com a conta suspensa, quando qualquer busca é feita ou a Início é aberta, então ele não aparece.
- **AC-9** Dado `page=11`, quando busca, então recebe 400.
- **AC-10** Dada qualquer resposta da busca, da Início ou dos favoritos, quando inspecionada, então não contém telefone, endereço nem coordenadas.
- **AC-11** Dado um cliente que excede 60 buscas por minuto, quando faz a seguinte, então recebe 429 com `Retry-After`.
- **AC-12** Dado um prestador com cidade base São Paulo e raio de 5 km a 15 km do centroide, quando o cliente busca sem GPS com `cityIbgeCode` de São Paulo, então ele aparece.
- **AC-13** Dado um prestador de cidade vizinha cujo raio, a partir do ponto público, alcança o limite da cidade escolhida, quando o cliente busca sem GPS nessa cidade, então ele aparece.
- **AC-14** Dado um prestador com serviços só "a partir de R$ 15/m²", quando aparece na busca, então o card traz o valor com a unidade, ele não entra em `maxPriceFrom` e, com `sort=price`, vem depois dos que têm preço por serviço.
- **AC-15** Dado `maxDistanceKm=10`, quando busca com GPS, então nenhum resultado traz `distanceKm` acima de 10.
- **AC-16** Dado um ponto com `lat/lng` e um `cityIbgeCode` de outra cidade, quando busca, então `context.mode` é `city` e nenhum resultado traz `distanceKm`.
- **AC-17** Dados dois prestadores iguais, um com "chuveiro" no título de um serviço e outro só no "Sobre", quando o cliente busca "chuveiro", então os dois aparecem e o do título vem primeiro.
- **AC-18** Dado `sort=rating`, quando busca, então os prestadores com nota vêm primeiro, da maior para a menor, e os sem nota depois.
- **AC-19** Dado um prestador publicado, quando ele muda a profissão, então a busca pelo termo novo o encontra na requisição seguinte.

### Início

- **AC-20** Dados prestadores com nota e sem nota que atendem um ponto, quando o cliente abre a Início nesse ponto, então `topRated` traz só os com nota, da maior para a menor, e `hasProviders` é true.
- **AC-21** Dada uma cidade sem prestadores, quando o cliente abre a Início nela, então `topRated` vem vazio, `hasProviders` é false e `city` vem preenchida.

### Favoritos

- **AC-22** Dado um usuário autenticado, quando favorita e depois lista favoritos, então o prestador aparece; quando remove, some.
- **AC-23** Dado um usuário com vitrine, quando tenta favoritar a própria vitrine, então recebe 404; dada uma vitrine pausada, também 404.
- **AC-24** Dado um usuário com 200 favoritos, quando salva mais um, então recebe 409.
- **AC-25** Dado um favorito cuja vitrine é pausada, quando o usuário lista, então ela não aparece; quando é publicada de novo, volta.
- **AC-26** Dado um prestador salvo, quando o dono do favorito consulta o perfil público com o token, então `isFavorite` é true; sem token, false.
- **AC-27** Dada uma conta com favoritos, quando é excluída, então os favoritos dela deixam de existir no banco.
- **AC-28** Sem token, `GET /me/favorites`, `PUT` e `DELETE` respondem 401; o favorito de outra conta nunca aparece na lista de quem chama.

### App

- **AC-29** Dado um visitante sem login no perfil público, quando toca no coração, então o app abre Entrar e, depois do login, volta ao perfil com o prestador salvo.
- **AC-30** Dada a Início, quando o cliente toca em "Ver disponíveis", então a Busca abre com o chip "Atende hoje" ligado.

## Deferred Decisions

| Decisão | Fallback escolhido agora | Revisitar quando |
| --- | --- | --- |
| Motor de busca dedicado | PostgreSQL full-text + trigram | p95 acima de 400 ms ou mais de 200 mil prestadores |
| Personalização por histórico | Relevância igual para todos | Depois do MVP, com dados de uso |
| Número de resultados na Busca | Sem total; linha "Profissionais perto de você" | Pesquisa com usuários pedir o número |
| Reindexar vitrines ao renomear categoria | Categorias fixas por migração; o índice usa o nome vigente quando a vitrine muda | Spec 008 (edição de categorias pelo admin) |
| Limite de favoritos | 200 por conta | Reclamação de quem chegar ao limite |

## Open Questions

Nenhuma. "Disponíveis agora" considera só o botão "Disponível hoje", como em [02-regras-de-negocio.md §3](../02-regras-de-negocio.md), e na Início é só o card que abre a Busca filtrada.
