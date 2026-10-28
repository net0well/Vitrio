# Spec 001 — Contas e autenticação

- **Status:** Draft
- **Data:** 2026-10-28
- **Responsável:** Wellington

## Problem

Clientes e prestadores precisam de uma conta para pedir orçamento, anunciar, avaliar e receber avisos. O Vitrio tem **cadastro e login próprios**, com e-mail e senha e o e-mail confirmado por código, sem depender de provedor externo: o cadastro, os dados e as regras de acesso ficam sob controle do Vitrio. Entrar tem de ser raro (a sessão dura enquanto a pessoa usa o app), seguro (senha bem guardada, sessões revogáveis, tokens curtos, nada que revele quais e-mails têm conta) e cumprir as regras das lojas (exclusão de conta dentro do app) e a LGPD.

## Scope

### In

- Criar conta com nome, e-mail e senha. A conta só recebe sessão depois de confirmar o e-mail com um código de 6 dígitos enviado por e-mail.
- Entrar com e-mail e senha.
- **Continuar conectado:** depois de entrar, o app renova a sessão sozinho; a pessoa só entra de novo depois de 60 dias sem abrir o app, 1 ano desde o último login ou quando a sessão é encerrada (§Edge Cases).
- Esqueci a senha: código por e-mail e nova senha; todas as sessões são encerradas.
- Trocar a senha estando logado.
- E-mails de conta com o layout de [design-system/emails.md](../design-system/emails.md): código de confirmação, código de redefinição, senha alterada e aviso de tentativa de cadastro com e-mail já usado.
- Sessão com JWT de acesso (10 min) e refresh token rotativo com detecção de reuso.
- Sair deste aparelho e sair de todos os aparelhos.
- Ver e editar dados básicos: nome de exibição, foto (opcional), telefone (opcional para cliente).
- Endereços salvos por CEP (rótulo, CEP, logradouro, número, complemento, bairro, cidade, UF).
- Excluir conta dentro do app.
- Registro do aparelho para push (token FCM), consumido pela spec 009.

### Out

- Login social (Google, Apple, Facebook) e login por telefone.
- Verificação de telefone por código e verificação em duas etapas.
- Trocar o e-mail da conta.
- Exportação de dados pelo app (atendida por solicitação ao suporte no MVP).
- Painel administrativo.

## Domain Model

- `User`: `Id`, `Email` (citext, único, normalizado em minúsculas), `EmailVerifiedAt?`, `PasswordHash`, `PasswordChangedAt`, `DisplayName`, `PhotoKey?`, `PhoneNumber?` (E.164, criptografado), `Status` (`PendingVerification`, `Active`, `Suspended`, `Deleted`), `FailedSignInCount`, `LockedOutUntil?`, `CreatedAt`, `DeletedAt?`, `Roles` (`customer` sempre; `provider` quando existir vitrine; `admin` interno).
- `EmailCode`: `Id`, `Email`, `Purpose` (`VerifyEmail`, `ResetPassword`), `CodeHash` (HMAC-SHA256 com chave do servidor), `ExpiresAt` (10 min), `Attempts` (máx. 5), `ConsumedAt?`, `CreatedAt`. Um código novo invalida o anterior do mesmo e-mail e propósito.
- `Session`: `Id` (`sid`), `UserId`, `DeviceName`, `CreatedAt` (momento do login), `LastSeenAt`, `RevokedAt?`, `RevokedReason?`.
- `RefreshToken`: `Id`, `SessionId`, `FamilyId`, `TokenHash`, `ExpiresAt` (60 dias deslizantes), `AbsoluteExpiresAt` (1 ano do login), `UsedAt?`, `ReplacedById?`, `RevokedAt?`.
- `Address`: `Id`, `UserId`, `Label`, `Cep`, `Street`, `Number`, `Complement?`, `Neighborhood`, `CityIbgeCode`, `State`, `Location` (ponto geocodificado quando disponível) — campos de endereço criptografados.
- `Device`: `InstallationId`, `UserId`, `Platform`, `PushToken`, `UpdatedAt`.
- Ciclo de vida da conta: `PendingVerification` → `Active` (código confirmado); `PendingVerification` sem confirmação em 24 h é apagada; `Active` → `Suspended` (moderação) → `Active`; `Active` → `Deleted` (anonimização em até 30 dias).
- **Senha:** 10 a 128 caracteres depois de normalizar (Unicode NFKC), qualquer caractere (inclusive espaço), sem regra de composição; recusada se estiver na lista de senhas comuns ou for igual ao e-mail. Guardada com o `PasswordHasher` do ASP.NET Core Identity (PBKDF2-HMAC-SHA512, formato v3), refeita no login quando o hasher pedir. Nunca sai da API, nem em hash.

