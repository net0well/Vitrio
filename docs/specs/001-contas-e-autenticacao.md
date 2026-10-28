# Spec 001 — Contas e autenticação

- **Status:** Draft
- **Data:** 2026-10-28
- **Responsável:** Wellington

## Problem

Clientes e prestadores precisam de uma conta para pedir orçamento, anunciar, avaliar e receber avisos. O login tem de ser rápido (sem senha), seguro (sessões revogáveis, tokens curtos) e cumprir as regras das lojas (login com Apple no iOS, exclusão de conta dentro do app) e a LGPD.

## Scope

### In

- Entrar com Google e com Apple (validação do ID token na API).
- Criação automática da conta no primeiro login.
- Sessão com JWT de acesso (10 min) e refresh token rotativo (30 dias deslizantes, 90 absolutos) com detecção de reuso.
- Sair deste aparelho e sair de todos os aparelhos.
- Ver e editar dados básicos: nome de exibição, foto (opcional), telefone (opcional para cliente).
- Endereços salvos por CEP (rótulo, CEP, logradouro, número, complemento, bairro, cidade, UF).
- Excluir conta dentro do app.
- Registro do aparelho para push (token FCM), consumido pela spec 009.

### Out

- Login com e-mail e senha, telefone ou outros provedores.
- Verificação de telefone por código.
- Exportação de dados pelo app (atendida por solicitação ao suporte no MVP).
- Painel administrativo.

## Domain Model

- `User`: `Id`, `Email` (citext, único), `DisplayName`, `PhotoKey?`, `PhoneNumber?` (E.164, criptografado), `Status` (`Active`, `Suspended`, `Deleted`), `CreatedAt`, `DeletedAt?`, `Roles` (`customer` sempre; `provider` quando existir vitrine; `admin` interno).
- `ExternalLogin`: `UserId`, `Provider` (`Google`, `Apple`), `Subject` (único por provedor).
- `Session`: `Id` (`sid`), `UserId`, `DeviceName`, `CreatedAt`, `LastSeenAt`, `RevokedAt?`.
- `RefreshToken`: `Id`, `SessionId`, `FamilyId`, `TokenHash`, `ExpiresAt`, `AbsoluteExpiresAt`, `UsedAt?`, `ReplacedById?`, `RevokedAt?`.
- `Address`: `Id`, `UserId`, `Label`, `Cep`, `Street`, `Number`, `Complement?`, `Neighborhood`, `CityIbgeCode`, `State`, `Location` (ponto geocodificado quando disponível) — campos de endereço criptografados.
- `Device`: `InstallationId`, `UserId`, `Platform`, `PushToken`, `UpdatedAt`.
- Ciclo de vida da conta: `Active` → `Suspended` (moderação) → `Active`; `Active` → `Deleted` (anonimização em até 30 dias).

## API Contract

| Verbo | Rota | Request | Response | Erros |
| --- | --- | --- | --- | --- |
| POST | `/api/v1/auth/google` | `{ idToken, deviceName }` | `{ accessToken, expiresIn, refreshToken, user }` | 400 token malformado, 401 token inválido, 403 conta suspensa, 429 |
| POST | `/api/v1/auth/apple` | `{ idToken, nonce, deviceName, fullName? }` | idem | idem |
| POST | `/api/v1/auth/refresh` | `{ refreshToken }` | `{ accessToken, expiresIn, refreshToken }` | 401 inválido, expirado ou reuso detectado |
| POST | `/api/v1/auth/logout` | `{ allDevices: bool }` | 204 | 401 |
| GET | `/api/v1/me` | — | `{ id, displayName, email, photoUrl, phoneNumber, roles, hasProviderProfile }` | 401 |
| PATCH | `/api/v1/me` | `{ displayName?, phoneNumber?, photoKey? }` | `me` | 400, 401 |
| DELETE | `/api/v1/me` | `{ confirmation: "EXCLUIR" }` | 204 | 400, 401, 409 pedidos agendados em aberto |
| GET | `/api/v1/me/addresses` | — | `[address]` | 401 |
| POST | `/api/v1/me/addresses` | `{ label, cep, number, complement? }` | 201 `address` | 400 CEP inválido ou não encontrado, 401, 409 limite de 10 |
| PUT | `/api/v1/me/addresses/{id}` | idem | `address` | 400, 401, 404 |
| DELETE | `/api/v1/me/addresses/{id}` | — | 204 | 401, 404 |
| PUT | `/api/v1/me/devices/{installationId}` | `{ platform, pushToken }` | 204 | 400, 401 |
| DELETE | `/api/v1/me/devices/{installationId}` | — | 204 | 401 |

## Authorization

- `/auth/google`, `/auth/apple`, `/auth/refresh`: anônimos, política de rate limit `auth`.
- Demais rotas: usuário autenticado; tudo filtrado por `sub` do token. Endereço ou aparelho de outro usuário responde 404.
- `roles` nunca vem do request. `admin` só por tabela interna.
- Sessão revogada ou conta suspensa/excluída invalida o access token em até 1 minuto (lista de `sid` revogados em cache).

