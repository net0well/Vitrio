# Spec 003 — Vitrine do prestador

- **Status:** Approved (2026-10-29)
- **Data:** 2026-10-28 (revisada em 2026-10-29)
- **Responsável:** Wellington

## Problem

Autônomos e MEIs precisam de uma vitrine que inspire confiança e traga pedidos: quem são, o que fazem, onde atendem, quanto cobram, trabalhos anteriores e avaliações. O cadastro tem de ser simples para quem tem pouca familiaridade com apps e proteger o endereço de quem trabalha em casa.

## Scope

### In

- Criar vitrine (rascunho) a partir da conta: a conta passa a ter o papel `provider`.
- Onboarding em 4 passos, cada um salvo ao avançar: Sobre você (foto opcional, nome de exibição, profissão, "Sobre", anos de experiência); Monte sua vitrine (categorias, área de atendimento, telefone e WhatsApp, MEI); Fotos e preços; Revisar e publicar.
- Categorias (até 3, uma principal) da lista fixa inicial: Elétrica, Hidráulica, Pedreiro, Pintura, Diarista, Jardinagem, Montagem de móveis, Ar-condicionado, Marido de aluguel, Fretes e mudanças, Beleza a domicílio, Aulas particulares, Informática, Cuidador(a), Pet. Carregada por migração; mudanças depois só por admin (spec 008).
- Área de atendimento: endereço base por CEP (privado), raio de 1 a 50 km, até 30 bairros atendidos (texto normalizado).
- Telefone de contato obrigatório, pré-preenchido com o celular da conta e editável; botão de WhatsApp opcional.
- Serviços (até 20) com preço "a partir de" ou "Sob orçamento".
- Foto de perfil e fotos de trabalhos (até 30), com legenda, reordenação e exclusão.
- Disponibilidade semanal (dias e faixas) e "Atende emergências".
- "Disponível hoje" (liga e desliga; vale só no dia em que foi ligado, no fuso da cidade base, calculado na leitura).
- Verificação MEI por CNPJ (selo), nova tentativa automática quando a consulta falha e revalidação a cada 30 dias.
- Publicar, pausar e editar.
- Perfil público (`GET /providers/{id}`) e Painel do prestador com o que já existe nesta spec.
- Selos e números do perfil conforme [02-regras-de-negocio.md §3](../02-regras-de-negocio.md): MEI verificado e Disponível hoje são calculados aqui; "Responde em ~X", nota e serviços feitos aparecem quando as specs 005 e 007 alimentarem os números (até lá, "Novo no Vitrio").
- Link para compartilhar o perfil, `https://vitrio.app/p/{id}`, servido pela API:
  - página HTML simples com nome, profissão, cidade, nota ou "Novo no Vitrio" e o botão da Google Play;
  - tags Open Graph para a prévia no WhatsApp;
  - `/.well-known/assetlinks.json` montado pela configuração, para o Android abrir o app (App Links).
- Fila de jobs com Hangfire (processamento de imagem, novas tentativas e revalidação do MEI).
- Storage S3 compatível: R2 em produção, SeaweedFS no `docker-compose` local e nos testes.
- Remoção da vitrine quando a conta é excluída (consumidor de `user.deleted` da spec 001).
- App:
  - modo prestador, com "Quero oferecer meus serviços" nas boas-vindas e "Entrar como prestador" / "Voltar para cliente" em Conta;
  - abas Painel e Meu perfil (Pedidos e Agenda chegam com a spec 005);
  - onboarding e edição do perfil;
  - tela do perfil público, com o botão de WhatsApp quando ativo;
  - compartilhar o perfil e abrir o app pelo link.

### Out

- Avaliações na vitrine (spec 007), busca e `SearchVector` (spec 004), pedidos, botão "Pedir orçamento" e abas Pedidos e Agenda (spec 005).
- Avisos por push do selo MEI: os eventos saem nesta spec e o handler `AccountNotifications` chega com a spec 009. Até lá o Painel mostra o status do selo.
- Suspensão pela moderação, ocultação de fotos e edição de categorias pelo admin (spec 008).
- Métricas de desempenho detalhadas, plano Pro e link próprio.

## Domain Model