## API Contract

| Verbo | Rota | Request | Response | Erros |
| --- | --- | --- | --- | --- |
| POST | `/api/v1/auth/register` | `{ displayName, email, password, acceptedTermsVersion }` | 202 (sempre a mesma resposta, exista o e-mail ou não) | 400 senha fraca, termos não aceitos ou campo inválido, 429, 503 `email.unavailable` |
| POST | `/api/v1/auth/verify-email` | `{ email, code, deviceName }` | `{ accessToken, expiresIn, refreshToken, user }` | 400 `auth.invalid_code` (código errado, vencido ou usado), 429 |
| POST | `/api/v1/auth/resend-code` | `{ email, purpose: "verifyEmail" \| "resetPassword" }` | 202 (sempre) | 400, 429 com `Retry-After`, 503 |
| POST | `/api/v1/auth/login` | `{ email, password, deviceName }` | `{ accessToken, expiresIn, refreshToken, user }` | 401 `auth.invalid_credentials`, 403 `account.suspended`, 409 `account.email_not_verified` (só com a senha certa), 429 conta bloqueada com `Retry-After` |
| POST | `/api/v1/auth/password/forgot` | `{ email }` | 202 (sempre) | 400, 429, 503 |
| POST | `/api/v1/auth/password/reset` | `{ email, code, newPassword, deviceName }` | `{ accessToken, expiresIn, refreshToken, user }` | 400 `auth.invalid_code` ou senha fraca, 429 |
| POST | `/api/v1/auth/refresh` | `{ refreshToken }` | `{ accessToken, expiresIn, refreshToken }` | 401 inválido, expirado, revogado ou reuso detectado |
| POST | `/api/v1/auth/logout` | `{ allDevices: bool }` | 204 | 401 |
| PUT | `/api/v1/me/password` | `{ currentPassword, newPassword }` | 204 | 400 senha fraca, 401, 403 `auth.invalid_credentials` (senha atual errada) |
| GET | `/api/v1/me/sessions` | — | `[{ id, deviceName, createdAt, lastSeenAt, current }]` | 401 |
| DELETE | `/api/v1/me/sessions/{id}` | — | 204 | 401, 404 |
| GET | `/api/v1/me` | — | `{ id, displayName, email, photoUrl, phoneNumber, roles, hasProviderProfile }` | 401 |
| PATCH | `/api/v1/me` | `{ displayName?, phoneNumber?, photoKey? }` | `me` | 400, 401 |
| DELETE | `/api/v1/me` | — (a confirmação é feita no app) | 204 | 401, 409 pedido agendado ou contestação de pagamento aberta |
| GET | `/api/v1/me/addresses` | — | `[address]` | 401 |
| POST | `/api/v1/me/addresses` | `{ label, cep, number, complement? }` | 201 `address` | 400 CEP inválido ou não encontrado, 401, 409 limite de 10 |
| PUT | `/api/v1/me/addresses/{id}` | idem | `address` | 400, 401, 404 |
| DELETE | `/api/v1/me/addresses/{id}` | — | 204 | 401, 404 |
| PUT | `/api/v1/me/devices/{installationId}` | `{ platform, pushToken }` | 204 | 400, 401 |
| DELETE | `/api/v1/me/devices/{installationId}` | — | 204 | 401 |

## Authorization

