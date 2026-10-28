# Spec 004 — Busca de prestadores

- **Status:** Draft
- **Data:** 2026-10-28
- **Responsável:** Wellington

## Problem

O cliente precisa achar rápido quem atende onde ele está (ou na cidade que escolheu), comparar nota, distância e preço de referência, e chegar ao perfil certo. A busca tem de funcionar com e sem GPS, tolerar acentos e erros de digitação e resistir a raspagem.

## Scope

### In

- Busca por texto (profissão, serviço, nome) com tolerância a acento e pequenos erros.
- Filtro por categoria, distância máxima, nota mínima, MEI verificado, disponível hoje, faixa de preço "a partir de" e cidade.
- Ordenação: relevância (padrão), mais perto (só com GPS), melhor avaliado, menor preço.
- Contexto de localização da spec 002: ponto do GPS ou cidade escolhida.
- Início: categorias, "Disponíveis agora", "Bem avaliados perto de você".
- Favoritos: salvar, remover, listar.

### Out

- Mapa, buscas recentes, anúncios patrocinados.
- Busca por disponibilidade em horário específico.

## Domain Model

Sem agregados novos. Usa `Provider` (spec 003), `City` (spec 002) e:

- `Favorite`: `UserId`, `ProviderId`, `CreatedAt` (único por par).
- Projeção de busca (leitura): `ProviderSearchItem` com `Id`, `DisplayName`, `Headline`, `Initials`, `PhotoUrl?`, `RatingScore?` (nulo com menos de 3 avaliações), `ReviewCount`, `Neighborhood`, `CityName`, `DistanceKm?` (arredondada; só com GPS), `MeiVerified`, `AvailableToday`, `ResponseTimeBucket?`, `PriceFrom?`.
- Relevância: combinação de correspondência de texto, nota bayesiana, número de avaliações (log), completude do perfil, disponibilidade hoje e, com GPS, proximidade. Pesos configuráveis, sem patrocínio no MVP.

## API Contract

| Verbo | Rota | Request | Response | Erros |
| --- | --- | --- | --- | --- |
| GET | `/api/v1/providers` | `q?`, `categoryId?`, `lat?`, `lng?`, `cityIbgeCode?`, `maxDistanceKm?` (1–50), `minRating?` (3–5), `meiVerified?`, `availableToday?`, `maxPriceFrom?`, `sort?` (`relevance`, `distance`, `rating`, `price`), `page?` (1–10), `pageSize?` (≤ 30) | `{ items: [ProviderSearchItem], page, pageSize, hasMore, context: { mode, city } }` | 400 (ex.: `sort=distance` sem coordenadas, nem `lat/lng` nem `cityIbgeCode`), 429 |
| GET | `/api/v1/home?lat=&lng=&cityIbgeCode=` | contexto de localização | `{ categories, availableNow: [..10], topRated: [..10], city }` | 400, 429 |
| GET | `/api/v1/me/favorites?page=` | — | `{ items: [ProviderSearchItem], hasMore }` | 401 |
| PUT | `/api/v1/me/favorites/{providerId}` | — | 204 | 401, 404 |
| DELETE | `/api/v1/me/favorites/{providerId}` | — | 204 | 401 |

## Authorization

- `GET /providers` e `/home`: anônimos, política `search`.
- Favoritos: autenticado, filtrado pelo usuário.
- Respostas da busca nunca têm telefone, endereço ou coordenadas.

## Edge Cases & Failure Modes

- Sem `lat/lng` e sem `cityIbgeCode`: 400 (o app sempre tem um dos dois).
- Com `lat/lng` e `cityIbgeCode` de outra cidade: vale a cidade escolhida como centro (centroide); a distância não é exibida.
- Termo com erro ("eletrisista"): o trigram encontra "eletricista".
- Termo só com stopwords ou vazio após normalização: busca só pelos filtros.
- Cidade sem prestadores: lista vazia + `context.city` para o app mostrar o estado vazio com convite.
- `page` > 10: 400 (anti-raspagem).
- Prestador pausado ou suspenso: nunca aparece; favoritos dele somem da lista (mantidos no banco).
- Nota com menos de 3 avaliações: `ratingScore` nulo e o app mostra "Novo no Vitrio".
- Mesmo cliente buscando muitas vezes por segundo: 429 com `Retry-After`.

## Non-Functional Requirements

- p95 ≤ 400 ms para buscas com GPS e texto, com 50 mil prestadores ativos.
- Índices: GiST em `base_location`, GIN em `search_vector`, trigram em nome normalizado; consulta com pré-filtro de 50 km.
- Cache de 60 s para `/home` por célula de ~2 km e por cidade.
- Distância calculada a partir do ponto público e arredondada (regras em [02-regras-de-negocio.md §2](../02-regras-de-negocio.md)).

## Integrations

Nenhuma externa. Usa PostGIS, `unaccent` e `pg_trgm`.

## Acceptance Criteria

- **AC-1** Dados prestadores publicados a 2 km e a 12 km de um ponto, ambos com raio de 15 km, quando o cliente busca com esse ponto e `sort=distance`, então o de 2 km vem primeiro, a distância do primeiro vem com uma casa decimal e a do segundo como inteiro (calculadas a partir do ponto público).
- **AC-2** Dado um prestador a 8 km com raio de 5 km, quando o cliente busca a partir desse ponto, então ele não aparece.
- **AC-3** Dado o termo "eletrisista", quando o cliente busca, então prestadores de Elétrica aparecem.
- **AC-4** Dado o termo "pintor" sem acento e um prestador com "Pintura" no título, quando busca, então ele aparece.
- **AC-5** Dado `cityIbgeCode` sem coordenadas, quando busca, então os resultados vêm ordenados por relevância, sem `distanceKm`.
- **AC-6** Dado `sort=distance` sem coordenadas, quando busca, então recebe 400.
- **AC-7** Dado `minRating=4.5`, quando busca, então nenhum resultado tem `ratingScore` abaixo de 4,5 nem nulo.
- **AC-8** Dado um prestador pausado, quando qualquer busca é feita, então ele não aparece.
- **AC-9** Dado `page=11`, quando busca, então recebe 400.
- **AC-10** Dada qualquer resposta da busca, quando inspecionada, então não contém telefone, endereço nem coordenadas.
- **AC-11** Dado um usuário autenticado, quando favorita e depois lista favoritos, então o prestador aparece; quando remove, some.
- **AC-12** Dado um cliente que excede 60 buscas por minuto, quando faz a seguinte, então recebe 429 com `Retry-After`.

## Deferred Decisions

| Decisão | Fallback escolhido agora | Revisitar quando |
| --- | --- | --- |
| Motor de busca dedicado | PostgreSQL full-text + trigram | p95 acima de 400 ms ou mais de 200 mil prestadores |
| Personalização por histórico | Relevância igual para todos | Depois do MVP, com dados de uso |

## Open Questions

- Pesos iniciais da relevância (proposta: texto 0,35; nota 0,25; volume de avaliações 0,15; proximidade 0,15; completude 0,05; disponível hoje 0,05).
- "Disponíveis agora" deve considerar a faixa de horário do dia, além do botão "Disponível hoje"?