- `Provider` (agregado):
  - identificação: `Id`, `UserId` (único entre vitrines não `Removed`), `DisplayName` (2–60), `Headline` (profissão, até 60), `About` (40–600), `YearsOfExperience?` (0–60), `AvatarKey?`;
  - contato: `ContactPhone` (E.164, celular brasileiro, criptografado), `WhatsAppEnabled`;
  - estado: `Status` (`Draft`, `Published`, `Paused`, `Suspended`, `Removed`), `PublishedAt?`;
  - área: `CityIbgeCode`, `BaseAddress` (rua, número, complemento, bairro e CEP, criptografado), `BaseLocation` (privado, nunca usado em filtro nem distância), `PublicLocation` (centro da célula de ~500 m; usado em todo filtro e distância), `LocationApproximate` (true quando o CEP veio sem ponto e o ponto base é o ponto interno da cidade), `ServiceRadiusMeters` (1.000–50.000);
  - disponibilidade: `AvailableToday`, `AvailableTodaySetAt?`, `HandlesEmergencies`;
  - números derivados: `RatingScore`, `ReviewCount`, `CompletedOrders`, `ResponseTimeBucket?`, `ResponseSampleSize`, `LateCancellations90d`. Nascem zerados ou nulos nesta spec.
- Campos derivados só mudam por handler ou job, sempre recalculados do zero, com as regras de [02-regras-de-negocio.md §3 e §4](../02-regras-de-negocio.md).
- `ProviderCategory`: `ProviderId`, `CategoryId`, `IsPrimary` (até 3; exatamente uma principal).
- `ProviderService`: `Id`, `Title` (até 60), `Description?` (até 140), `PriceFrom?` (`Money`, de R$ 1,00 a R$ 100.000,00), `PriceUnit` (`PerService`, `PerHour`, `PerDay`, `PerSquareMeter`, `PerUnit`), `OnQuote` (bool; quando true, sem preço), `SortOrder`.
- `ServedNeighborhood`: `ProviderId`, `Name` (2–60), `NameNormalized` (único por vitrine).
- `AvailabilitySlot`: `DayOfWeek`, `Start`, `End` (horário local da cidade base, múltiplos de 30 min, `Start < End`, até 3 faixas por dia, sem sobreposição).
- `PortfolioPhoto`: `Id`, `ProviderId`, `ObjectKey`, `ThumbnailKey`, `Caption?` (até 60), `SortOrder`, `Status` (`Processing`, `Active`, `Rejected`, `Hidden`), `RejectionReason?`.
- `MeiVerification`: `ProviderId`, `Cnpj` (único entre vitrines não `Removed`), `Status` (`Verified`, `Pending`, `Failed`, `Lost`), `CheckedAt`, `PendingSince?`, `CompanyName?`, `ReasonCode?`.
- `Category` (referência): `Id`, `Slug`, `Name`, `Icon`, `SortOrder`, `Active`.
- Regras de publicação: [02-regras-de-negocio.md §3](../02-regras-de-negocio.md). A checklist de publicação é calculada pelo agregado e devolvida em `GET /provider`.

## API Contract

