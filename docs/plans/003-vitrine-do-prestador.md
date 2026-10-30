# Plano: Vitrine do prestador

**Status:** Em andamento
**Spec:** [003-vitrine-do-prestador](../specs/003-vitrine-do-prestador.md) (Approved)
**Arquitetura:** Clean Architecture com Controllers ([03-arquitetura.md](../03-arquitetura.md))
**Camadas afetadas:**
- Domain: `Providers`, `Common` (`Money`, ponto geográfico).
- Application: `Providers`, papéis em `Accounts`, abstrações de storage, imagem, CNPJ e jobs.
- Shared: núcleo técnico (`Result`, `Error`, `IClock`, `ClockTime`, `ClaimNames`, `ConcurrencyConflictException`), criado nesta spec.
- Infrastructure: tabelas da vitrine, SeaweedFS/R2, SkiaSharp, BrasilAPI CNPJ.
- Jobs: Hangfire, criado nesta spec.
- Api: `ProviderController`, `ProvidersController`, `CategoriesController`, página `/p/{id}` e `assetlinks.json`.
- App Flutter: modo prestador.

## Recortes

Todos na branch `feat/vitrine-do-prestador`, em vários commits. O PR só abre quando todos estiverem prontos e testados de ponta a ponta.

| # | Recorte | Critérios |
| --- | --- | --- |
| 1 | Categorias e vitrine na API: criar, dados, categorias, área, serviços, disponibilidade, "Disponível hoje", publicar, pausar, Painel | AC-1, AC-2, AC-13, AC-14, AC-18 |
| 2 | Perfil público com cache e invalidação por evento; vitrine removida com a conta | AC-3, AC-7, AC-10, AC-12, AC-19, AC-20 (vitrine), AC-21 |
| 3 | Fotos: storage, URL de upload, processamento, foto de perfil, limpeza | AC-8, AC-9, AC-11, AC-22 |
| 4 | Selo MEI: verificação, novas tentativas e revalidação com Hangfire | AC-4, AC-5, AC-6, AC-15, AC-16, AC-20 (CNPJ) |
| 5 | Link de compartilhar: `/p/{id}` com Open Graph e `assetlinks.json` | AC-17 (API) |
| 6 | App: modo prestador, onboarding, Painel, Meu perfil, perfil público, fotos, compartilhar e abrir pelo link | AC-17 (app), AC-23 |

## Decisões técnicas

