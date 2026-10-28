# Spec 003 — Vitrine do prestador

- **Status:** Draft
- **Data:** 2026-10-28
- **Responsável:** Wellington

## Problem

Autônomos e MEIs precisam de uma vitrine que inspire confiança e traga pedidos: quem são, o que fazem, onde atendem, quanto cobram, trabalhos anteriores e avaliações. O cadastro tem de ser simples para quem tem pouca familiaridade com apps e proteger o endereço de quem trabalha em casa.

## Scope

### In

- Criar vitrine (rascunho) a partir da conta: passa a ter papel `provider`.
- Onboarding em 4 passos: Sobre você; Monte sua vitrine (categorias, área de atendimento, WhatsApp, MEI); Fotos e preços; Revisar e publicar.
- Categorias (até 3) a partir da lista fixa.
- Área de atendimento: endereço base por CEP (privado), raio 1–50 km, bairros atendidos (texto normalizado).
- Serviços (até 20) com preço "a partir de" ou "Sob orçamento".
- Fotos de trabalhos (até 30), com legenda, reordenação e exclusão.
- Disponibilidade semanal (dias e faixas) e "Atende emergências".
- "Disponível hoje" (liga/desliga; vale só no dia em que foi ligado, no fuso da cidade base, calculado na leitura).
- Verificação MEI por CNPJ (selo) e revalidação a cada 30 dias.
- Publicar, pausar e editar.
- Perfil público (`GET /providers/{id}`) e painel inicial do prestador (contadores).
- Selos e números do perfil conforme [02-regras-de-negocio.md §3](../02-regras-de-negocio.md): MEI verificado, Disponível hoje, Responde em ~X, serviços feitos pelo app.
- Link para compartilhar o perfil: `https://vitrio.app/p/{id}`, aberto pelo app via Android App Links (`assetlinks.json` em `vitrio.app`); sem o app, uma página estática com nome, profissão, nota e o botão da Google Play.

### Out

- Avaliações na vitrine (spec 007), busca (spec 004), pedidos (spec 005).
- Métricas de desempenho detalhadas, plano Pro, link próprio.

## Domain Model

- `Provider` (agregado): `Id`, `UserId` (único), `DisplayName`, `Headline` (profissão, até 60), `About` (40–600), `YearsOfExperience?`, `PhotoKey?`, `ContactPhone` (E.164, criptografado), `WhatsAppEnabled`, `Status` (`Draft`, `Published`, `Paused`, `Suspended`, `Removed`), `CityIbgeCode`, `BaseAddress` (criptografado), `BaseLocation` (privado, nunca usado em filtro nem distância), `PublicLocation` (centro da célula de ~500 m; usado em todo filtro e distância), `ServiceRadiusMeters` (1.000–50.000), `AvailableToday`, `AvailableTodaySetAt?`, `HandlesEmergencies`, `RatingScore`, `ReviewCount`, `CompletedOrders`, `ResponseTimeBucket?`, `ResponseSampleSize`, `LateCancellations90d`, `SearchVector`.
- Campos derivados (`RatingScore`, `CompletedOrders`, `ResponseTimeBucket`, `LateCancellations90d`) só mudam por handler ou job, sempre recalculados do zero, com as regras de [02-regras-de-negocio.md §3 e §4](../02-regras-de-negocio.md).
- `ProviderCategory`: `ProviderId`, `CategoryId` (até 3; uma marcada como principal).
- `ProviderService`: `Id`, `Title` (até 60), `Description?` (até 140), `PriceFrom?` (`Money`), `PriceUnit` (`PerService`, `PerHour`, `PerDay`, `PerSquareMeter`, `PerUnit`), `OnQuote` (bool), `SortOrder`.
- `ServedNeighborhood`: `ProviderId`, `Name`, `NameNormalized`.
- `AvailabilitySlot`: `DayOfWeek`, `Start`, `End`.
- `PortfolioPhoto`: `Id`, `ProviderId`, `ObjectKey`, `ThumbnailKey`, `Caption?` (até 60), `SortOrder`, `Status` (`Processing`, `Active`, `Rejected`, `Hidden`).
- `MeiVerification`: `ProviderId`, `Cnpj` (único entre vitrines), `Status` (`Verified`, `Pending`, `Failed`, `Lost`), `CheckedAt`, `CompanyName`, `Reason?`.
- `Category` (referência): `Id`, `Slug`, `Name`, `Icon`, `SortOrder`, `Active`.
- Regras de publicação: [02-regras-de-negocio.md §3](../02-regras-de-negocio.md).

## API Contract