- Rotas `/auth/*` (exceto `logout`): anônimas, política de rate limit `auth` (por IP). Os limites por e-mail de §Edge Cases ficam no handler, contados no banco.
- Demais rotas: usuário autenticado; tudo filtrado por `sub` do token. Endereço, aparelho ou sessão de outro usuário responde 404.
- `roles` nunca vem do request. `admin` só por tabela interna.
- Sessão revogada ou conta suspensa/excluída invalida o access token em até 1 minuto. A revogação é gravada no banco (`Session.RevokedAt`, `User.Status`); a validação consulta esse estado por trás de um cache de 1 min ([09-cache.md §4](../09-cache.md)), então reinício e várias instâncias não reabrem o acesso.

## Edge Cases & Failure Modes

- **Continuar conectado:** o app guarda o refresh token no `flutter_secure_storage` e o renova ao abrir e sempre que o access token vence; cada uso troca o refresh e empurra a validade para 60 dias à frente, até o limite de 1 ano desde o login. A sessão acaba antes disso quando a pessoa sai, troca ou redefine a senha (as outras sessões; na redefinição, todas), encerra a sessão em "Aparelhos conectados", a conta é suspensa ou excluída, ou um refresh já usado é reapresentado.
- **Sem revelar quais e-mails têm conta:** cadastro, reenvio de código e "Esqueci a senha" respondem sempre 202. Cadastro com e-mail de conta ativa não cria nada e manda ao dono o e-mail "Alguém tentou criar uma conta com seu e-mail"; "Esqueci a senha" com e-mail inexistente não envia nada. Login com e-mail inexistente roda uma verificação de senha falsa para levar o mesmo tempo e responde o mesmo `auth.invalid_credentials`.
- **E-mail não confirmado:** login com a senha certa numa conta `PendingVerification` responde 409 `account.email_not_verified`; o app vai para a tela do código e chama `resend-code`. Com a senha errada, a resposta é a genérica.
- **Cadastro repetido antes de confirmar:** troca nome e senha da conta pendente e envia código novo (dentro dos limites). Quem tem o e-mail sempre consegue tomar posse da conta pendente confirmando o código; ninguém usa uma conta sem provar acesso à caixa de e-mail.
- **Código:** 6 dígitos de `RandomNumberGenerator`, válido por 10 min, uso único, comparação em tempo constante. Na 5ª tentativa errada o código é invalidado. Reenvio no mínimo 60 s depois do anterior e no máximo 5 códigos por e-mail por hora (429 com `Retry-After`).
- **Senha errada repetida:** 5 erros em 15 min bloqueiam o login daquela conta por 15 min (429 com `Retry-After`); redefinir a senha desbloqueia. O bloqueio é da conta e o rate limit `auth` limita cada IP, para um atacante não conseguir testar muitas contas nem trancar muitas de uma vez.
- **Redefinição de senha:** revoga todas as sessões e refresh tokens, abre uma sessão nova no aparelho que redefiniu, zera o bloqueio e envia o e-mail "Sua senha foi alterada". Troca de senha logado revoga as outras sessões e envia o mesmo aviso.
- **Envio de e-mail falha (SMTP fora do ar ou recusado):** a requisição responde 503 `email.unavailable` com `Retry-After`, igual para e-mail existente ou não. O código gerado fica válido e o app oferece "Reenviar".
- **Termos de uso:** o cadastro grava a versão aceita dos termos e da política de privacidade e a data; sem aceite, 400.
- Refresh token reapresentado depois de usado: revoga a família inteira, registra auditoria e devolve 401.
- Dois refresh simultâneos do mesmo aparelho: o `ApiClient` serializa; se mesmo assim chegarem dois, o segundo recebe 401 e o app volta para a tela de login.
- Conta suspensa tenta entrar: 403 com ProblemDetails `account.suspended` e motivo genérico.
- Exclusão com pedido `Scheduled` ou contestação de pagamento aberta (como cliente ou prestador): bloqueia com 409 e explica o que resolver antes.
- Exclusão de quem tem vitrine: pedidos `Sent` e `Quoted` são cancelados com motivo "Conta excluída", a vitrine vira `Removed` (some da busca e do perfil público), o CNPJ fica livre e as avaliações recebidas deixam de aparecer.
- `DELETE /me` não leva corpo: alguns proxies e clientes HTTP descartam corpo em `DELETE`. A confirmação ("Digite EXCLUIR") acontece no app antes da chamada.
- CEP não encontrado na BrasilAPI: aceita cadastro manual de logradouro, bairro e cidade (cidade escolhida da lista do IBGE).