| Verbo | Rota | Request | Response | Erros |
| --- | --- | --- | --- | --- |
| GET | `/api/v1/categories` | — | `[{ id, slug, name, icon }]` (só ativas, na ordem) | — |
| POST | `/api/v1/provider` | `{ displayName, headline }` | 201 `providerPrivate` (o app chama `POST /auth/refresh` em seguida para o token trazer o papel `provider`) | 400, 401, 409 já existe |
| GET | `/api/v1/provider` | — | `providerPrivate` (inclui endereço base, telefone, status do selo MEI e `checklist`) | 401, 404 |
| PATCH | `/api/v1/provider` | `{ displayName?, headline?, about?, yearsOfExperience?, contactPhone?, whatsAppEnabled?, handlesEmergencies? }` | `providerPrivate` | 400, 401, 409 concorrência |
| PUT | `/api/v1/provider/avatar` | `{ objectKey }` | 202 | 400, 401, 404 objeto não encontrado |
| DELETE | `/api/v1/provider/avatar` | — | 204 | 401 |
| PUT | `/api/v1/provider/categories` | `{ categoryIds: [..3], primaryCategoryId }` | 204 | 400, 401 |
| PUT | `/api/v1/provider/service-area` | `{ cep, number, complement?, radiusKm, neighborhoods: [..30] }` | `{ city, neighborhood, locationApproximate }` | 400 CEP ou cidade, 401, 503 CEP indisponível |
| PUT | `/api/v1/provider/services` | `{ services: [..20] }` | 204 | 400, 401 |
| PUT | `/api/v1/provider/availability` | `{ slots: [{ dayOfWeek, start, end }], handlesEmergencies }` | 204 | 400 sobreposição, 401 |
| PUT | `/api/v1/provider/available-today` | `{ value: bool }` | 204 | 401, 409 vitrine não publicada |
| POST | `/api/v1/provider/photos/upload-url` | `{ purpose: "Portfolio" \| "Avatar", contentType, contentLength }` | `{ uploadUrl, objectKey, expiresAt }` | 400 tipo ou tamanho, 401, 409 limite de 30, 429, 503 storage indisponível |
| POST | `/api/v1/provider/photos` | `{ objectKey, caption? }` | 202 `{ id, status: "Processing" }` | 400, 401, 404 objeto não encontrado, 409 limite de 30 |
| GET | `/api/v1/provider/photos` | — | `[{ id, url, thumbnailUrl, caption, status, rejectionReason }]` (todas, inclusive em processamento e rejeitadas) | 401 |
| PATCH | `/api/v1/provider/photos/{id}` | `{ caption }` | 204 | 400, 401, 404 |
| PUT | `/api/v1/provider/photos/order` | `{ ids: [...] }` | 204 | 400, 401 |
| DELETE | `/api/v1/provider/photos/{id}` | — | 204 | 401, 404 |
| POST | `/api/v1/provider/mei-verification` | `{ cnpj }` | `{ status, companyName?, reason? }` | 400 dígito verificador, 401, 409 CNPJ já vinculado; consulta indisponível devolve 202 com `status: "Pending"` |
| POST | `/api/v1/provider/publish` | — | `{ status: "Published" }` | 401, 422 `{ missing: [...] }` |
| POST | `/api/v1/provider/pause` | — | `{ status: "Paused" }` | 401, 409 vitrine não publicada |
| GET | `/api/v1/provider/dashboard` | — | `{ status, profileCompleteness, nextStep, meiStatus, availableToday }` (números de pedidos e avaliações entram com as specs 005 e 007) | 401, 404 |
| GET | `/api/v1/providers/{id}` | `?lat=&lng=` opcionais | `providerPublic` | 404 (inexistente, rascunho, pausada, suspensa ou removida), 400 ponto fora do Brasil |
| GET | `/api/v1/providers/{id}/photos?page=` | — | `{ items: [{ id, url, thumbnailUrl, caption }], hasMore }` (12 por página, só `Active`) | 404 |
| GET | `/p/{id}` | — | HTML da vitrine publicada com Open Graph; página "Perfil não disponível" com 404 nos outros casos | — |
| GET | `/.well-known/assetlinks.json` | — | Declaração do App Links com o pacote e as impressões digitais da configuração | — |

`providerPublic`: `id`, `displayName`, `headline`, `about`, `yearsOfExperience`, `avatarUrl`, `city { ibgeCode, name, state }`, `neighborhoods`, `radiusKm`, `categories` (principal primeiro), `services`, `availability`, `handlesEmergencies`, `badges { meiVerified, availableToday }`, `rating { score, count }` ou `isNew`, `responseTime?`, `completedOrders?` (só a partir de 5), `whatsAppPhone` (só com WhatsApp ativo), `distanceKm` (só com `lat` e `lng`, arredondada pela regra de [02 §2](../02-regras-de-negocio.md)), `firstPhotos` (até 6), `shareUrl`. Nunca contém endereço base, coordenadas, CNPJ nem razão social.

## Authorization

- Rotas `/provider/*`: autenticado, filtradas pelo `UserId` do token.
  - `POST /provider` vale para qualquer conta sem vitrine ativa.
  - As demais exigem a política `Provider`: papel `provider`, que a conta tem enquanto houver vitrine em `Draft`, `Published` ou `Paused`.
  - Vitrine pausada continua gerenciável, inclusive para despausar.
- `GET /providers/{id}`, fotos públicas, `/p/{id}` e `assetlinks.json`: anônimos, política de rate limit `profile`.
- Rate limit: `write` (60/min por usuário) nas escritas de `/provider/*`; `uploads` (60/h por usuário) em `upload-url`.
- Status, nota, contadores, selo, `PublicLocation` e `LocationApproximate` nunca vêm do request.
- Endereço base, `BaseLocation`, CNPJ e razão social só em `GET /provider` do próprio dono; nunca em respostas públicas nem em logs.

## Edge Cases & Failure Modes