## Edge Cases & Failure Modes

- Mesmo e-mail em Google e Apple: contas são vinculadas por `ExternalLogin`; se o e-mail já existir com outro provedor, vincula ao mesmo usuário somente se o e-mail vier verificado pelos dois provedores.
- Apple com "Ocultar meu e-mail": o e-mail de relay é aceito como e-mail da conta.
- Apple só envia o nome no primeiro login: o app repassa `fullName`; se não vier, `DisplayName` é a parte antes do `@`, editável depois.
- Refresh token reapresentado depois de usado: revoga a família inteira, registra auditoria e devolve 401.
- Dois refresh simultâneos do mesmo aparelho: o `ApiClient` serializa; se mesmo assim chegarem dois, o segundo recebe 401 e o app refaz login silencioso se o provedor permitir.
- Conta suspensa tenta logar: 403 com ProblemDetails `account.suspended` e motivo genérico.
- Exclusão com pedidos `Scheduled`: bloqueia com 409 e explica que é preciso concluir ou cancelar antes.
- JWKS do provedor fora do ar: usa as chaves em cache (até 24 h); sem cache, 503 com `Retry-After`.
- CEP não encontrado na BrasilAPI: aceita cadastro manual de logradouro, bairro e cidade (cidade escolhida da lista do IBGE).

## Non-Functional Requirements

- p95 de `/auth/*` ≤ 300 ms (com JWKS em cache).
- Tokens e refresh nunca em log; auditoria de login, logout, revogação por reuso e exclusão de conta.
- Telefone e endereço criptografados em coluna (Data Protection).
- Exclusão: dados pessoais apagados ou anonimizados em até 30 dias; job diário conclui pendências.
- Conformidade com as lojas: botão "Excluir conta" em Conta, com confirmação.

## Integrations

- Google: validação de ID token com JWKS do Google e `aud` = client ids Android e iOS.
- Apple: validação de ID token com JWKS da Apple, `aud` = bundle id, `nonce` com hash SHA-256.
- BrasilAPI CEP: logradouro, bairro, cidade e UF a partir do CEP.

## Acceptance Criteria

- **AC-1** Dado um ID token válido do Google de alguém sem conta, quando o app chama `POST /auth/google`, então a conta é criada com papel `customer`, uma sessão é aberta e a resposta traz access e refresh tokens.
- **AC-2** Dado um ID token com `aud` diferente dos client ids do app, quando chama `POST /auth/google`, então a resposta é 401 e nenhuma conta é criada.
- **AC-3** Dado um login com Apple cujo `nonce` não confere, quando chama `POST /auth/apple`, então a resposta é 401.
- **AC-4** Dado um refresh token válido, quando chama `POST /auth/refresh`, então recebe um novo par e o refresh anterior deixa de valer.
- **AC-5** Dado um refresh token já usado, quando é apresentado de novo, então a resposta é 401, todos os refresh da mesma família são revogados e um evento de auditoria é gravado.
- **AC-6** Dado um usuário logado em dois aparelhos, quando chama `POST /auth/logout` com `allDevices: true`, então os dois deixam de conseguir renovar a sessão e o access token do outro aparelho é recusado em até 1 minuto.
- **AC-7** Dado um usuário suspenso, quando tenta entrar, então recebe 403 com código `account.suspended`.
- **AC-8** Dado um usuário autenticado, quando cadastra um endereço com CEP válido, então logradouro, bairro, cidade (código IBGE) e UF são preenchidos e o endereço aparece em `GET /me/addresses`.
- **AC-9** Dado o id de um endereço de outro usuário, quando chama `PUT /me/addresses/{id}`, então a resposta é 404.
- **AC-10** Dado um usuário sem pedidos agendados, quando confirma a exclusão da conta, então a sessão é encerrada, o login seguinte com o mesmo provedor cria uma conta nova e as avaliações antigas aparecem como "Usuário removido".
- **AC-11** Dado um usuário com pedido `Scheduled`, quando tenta excluir a conta, então recebe 409 explicando que precisa concluir ou cancelar o pedido.
- **AC-12** Dado qualquer endpoint desta spec que exige login, quando chamado sem token, então responde 401.

## Deferred Decisions

| Decisão | Fallback escolhido agora | Revisitar quando |
| --- | --- | --- |
| Login por telefone | Só Google e Apple | Prestadores relatarem dificuldade com conta Google |
| Exportação de dados no app | Atendimento por e-mail do suporte | Volume de pedidos de exportação passar de 5/mês |

## Open Questions

- Nome exibido do cliente para o prestador antes do aceite: só primeiro nome? (proposta: primeiro nome + inicial)
- Foto de perfil do cliente: permitir no MVP ou só iniciais?