## Non-Functional Requirements

- p95 de `/auth/login` ≤ 400 ms (o hash da senha é propositalmente lento); demais `/auth/*` ≤ 300 ms sem contar o envio de e-mail.
- Senha, código, tokens e refresh nunca em log nem em evento de domínio. O corpo dos e-mails não é logado; o log de envio guarda só id da conta, tipo do e-mail e resultado.
- Os e-mails saem logo depois do commit, direto do handler, sem passar pela outbox, para o código não ficar gravado em texto em lugar nenhum.
- Auditoria: cadastro, confirmação de e-mail, login (sucesso e bloqueio), redefinição e troca de senha, sessão encerrada, logout, revogação por reuso e exclusão de conta.
- Telefone e endereço criptografados em coluna (Data Protection).
- Exclusão: dados pessoais apagados ou anonimizados em até 30 dias; job diário conclui pendências e apaga contas `PendingVerification` com mais de 24 h, códigos vencidos e refresh tokens expirados.
- Conformidade com as lojas: botão "Excluir conta" em Conta, com confirmação, e página web de exclusão para quem desinstalou o app.
- Métricas de acesso: telas de cadastro e login abertas, cadastros confirmados, logins concluídos, bloqueios e redefinições por dia, em contagens sem dado pessoal.

## Integrations

- **E-mail transacional por SMTP** (MailKit). Em desenvolvimento, Mailpit (`infra/docker-compose.yml`, caixa em `http://localhost:8025`); em produção, um provedor com o domínio `vitrio.app` autenticado (SPF, DKIM e DMARC). Remetente `Vitrio <nao-responda@vitrio.app>`. Layout e textos em [design-system/emails.md](../design-system/emails.md).
- BrasilAPI CEP: logradouro, bairro, cidade e UF a partir do CEP.

## Acceptance Criteria