| Verbo | Rota | Request | Response | Erros |
| --- | --- | --- | --- | --- |
| GET | `/api/v1/categories` | — | `[{ id, slug, name, icon }]` | — |
| POST | `/api/v1/provider` | `{ displayName, headline }` | 201 `providerDraft` (o app chama `POST /auth/refresh` em seguida para o token trazer o papel `provider`) | 400, 401, 409 já existe |
| GET | `/api/v1/provider` | — | `providerPrivate` (inclui endereço base e checklist de publicação) | 401, 404 |
| PATCH | `/api/v1/provider` | `{ displayName?, headline?, about?, yearsOfExperience?, photoKey?, contactPhone?, whatsAppEnabled?, handlesEmergencies? }` | `providerPrivate` | 400, 401, 409 concorrência |
| PUT | `/api/v1/provider/categories` | `{ categoryIds: [..3], primaryCategoryId }` | 204 | 400, 401 |
| PUT | `/api/v1/provider/service-area` | `{ cep, number, complement?, radiusKm, neighborhoods: [..30] }` | `{ city, publicArea }` | 400 CEP, 401, 503 CEP indisponível |
| PUT | `/api/v1/provider/services` | `{ services: [..20] }` | 204 | 400, 401 |
| PUT | `/api/v1/provider/availability` | `{ slots: [{ dayOfWeek, start, end }] }` | 204 | 400 sobreposição, 401 |
| PUT | `/api/v1/provider/available-today` | `{ value: bool }` | 204 | 401, 409 vitrine não publicada |
| POST | `/api/v1/provider/photos/upload-url` | `{ contentType, contentLength }` | `{ uploadUrl, objectKey, expiresAt }` | 400 tipo/tamanho, 401, 409 limite de 30, 429 |
| POST | `/api/v1/provider/photos` | `{ objectKey, caption? }` | 202 `{ id, status: "Processing" }` | 400, 401, 404 objeto não encontrado |
| PATCH | `/api/v1/provider/photos/{id}` | `{ caption }` | 204 | 400, 401, 404 |
| PUT | `/api/v1/provider/photos/order` | `{ ids: [...] }` | 204 | 400, 401 |
| DELETE | `/api/v1/provider/photos/{id}` | — | 204 | 401, 404 |
| POST | `/api/v1/provider/mei-verification` | `{ cnpj }` | `{ status, companyName?, reason? }` | 400 dígito verificador, 401, 409 CNPJ já vinculado, 503 consulta indisponível (status `Pending`) |
| POST | `/api/v1/provider/publish` | — | `{ status: "Published" }` | 401, 422 `{ missing: [...] }` |
| POST | `/api/v1/provider/pause` | — | `{ status: "Paused" }` | 401, 409 |
| GET | `/api/v1/provider/dashboard` | — | `{ newOrders, ratingScore, reviewCount, completedThisMonth, profileCompleteness, nextStep }` | 401, 404 |
| GET | `/api/v1/providers/{id}` | — | `providerPublic` (sem endereço, sem coordenadas, telefone só se WhatsApp ativo, distância se `lat,lng` informados) | 404 (inexistente, rascunho, pausado ou suspenso) |
| GET | `/api/v1/providers/{id}/photos?page=` | — | `{ items, hasMore }` | 404 |

## Authorization

- Rotas `/provider/*`: autenticado; política `Provider` (vitrine em `Draft`, `Published` ou `Paused`) exceto `POST /provider` (qualquer usuário sem vitrine). Tudo filtrado pelo `UserId` do token. Vitrine pausada continua gerenciável, inclusive para despausar e atender pedidos em andamento.
- `GET /providers/{id}` e fotos: anônimo, política `profile`.
- Status, nota, contadores, selo e `PublicLocation` nunca vêm do request.
- Endereço base e `BaseLocation` só em `GET /provider` do próprio dono; nunca em respostas públicas nem em logs.

## Edge Cases & Failure Modes

- Publicar sem campos mínimos: 422 com a lista do que falta (`about`, `categories`, `serviceArea`, `services`, `contactPhone`).
- CNPJ válido mas baixado, inapto ou não optante pelo MEI: `Failed` com motivo legível ("CNPJ não é optante pelo MEI").
- CNPJ já usado por outra vitrine: 409; denúncia possível pelo dono legítimo.
- BrasilAPI fora do ar: `Pending`; job tenta de novo a cada 6 h por até 3 dias. Depois disso, `Failed` com motivo "Não conseguimos consultar o CNPJ agora", push ao prestador (`MeiVerificationFailed`) e opção de tentar de novo.
- Revalidação perde o selo: `Lost`, push ao prestador, selo some da busca e do perfil na hora (cache invalidado).
- Qualquer mudança no prestador vinda de job ou evento (selo, nota, tempo de resposta, serviços feitos, foto ocultada, suspensão) invalida o perfil público pelo ponto único de [09-cache.md §3](../09-cache.md).
- Upload confirmado mas arquivo inválido (não é imagem, > 10 MB, > 8000 px): foto vai para `Rejected` e o app mostra o motivo.
- Foto com EXIF de GPS: metadados removidos antes de publicar.
- Raio que cobre várias cidades: permitido; a cidade base continua a do endereço.
- Prestador pausado com pedidos em aberto: pedidos continuam; novos pedidos bloqueados.
- Edição concorrente em dois aparelhos: 409 por `xmin`; o app recarrega.
- Texto "Sobre" com telefone, e-mail ou link: 400 com mensagem ("Use o botão de WhatsApp para contato").