| Tema | Decisão |
| --- | --- |
| Agregado | `Provider` em `Domain/Providers`, com serviços, categorias, bairros, faixas, fotos e verificação MEI como entidades filhas. Regras de publicação, checklist, limites (3 categorias, 20 serviços, 30 fotos, 30 bairros, 3 faixas por dia) e transições de estado no agregado, com `Result`. Concorrência por `xmin`. |
| Localização | O Domain guarda o ponto como value object próprio (latitude e longitude); a Infrastructure converte para `geography` (NTS fica fora do Domain, como na spec 002). Célula pública de ~500 m calculada no Domain (grade de 0,0045° de latitude e 0,0045°/cos(lat) de longitude, centro da célula). Distância e arredondamento ([02 §2](../02-regras-de-negocio.md)) também no Domain. |
| Área de atendimento | `IServiceAreaService` resolve CEP → endereço, cidade e ponto. Usa o `IAddressLookupService.LookupAsync` da spec 001 e, sem coordenadas, o ponto interno da cidade (`ICitySearchableRepository.GetAsync`), marcando `LocationApproximate`. |
| Dados pessoais | `BaseAddress`, `BaseLocation` e `ContactPhone` com `IsPersonalData()` (criptografia de coluna da spec 001). Auditoria da troca de endereço base sem o endereço. |
| Texto com contato | Regra pura no Domain que detecta telefone, e-mail e link em "Sobre", legendas e serviços. |
| Papel `provider` | `AccountRolesService` passa a incluir `provider` quando a conta tem vitrine em `Draft`, `Published` ou `Paused`. `hasProviderProfile` do `GET /me` segue o mesmo dado. |
| Repositories | `IProviderWritableRepository` (agregado rastreado), `IProviderSearchableRepository` (privado, Painel, perfil público e papéis, com projeção) e `ICategorySearchableRepository`. |
| Cache | Perfil público em `provider:{id}` (5 min Redis, 30 s local, tag `provider:{id}`). O cache guarda os campos; "Disponível hoje" (fuso da cidade) e distância são calculados ao montar a resposta. Categorias em `categories` (1 h e 5 min) com `Cache-Control` público. |
| Eventos | `provider.published`, `provider.paused` e `provider.profile_updated` no agregado; handler `InvalidateProviderProfileCache` remove a tag. `RemoveProviderProfileOfDeletedUser` consome `user.deleted`. Foto processada ou rejeitada gera `provider.profile_updated`. |
| Storage | `IObjectStorage` na Application; implementação com AWSSDK.S3 (Apache 2.0) e `ForcePathStyle`, que serve para R2 e SeaweedFS. Chaves: `uploads/{guid}` (temporária) e `public/providers/{id}/{guid}.webp`. URL assinada de `PUT` de 5 min com `Content-Type` e `Content-Length` assinados. URL pública por configuração. SeaweedFS no `docker-compose` e em Testcontainers. |
| Imagem | `IImageProcessor` na Application; SkiaSharp na Infrastructure. Confere a assinatura binária, aplica a orientação do EXIF, limita a 10 MB e 8000 px, reencoda em WebP (2048 px e miniatura de 480 px; perfil quadrado de 512 px) sem metadados e apaga o original. |
| CNPJ | `ICnpjLookup` (BrasilAPI CNPJ v1) com resiliência; validação do dígito verificador no Domain. Dublê nos testes. |
| Jobs | Hangfire com Hangfire.PostgreSql no projeto `Vitrio.Jobs`, no processo da API, desligável por `Jobs:Enabled` (desligado nos testes de integração). Os jobs só chamam comandos da Application pelo `ISender`: processamento da foto, novas tentativas do MEI pendente, revalidação diária em lotes de 200 e limpeza de uploads sem confirmação. A confirmação da foto enfileira o processamento por `IPhotoProcessingQueue` depois do commit; a limpeza também reenfileira foto presa em `Processing` há mais de 10 min, para nenhum envio se perder. Painel só para admin, fora da internet pública. Os testes chamam os comandos direto, com o relógio ajustável. |
| Página do link | `ShareController` responde `/p/{id}` com HTML montado por template embutido, texto codificado, Open Graph (título, descrição e imagem) e `Cache-Control: public, max-age=300`. `WellKnownController` responde o `assetlinks.json` pela configuração. Os dois são anônimos e usam a política `profile`. |
| Rate limit | Escritas pelo limitador global já existente (60/min por usuário); `uploads` no `upload-url`; `profile` nas leituras e nas rotas públicas; `search` nas categorias. |
| App | Pacotes `image_picker` (galeria e câmera) e `share_plus`. Upload direto à URL assinada. Modo ativo salvo no aparelho. Shell do prestador com Painel e Meu perfil. Rota `/p/:id` com intent-filter `autoVerify` para `vitrio.app/p/`. |

## Recorte 1: categorias e vitrine na API

**Situação:** pronto. O telefone de contato é pré-preenchido pela própria API na criação, a partir do celular da conta; o endereço base e o ponto base ficam criptografados, e só a célula pública fica em claro.

1. Domain: `Money`, ponto e célula pública, `Provider` com as entidades filhas, estados, checklist, eventos `provider.*`, regra de texto com contato.
2. Application: `Providers` (criar, `GET /provider`, `PATCH`, categorias, área, serviços, disponibilidade, "Disponível hoje", publicar, pausar, Painel), `IServiceAreaService` e repositories; papéis com `provider`.
3. Infrastructure: tabelas, configurações, migração com as 15 categorias, repositories.
4. Api: `CategoriesController` e `ProviderController`; 422 com a lista `missing`.
5. Testes de domínio, de integração (401 e 403 incluídos; não há rota com id de outra pessoa neste recorte) e de arquitetura.

## Recorte 2: perfil público