- **AC-1** Dado um e-mail sem conta, quando a pessoa se cadastra com nome, senha válida e aceite dos termos, então a resposta é 202, a conta fica `PendingVerification` sem sessão e um e-mail com código de 6 dígitos é enviado.
- **AC-2** Dada uma conta pendente, quando a pessoa envia o código certo em até 10 min, então a conta vira `Active`, `EmailVerifiedAt` é preenchido e a resposta traz access e refresh tokens.
- **AC-3** Dado um código enviado há 11 min, ou já usado, ou com 5 tentativas erradas, quando é enviado, então a resposta é 400 `auth.invalid_code`.
- **AC-4** Dado um e-mail de conta ativa, quando alguém tenta se cadastrar com ele, então a resposta é a mesma 202 do AC-1, nenhuma conta é criada e o dono recebe o aviso de tentativa de cadastro.
- **AC-5** Dada uma conta ativa, quando entra com e-mail e senha certos, então recebe access e refresh tokens; com a senha errada ou um e-mail inexistente, recebe o mesmo 401 `auth.invalid_credentials`.
- **AC-6** Dada uma conta com 5 senhas erradas em 15 min, quando tenta entrar de novo, então recebe 429 com `Retry-After`, mesmo com a senha certa; depois de redefinir a senha, entra normalmente.
- **AC-7** Dada uma conta pendente, quando entra com a senha certa, então recebe 409 `account.email_not_verified`.
- **AC-8** Dado "Esqueci a senha" com e-mail de conta ativa, quando a pessoa envia código certo e senha nova, então recebe tokens novos, todas as sessões antigas são revogadas e chega o e-mail "Sua senha foi alterada".
- **AC-9** Dada uma senha com 9 caracteres ou da lista de senhas comuns, quando usada no cadastro, na redefinição ou na troca, então a resposta é 400 no campo da senha.
- **AC-10** Dado um refresh token usado há 59 dias, quando chama `POST /auth/refresh`, então recebe um novo par; com um refresh parado há 61 dias, ou de um login feito há mais de 1 ano, recebe 401.
- **AC-11** Dado um refresh token já usado, quando é apresentado de novo, então a resposta é 401, todos os refresh da mesma família são revogados e um evento de auditoria é gravado.
- **AC-12** Dado um usuário logado em dois aparelhos, quando chama `POST /auth/logout` com `allDevices: true`, então os dois deixam de conseguir renovar a sessão e o access token do outro aparelho é recusado em até 1 minuto.
- **AC-13** Dado um usuário logado em dois aparelhos, quando encerra a sessão do outro aparelho em `DELETE /me/sessions/{id}`, então aquele aparelho volta para a tela de login na próxima renovação; a sessão de outro usuário responde 404.
- **AC-14** Dado um usuário suspenso, quando tenta entrar, então recebe 403 com código `account.suspended`.
- **AC-15** Dado um sexto pedido de código para o mesmo e-mail na mesma hora, quando é feito, então recebe 429 com `Retry-After` e nenhum e-mail sai.
- **AC-16** Dado qualquer e-mail de conta enviado, quando é aberto no Mailpit, então tem versão HTML no layout do design system e versão em texto puro, sem link para entrar e sem dado além do nome e do código.
- **AC-17** Dado um usuário autenticado, quando cadastra um endereço com CEP válido, então logradouro, bairro, cidade (código IBGE) e UF são preenchidos e o endereço aparece em `GET /me/addresses`.
- **AC-18** Dado o id de um endereço de outro usuário, quando chama `PUT /me/addresses/{id}`, então a resposta é 404.
- **AC-19** Dado um usuário sem pedidos agendados, quando confirma a exclusão da conta, então a sessão é encerrada, um novo cadastro com o mesmo e-mail cria uma conta nova e as avaliações antigas aparecem como "Usuário removido".
- **AC-20** Dado um usuário com pedido `Scheduled` ou contestação de pagamento aberta, quando tenta excluir a conta, então recebe 409 explicando o que precisa resolver.
- **AC-21** Dado um prestador sem pedidos agendados nem contestações, quando exclui a conta, então os pedidos `Sent` e `Quoted` dele são cancelados, a vitrine responde 404 no perfil público e o mesmo CNPJ pode ser usado em outra vitrine.
- **AC-22** Dada uma sessão revogada, quando a API é reiniciada e o access token antigo é usado, então a resposta é 401.
- **AC-23** Dado qualquer endpoint desta spec que exige login, quando chamado sem token, então responde 401.

## Deferred Decisions

| Decisão | Fallback escolhido agora | Revisitar quando |
| --- | --- | --- |
| Login social (Google, Apple) | Só cadastro e login próprios. Se entrar um dia, vira mais uma forma de provar identidade, vinculada à conta pelo e-mail confirmado | Abandono no cadastro acima de 25% ou pedido recorrente dos usuários |
| Desbloqueio por biometria no app | Sessão longa sem biometria; o celular já tem bloqueio de tela | Ações sensíveis (pagamento pelo app) entrarem |
| Verificação em duas etapas | Não há; bloqueio por tentativas, aviso de senha alterada e lista de aparelhos conectados | Fraude de conta aparecer; para admins, antes do painel web |
| Integridade do app (Play Integrity) | Rate limit e bloqueio por tentativas | Cadastros automatizados aparecerem |
| Trocar o e-mail da conta | Pelo suporte | Pedidos recorrentes |
| Exportação de dados no app | Atendimento por e-mail do suporte | Volume de pedidos de exportação passar de 5/mês |

## Open Questions

- Provedor de e-mail em produção (proposta: Amazon SES na região de São Paulo, pelo custo e pela entrega; alternativas: Resend, Brevo).
- Nome exibido do cliente para o prestador antes do aceite: só primeiro nome? (proposta: primeiro nome + inicial)
- Foto de perfil do cliente: permitir no MVP ou só iniciais?