- Publicar sem campos mínimos: 422 com a lista do que falta (`displayName`, `about`, `categories`, `serviceArea`, `services`, `contactPhone`).
- CEP base sem coordenadas na BrasilAPI: o ponto base é o ponto interno da cidade (tabela `cities` da spec 002), `LocationApproximate` fica true e o app avisa que a área é aproximada.
- CEP de cidade fora da tabela `cities` ou de UF inválida: 400 `address.city_not_found`. CEP inexistente: 400 `address.cep_not_found` (a vitrine exige o CEP; não há endereço digitado à mão). BrasilAPI fora do ar: 503 `address.cep_unavailable`; nada é salvo.
- CNPJ válido mas baixado, inapto ou não optante pelo MEI: `Failed` com motivo legível ("CNPJ não é optante pelo MEI"). CNPJ inexistente na consulta: `Failed` "CNPJ não encontrado".
- CNPJ já usado por outra vitrine não `Removed`: 409; o dono legítimo pode denunciar (spec 008).
- BrasilAPI fora do ar na verificação: `Pending` (202). Um job tenta de novo a cada 6 h. Depois de 3 dias sem resposta, a verificação vira `Failed` com o motivo "Não conseguimos consultar o CNPJ agora" e gera `mei.verification_failed` com `FromPending`. O prestador pode tentar de novo.
- Revalidação que perde o selo: `Lost`, `mei.verification_lost`, e o selo some do perfil na hora (cache invalidado pelo evento).
- Qualquer mudança no prestador, inclusive vinda de job, gera um evento do catálogo que invalida o perfil público ([09-cache.md §3](../09-cache.md)).
- Eventos da vitrine (`provider.*` e `mei.*`), conteúdo e consumidores: catálogo de [10-eventos.md](../10-eventos.md) §9. Nesta spec são implementados `InvalidateProviderProfileCache` e `RemoveProviderProfileOfDeletedUser`; `AccountNotifications` chega com a spec 009.
- Upload confirmado mas arquivo inválido (não é JPEG, PNG ou WebP; mais de 10 MB; mais de 8000 px): a foto vai para `Rejected` com o motivo, e o app mostra.
- URL de upload usada depois de 5 min, ou objeto que não chegou ao storage: a confirmação responde 404 e o app pede outra URL.
- Foto com EXIF: todos os metadados são removidos antes de publicar, inclusive na foto de perfil.
- Objetos enviados e nunca confirmados: um job apaga uploads sem confirmação com mais de 24 h.
- Storage fora do ar: `upload-url` responde 503; o processamento da imagem tenta de novo pelo Hangfire.
- Raio que cobre várias cidades: permitido; a cidade base continua a do endereço.
- Edição concorrente em dois aparelhos: 409 por `xmin`; o app recarrega.
- Texto "Sobre", legenda, título ou descrição de serviço com telefone, e-mail ou link: 400 com mensagem ("Use o botão de WhatsApp para contato").
- Conta excluída: a vitrine passa a `Removed`, some do perfil público, libera o CNPJ e a conta perde o papel `provider`.
- "Disponível hoje" com a vitrine pausada: o selo não aparece; a vitrine pausada nem tem perfil público.

## Non-Functional Requirements

- `GET /providers/{id}` com p95 ≤ 150 ms usando `HybridCache` (5 min no Redis, 30 s local, invalidado por qualquer mudança no prestador). "Disponível hoje" e distância são calculados fora do cache. Ver [09-cache.md](../09-cache.md).
- Fotos públicas servidas por URL pública do storage (CDN do R2 em produção), com chave não adivinhável; WebP com até 2048 px, miniatura de 480 px com até 60 KB, foto de perfil quadrada de 512 px.
- Ponto público recalculado sempre que o endereço base muda; auditoria de mudança de endereço base (sem o endereço no registro).
- Revalidação MEI diária em lotes de 200, com intervalo entre consultas para respeitar a BrasilAPI.
- Do upload confirmado à foto `Active` em até 30 s no p95.
- `/p/{id}` responde com `Cache-Control: public, max-age=300`.

## Integrations

- BrasilAPI CNPJ v1 (`situacao_cadastral`/`descricao_situacao_cadastral`, `opcao_pelo_mei`, `razao_social`), com timeout e resiliência como as outras integrações.
- BrasilAPI CEP v2 (endereço base e ponto aproximado) e a tabela `cities` (spec 002) para a cidade e o ponto interno.
- Storage S3 compatível com URL assinada de `PUT` (R2 em produção, SeaweedFS no local e nos testes) e processamento de imagem com SkiaSharp em job do Hangfire.
- Hangfire com Hangfire.PostgreSql, no mesmo processo da API; painel só para admin, fora da internet pública.
- Configuração: URL da Google Play, pacote Android e impressões digitais SHA-256 da chave de assinatura para o `assetlinks.json`.

## Acceptance Criteria