## Non-Functional Requirements

- `GET /providers/{id}` p95 ≤ 150 ms com `HybridCache` (5 min no Redis, 30 s local, invalidado por qualquer mudança no prestador; "Disponível hoje" e distância calculados fora do cache). Ver [09-cache.md](../09-cache.md).
- Fotos públicas servidas por CDN; miniatura ≤ 60 KB.
- Ponto público recalculado sempre que o endereço base muda; auditoria de mudança de endereço base.
- Revalidação MEI em lotes de 200 com intervalo para respeitar a BrasilAPI.

## Integrations

- BrasilAPI CNPJ v1 (`situacao_cadastral`/`descricao_situacao_cadastral`, `opcao_pelo_mei`, `razao_social`).
- BrasilAPI CEP v2 (endereço base e geocodificação aproximada).
- Storage S3 compatível (URLs assinadas) e job de processamento de imagens (SkiaSharp).

## Acceptance Criteria

- **AC-1** Dado um usuário sem vitrine, quando cria a vitrine, então ela nasce em `Draft` e o usuário ganha o papel `provider`.
- **AC-2** Dada uma vitrine sem "Sobre" e sem serviços, quando tenta publicar, então recebe 422 com `missing` contendo `about` e `services`.
- **AC-3** Dada uma vitrine com todos os campos mínimos, quando publica, então o status vira `Published` e ela aparece em `GET /providers/{id}`.
- **AC-4** Dado um CNPJ ativo e optante pelo MEI, quando o prestador verifica, então o status é `Verified` e o perfil público mostra o selo "MEI verificado".
- **AC-5** Dado um CNPJ não optante pelo MEI, quando verifica, então o status é `Failed` com motivo legível e nenhum selo aparece.
- **AC-6** Dado um CNPJ já vinculado a outra vitrine, quando verifica, então recebe 409.
- **AC-7** Dada uma vitrine publicada, quando qualquer pessoa consulta o perfil público, então a resposta não contém endereço base nem coordenadas, e a distância (se informada a localização) é arredondada.
- **AC-8** Dada uma foto JPEG com GPS no EXIF, quando é processada, então a versão publicada não contém metadados de localização.
- **AC-9** Dada uma vitrine com 30 fotos, quando pede nova URL de upload, então recebe 409.
- **AC-10** Dado "Disponível hoje" ligado ontem no fuso da cidade base, quando alguém consulta o perfil ou a busca hoje, então o selo não aparece, sem depender de nenhum job.
- **AC-11** Dado o id de foto de outra vitrine, quando o prestador tenta apagar, então recebe 404.
- **AC-12** Dada uma vitrine pausada, quando alguém consulta o perfil público, então recebe 404.
- **AC-13** Dado um texto "Sobre" com número de telefone, quando salva, então recebe 400 explicando que contato é pelo botão de WhatsApp.
- **AC-14** Dada uma vitrine pausada, quando o prestador chama `POST /provider/publish`, então ela volta a `Published` (a política `Provider` aceita vitrine pausada).
- **AC-15** Dado um selo MEI que a revalidação marca como `Lost`, quando o perfil público é consultado logo depois, então o selo não aparece.
- **AC-16** Dada uma verificação MEI `Pending` há mais de 3 dias, quando o job roda, então ela vira `Failed` com motivo e o prestador recebe aviso.
- **AC-17** Dado o link `https://vitrio.app/p/{id}` de uma vitrine publicada, quando aberto num Android com o app, então o app abre no perfil; sem o app, a página mostra o perfil resumido e o botão da Google Play.

## Deferred Decisions

| Decisão | Fallback escolhido agora | Revisitar quando |
| --- | --- | --- |
| Moderação prévia de fotos | Publicação imediata com denúncia e ocultação automática | Denúncias de foto passarem de 1% das fotos |
| Vídeos na vitrine | Só fotos | Pedido recorrente dos prestadores |
| Conferir nome do titular do CNPJ com o nome da conta | Só mostra a razão social ao prestador na confirmação | Casos de uso indevido de CNPJ |

## Open Questions

- Lista inicial de categorias (proposta: Elétrica, Hidráulica, Pedreiro, Pintura, Diarista, Jardinagem, Montagem de móveis, Ar-condicionado, Marido de aluguel, Fretes e mudanças, Beleza a domicílio, Aulas particulares, Informática, Cuidador(a), Pet).
- Prestador sem CNPJ e sem telefone de WhatsApp pode publicar? (proposta: sim, telefone é obrigatório mas WhatsApp é opcional)