**Situação:** pronto. O cache guarda um retrato da vitrine; "Disponível hoje", nota, serviços feitos e distância são calculados ao montar a resposta. Conta excluída ou suspensa some do perfil público na hora, porque a consulta exige a conta ativa.

1. `GET /providers/{id}` com `providerPublic`, distância e selos calculados na leitura.
2. Cache `provider:{id}` e `InvalidateProviderProfileCache` para `provider.*` e `user.deleted`.
3. `RemoveProviderProfileOfDeletedUser`: vitrine `Removed` e papel retirado.

## Recorte 3: fotos

**Situação:** pronto.
- **Envio:** a URL assinada fixa o tipo e o tamanho; o teste prova que o storage recusa corpo maior que o assinado.
- **Processamento:** gera WebP de até 2048 px, miniatura de 480 px com até 60 KB e foto de perfil quadrada de 512 px, sem nenhum metadado (o teste envia um JPEG com GPS no EXIF).
- **Buckets:** `vitrio-uploads` é privado; `vitrio-public` tem leitura anônima e só recebe a versão processada.
- **Ambiente:** no local, as URLs saem com `10.0.2.2` para o emulador alcançar o storage.

1. SeaweedFS no `docker-compose` e em Testcontainers; `IObjectStorage`.
2. `upload-url`, confirmação, lista, legenda, ordem, exclusão, foto de perfil e fotos públicas paginadas.
3. Hangfire, `IPhotoProcessingQueue` e o processamento com SkiaSharp.
4. Job de limpeza de uploads sem confirmação e de fotos presas em `Processing`.

## Recorte 4: selo MEI

**Situação:** pronto.
- **Consulta:** um service (`IMeiCheckService`) traduz a consulta de CNPJ em resultado de domínio, usado na verificação e na revalidação.
- **Revalidação:** o job roda de hora em hora, em lotes de 200 com pausa de 1 s entre consultas. Repete o pendente a cada 6 h, desiste depois de 3 dias e revalida o selo a cada 30 dias.
- **Consulta fora do ar:** durante a revalidação, mantém o selo e tenta de novo.
- **CNPJ e razão social:** a razão social fica criptografada, porque a de MEI costuma trazer o nome e às vezes o CPF. O CNPJ fica em claro, com índice único, e é liberado quando a vitrine é removida.

1. Dígito verificador, `ICnpjLookup` e a verificação (`Verified`, `Failed`, `Pending`, 409 de CNPJ vinculado).
2. Comandos de nova tentativa (a cada 6 h, `Failed` depois de 3 dias) e de revalidação (30 dias, lotes de 200), agendados no Hangfire.
3. Eventos `mei.*` e invalidação do perfil.

## Recorte 5: link de compartilhar

**Situação:** pronto.
- **Página `/p/{id}`:** HTML de template embutido na Api, com o texto codificado e Open Graph, `Cache-Control: public, max-age=300` e CSP próprio (só estilo inline e imagens do storage). O perfil em cache vem de um service compartilhado com o perfil público.
- **`assetlinks.json`:** pacote e impressões digitais vêm de `PublicLinks`. No local, usa `app.vitrio.dev` e a chave de depuração.
- **Produção:** o proxy precisa encaminhar `vitrio.app/p/*` e `/.well-known/assetlinks.json` para a API.

1. `/p/{id}` com HTML e Open Graph; página de perfil não disponível.
2. `assetlinks.json` pela configuração.

## Recorte 6: app

**Situação:** pronto. As telas seguem o protótipo e o design system, com as diferenças aprovadas registradas na spec. O app ganhou os componentes do design system que faltavam (campo com rótulo, switch em linha, barra superior, barra de ação, busca, card de destaque, tile de categoria, card de prestador). Os três testes de ponta a ponta (conta, localização e prestador) passam no emulador contra a API local.

1. Modo prestador (Conta e boas-vindas), shell com Painel e Meu perfil.
2. Onboarding em 4 passos com o telefone pré-preenchido, edição do perfil e Painel.
3. Fotos com upload direto e acompanhamento do processamento.
4. Tela do perfil público, compartilhar e abertura pelo link.
5. Testes de widget e teste no emulador contra a API local com SeaweedFS.