- **AC-1** Dado um usuário sem vitrine, quando cria a vitrine, então ela nasce em `Draft` e, depois do refresh, o token traz o papel `provider`.
- **AC-2** Dada uma vitrine sem "Sobre" e sem serviços, quando tenta publicar, então recebe 422 com `missing` contendo `about` e `services`.
- **AC-3** Dada uma vitrine com todos os campos mínimos, quando publica, então o status vira `Published` e ela aparece em `GET /providers/{id}`.
- **AC-4** Dado um CNPJ ativo e optante pelo MEI, quando o prestador verifica, então o status é `Verified` e o perfil público mostra o selo "MEI verificado".
- **AC-5** Dado um CNPJ não optante pelo MEI, quando verifica, então o status é `Failed` com motivo legível e nenhum selo aparece.
- **AC-6** Dado um CNPJ já vinculado a outra vitrine, quando verifica, então recebe 409.
- **AC-7** Dada uma vitrine publicada, quando qualquer pessoa consulta o perfil público, então a resposta não contém endereço base, coordenadas nem CNPJ, e a distância (se informada a localização) é arredondada.
- **AC-8** Dada uma foto JPEG com GPS no EXIF, quando é processada, então a versão publicada não contém metadados de localização.
- **AC-9** Dada uma vitrine com 30 fotos, quando pede nova URL de upload, então recebe 409.
- **AC-10** Dado "Disponível hoje" ligado ontem no fuso da cidade base, quando alguém consulta o perfil hoje, então o selo não aparece, sem depender de nenhum job.
- **AC-11** Dado o id de foto de outra vitrine, quando o prestador tenta apagar, então recebe 404.
- **AC-12** Dada uma vitrine pausada ou em rascunho, quando alguém consulta o perfil público, então recebe 404.
- **AC-13** Dado um texto "Sobre" com número de telefone, quando salva, então recebe 400 explicando que contato é pelo botão de WhatsApp.
- **AC-14** Dada uma vitrine pausada, quando o prestador chama `POST /provider/publish`, então ela volta a `Published`.
- **AC-15** Dado um selo MEI que a revalidação marca como `Lost`, quando o perfil público é consultado logo depois, então o selo não aparece.
- **AC-16** Dada uma verificação MEI `Pending` há mais de 3 dias, quando o job roda, então ela vira `Failed` com motivo, gera `mei.verification_failed` com `FromPending` e o Painel mostra o motivo.
- **AC-17** Dado o link `https://vitrio.app/p/{id}` de uma vitrine publicada, quando aberto num Android com o app, então o app abre no perfil; sem o app, a página mostra o perfil resumido, as tags Open Graph e o botão da Google Play.
- **AC-18** Dado um CEP base sem coordenadas na BrasilAPI, quando o prestador salva a área de atendimento, então o ponto público sai do ponto interno da cidade e a resposta traz `locationApproximate: true`.
- **AC-19** Dada uma vitrine com WhatsApp desligado, quando alguém consulta o perfil público, então a resposta não contém o telefone.
- **AC-20** Dada uma conta com vitrine e CNPJ verificado, quando a conta é excluída, então a vitrine passa a `Removed`, o perfil público responde 404 e o CNPJ pode ser verificado por outra vitrine.
- **AC-21** Dada uma vitrine publicada, quando o prestador muda o texto "Sobre", então o perfil público mostra o texto novo em até 2 s (sem esperar o fim do cache).
- **AC-22** Dado um arquivo que não é imagem enviado como foto, quando o processamento roda, então a foto fica `Rejected` com o motivo e não aparece no perfil público.
- **AC-23** Dada uma conta com celular cadastrado, quando abre o passo "Monte sua vitrine" pela primeira vez, então o telefone de contato vem preenchido com esse celular.

## Deferred Decisions

| Decisão | Fallback escolhido agora | Revisitar quando |
| --- | --- | --- |
| Moderação prévia de fotos | Publicação imediata com denúncia e ocultação automática (spec 008) | Denúncias de foto passarem de 1% das fotos |
| Vídeos na vitrine | Só fotos | Pedido recorrente dos prestadores |
| Conferir nome do titular do CNPJ com o nome da conta | Só mostra a razão social ao prestador na confirmação | Casos de uso indevido de CNPJ |
| Avisos do selo MEI por push | Eventos `mei.*` gerados agora; o Painel mostra o status | Spec 009 (notificações) |
| Ponto base preciso quando o CEP não tem coordenadas | Ponto interno da cidade, marcado como aproximado | Reclamação de distância errada em cidade grande |

## Open Questions

Nenhuma.
