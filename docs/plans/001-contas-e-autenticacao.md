# Plano: Contas e autenticação

**Status:** Em andamento
**Spec:** [001-contas-e-autenticacao](../specs/001-contas-e-autenticacao.md) (Approved)
**Arquitetura:** Clean Architecture com Controllers ([03-arquitetura.md](../03-arquitetura.md))
**Camadas afetadas:** Domain (`Accounts`), Application (`Accounts`), Infrastructure (persistência, senha, códigos, e-mail, tokens), Api (`AuthController`, `MeController`), app Flutter.

## Recortes

Os recortes são feitos nesta ordem, todos na branch `feat/contas-e-autenticacao`, em vários commits. O PR só abre quando os seis estiverem prontos e testados de ponta a ponta. Os critérios de aceite da spec ficam mapeados no recorte que os cobre.

| # | Recorte | Critérios | Depende de |
| --- | --- | --- | --- |
| 1 | Cadastro com confirmação de e-mail | AC-1, AC-2, AC-3, AC-4, AC-9 (cadastro), AC-15, AC-16, AC-26 | — |
| 2 | Login, sessão longa e aparelhos conectados | AC-5, AC-6, AC-7, AC-10, AC-11, AC-12, AC-13, AC-14, AC-22, AC-23 | 1 |
| 3 | Esqueci a senha e troca de senha | AC-8, AC-9 (redefinição e troca) | 2 |
| 4 | Conta, celular, endereços e aparelhos de push | AC-17, AC-18, AC-24, AC-25 | 2 |
| 5 | Exclusão de conta e primeiro admin | AC-19 (AC-20 e AC-21 completam quando pedidos, pagamentos e vitrine existirem) | 2 |
| 6 | Telas do app | Fluxos de cadastro, código, login, esqueci a senha, conta e aparelhos | 1 a 4 |

## Decisões técnicas

| Tema | Decisão |
| --- | --- |
| Agregado | `User` em `Vitrio.Domain/Accounts`, com métodos de transição (`Register`, `ConfirmEmail`, `RegisterFailedSignIn`, `ChangePassword`...) que devolvem `Result`. `EmailCode` e `Session`/`RefreshToken` como entidades próprias. |
| Value objects | `EmailAddress` (normalizado, até 254 caracteres), `DisplayName` (2 a 60, texto normalizado, sem contato), `PhoneNumber` (celular brasileiro, E.164). |
| Senha | Abstração `IPasswordHasher` na Application; implementação na Infrastructure sobre o `PasswordHasher<TUser>` de `Microsoft.Extensions.Identity.Core` (sem o resto do Identity). Política (`PasswordPolicy`) na Application: 10 a 128 caracteres após NFKC, lista de senhas comuns embutida como recurso, diferente do e-mail. |
| Códigos | `IVerificationCodes` na Application: gera 6 dígitos com `RandomNumberGenerator` e calcula HMAC-SHA256 com a chave `Auth:CodeKey` (segredo). Comparação com `CryptographicOperations.FixedTimeEquals`. |
| E-mail | `IEmailSender` na Application; `SmtpEmailSender` com MailKit na Infrastructure (`Email:Smtp`). Modelos HTML e texto como recursos embutidos, derivados de `docs/design-system/emails/`, com o logo embutido (`cid:vitrio-wordmark`) e valores escapados. Envio depois do commit, direto do handler. |
| Tokens | `ITokenIssuer` na Application; implementação com `JsonWebTokenHandler` e chave ES256 (`Jwt:SigningKey`, PEM em segredo). Em Development sem chave configurada, a API gera uma chave temporária e avisa no log; em produção a checagem de inicialização exige a chave. O JwtBearer valida com a chave pública da mesma configuração. |
| Sessão revogada | `OnTokenValidated` do JwtBearer consulta `session:{sid}:revoked` no `HybridCache` (banco por trás), conforme [09-cache.md §4](../09-cache.md). |
| Rate limit | Rotas `/auth/*` com `[EnableRateLimiting(RateLimitPolicies.Auth)]`; limites por e-mail (códigos) e bloqueio por conta contados no banco, no handler. |
| Testes | Integração com Testcontainers. Um `IEmailSender` falso captura os e-mails (fronteira externa) para os testes lerem o código. Um host separado com `FakeTimeProvider` cobre expirações (código de 10 min, bloqueio de 15 min, refresh de 60 dias), sem interferir nos outros testes. Um teste renderiza cada modelo de e-mail e confere HTML, texto e ausência de link. |

## Recorte 1: cadastro com confirmação de e-mail

**Situação:** pronto.

1. **Domain:** `User` (`PendingVerification` → `Active`), `EmailCode`, `EmailAddress`, `DisplayName`; erros em `AccountErrors`. Testes de unidade das transições e dos value objects.
2. **Application:** `Register`, `VerifyEmail`, `ResendCode` com validadores; abstrações `IPasswordHasher`, `IVerificationCodes`, `IEmailSender`, `ITokenIssuer`; `PasswordPolicy`. Testes de validadores e da política.
3. **Infrastructure:** configurações EF (`users`, `email_codes`, `sessions`, `refresh_tokens`), migração `AddAccounts`; hasher de senha, códigos HMAC, `SmtpEmailSender` e modelos de e-mail; `TokenIssuer` ES256 com emissão de sessão e refresh (hash SHA-256 do refresh).
4. **Api:** `AuthController` com `POST /auth/register`, `/auth/verify-email`, `/auth/resend-code`; `[AllowAnonymous]` e política `auth`; configuração de `Email:Smtp` (Mailpit em Development) e `Auth:CodeKey`.
5. **Testes de integração:** AC-1, AC-2, AC-3, AC-4, AC-9 (cadastro), AC-15, AC-16, AC-26 e respostas iguais com e sem conta.

## Recorte 2: login, sessão longa e aparelhos conectados

**Situação:** pronto.

1. **Domain:**
   - `User` ganha `FailedSignInCount`, `FailedSignInWindowStartedAt` e `LockedOutUntil`, com `RegisterFailedSignIn` (5 erros numa janela de 15 min bloqueiam por 15 min), `RegisterSuccessfulSignIn`, `IsLockedOut` e `ReplacePasswordHash` (rehash pedido pelo hasher).
   - `Session` ganha `Touch` e `IsActive` (não revogada, dentro de 60 dias sem uso e de 1 ano do login).
   - `RefreshToken` ganha `IsUsable`, `MarkUsed` e `Revoke`, e passa a ter `xmin`. Dois refresh simultâneos do mesmo token: um vence, o outro recebe 401.
2. **Application:**
   - Casos de uso: `Login`, `RefreshSession`, `Logout`, `ListSessions` (consulta) e `RevokeSession`.
   - `SessionService` passa a revogar a sessão e todos os refresh dela (logout, sair de todos, encerrar aparelho, reuso de refresh) e devolve os ids para limpar o cache depois do commit.
   - `PasswordService.Check` informa se o hash precisa ser refeito.
   - `ICurrentUser.SessionId` e `ISessionStatusCache` (limpa `session:{sid}:revoked`).
   - Repositories: `ISessionWritableRepository` (por id e dono, ativas do usuário), `ISessionSearchableRepository` (lista de aparelhos), `IRefreshTokenWritableRepository` (por hash, por sessão) e `IUserWritableRepository.GetByIdAsync`.
3. **Infrastructure:**
   - `SessionStatusChecker` consulta `session:{sid}:revoked` no `HybridCache` (1 min no Redis, 30 s local) e, na falta, o banco: sessão não revogada e conta `Active`.
   - Migração `AddSignInLockout`.
4. **Api:**
   - `AuthController`: `login`, `refresh` e `logout` (este com `[Authorize]`; os outros, anônimos com a política `auth`).
   - `MeController` com `GET /me/sessions` (política `profile`) e `DELETE /me/sessions/{id}`.
   - O JwtBearer recusa token de sessão revogada no `OnTokenValidated`.
5. **Testes de integração:** AC-5, AC-6 (bloqueio e desbloqueio após 15 min; desbloqueio pela redefinição fica para o recorte 3), AC-7, AC-10, AC-11, AC-12, AC-13, AC-14, AC-22 (segundo host com o mesmo banco e cache vazio) e AC-23.

## Recorte 3: esqueci a senha e troca de senha

**Situação:** pronto.

1. **Domain:** `User.ChangePassword` troca o hash, grava `PasswordChangedAt` e zera falhas e bloqueio. Motivos de revogação `password_reset` e `password_changed`.
2. **Application:**
   - `ForgotPassword` e `ResendCode` (agora também com `resetPassword`) usam `IEmailCodeService.PrepareAsync`, que emite o código dentro dos limites e monta o e-mail só quando há destinatário: conta pendente para `verifyEmail`, conta ativa para `resetPassword`. O handler salva e envia depois do commit.
   - `ResetPassword` confere o código, troca a senha, revoga todas as sessões, abre uma nova no aparelho e avisa por e-mail.
   - `ChangePassword` confere a senha atual (403 `auth.invalid_credentials` se errada), recusa a senha igual ao e-mail, troca a senha e revoga as outras sessões.
   - `IPasswordChangeService` concentra a troca (hash, revogação, aviso), usada pelos dois.
3. **Infrastructure:** modelos `password-reset-code` e `password-changed`; a data da troca sai como "qui 29/10 às 14:05", no fuso de São Paulo.
4. **Api:** `POST /auth/password/forgot` e `/auth/password/reset` (anônimos, política `auth`); `PUT /me/password` (`[Authorize]`).
5. **Testes de integração:** AC-8, AC-9 (redefinição e troca), desbloqueio do AC-6 pela redefinição, reenvio do código de redefinição, 401 sem token e todos os modelos de e-mail no Mailpit.

## Recorte 4: conta, celular, endereços e aparelhos de push

**Situação:** pronto.

1. **Domain:**
   - `PhoneNumber` (celular brasileiro com DDD válido, guardado em E.164) e `User.Rename`/`ChangePhoneNumber`.
   - `Cep`, `PostalLocation` (rua, bairro, cidade, UF, código IBGE e coordenadas do CEP quando houver) e `Address` (rótulo, número, complemento; até 10 por pessoa).
   - `Device` (instalação, plataforma `android`/`ios`, token FCM), que troca de dono quando outra conta entra no mesmo aparelho.
2. **Application:**
   - Casos de uso: `GetMe`, `UpdateMe`, `ListAddresses`, `AddAddress`, `UpdateAddress`, `DeleteAddress`, `RegisterDevice` e `RemoveDevice`.
   - `IAddressLookupService` resolve o CEP. A rua, o bairro e a cidade vêm da BrasilAPI, e o código IBGE vem da lista de municípios do IBGE pelo nome da cidade.
   - CEP não encontrado:
     - sem rua, bairro e cidade no request, a resposta é 400 `address.cep_not_found`;
     - com `street`, `neighborhood`, `state` e `cityIbgeCode`, o endereço é aceito, e a cidade é validada contra a lista do IBGE da UF.
   - BrasilAPI fora do ar, sem os dados manuais: 503 `address.cep_unavailable`.
3. **Infrastructure:**
   - **Criptografia de coluna:** celular, CEP, rua, número, complemento e bairro são criptografados com Data Protection. As chaves ficam na tabela `data_protection_keys`. Em produção, elas são protegidas por um certificado vindo do cofre (`DataProtection:Certificate`), e a API não sobe sem ele.
   - **Clientes HTTP:** `BrasilApiCepLookup` e `IbgeCityDirectory`, com resiliência padrão. A lista de municípios fica no `HybridCache` por 1 dia.
   - Migração `AddProfileAddressesAndDevices`.
4. **Api:** no `MeController`, todos com `[Authorize]`:
   - `GET /me` e `GET /me/addresses`, com a política `profile`;
   - `PATCH /me`;
   - `POST`, `PUT` e `DELETE /me/addresses`;
   - `PUT` e `DELETE /me/devices/{installationId}`.
5. **Testes:**
   - AC-17, AC-18, AC-24 e AC-25 (já coberto no domínio);
   - limite de 10 endereços;
   - cadastro manual com CEP não encontrado;
   - endereço e aparelho de outra pessoa respondem 404, e todas as rotas respondem 401 sem token;
   - dados criptografados no banco;
   - leitura das respostas da BrasilAPI e do IBGE com um handler HTTP falso (sem rede nos testes).

## Recorte 5: exclusão de conta e primeiro admin

**Situação:** pronto.

1. **Domain:**
   - `User.Delete` anonimiza na hora o e-mail (`{id}@removido.invalid`, o que libera o endereço para um cadastro novo), o nome ("Usuário removido"), o celular e a senha, grava `DeletedAt` e gera `user.deleted` (catálogo de [10-eventos.md](../10-eventos.md)).
   - `AdminGrant` registra quem é admin.
2. **Application:**
   - `DeleteAccount` consulta os bloqueios (`IAccountDeletionBlocker`, um por módulo; pedidos e pagamentos entram nas specs deles para o AC-20), revoga todas as sessões, apaga endereços e aparelhos, anonimiza e audita.
   - `GrantAdmin` dá o papel de admin a uma conta ativa, com auditoria.
   - `IAccountRolesService` monta os papéis do token e do `/me` (`customer` sempre, `admin` pela tabela).
3. **Api:**
   - `DELETE /me` (204).
   - Comando de linha `dotnet Vitrio.Api.dll grant-admin <email>`: roda no servidor, fora da API, e não sobe o servidor web.
4. **Testes de integração:**
   - AC-19: sessão encerrada, mesmo e-mail cria conta nova e dados pessoais apagados. As avaliações como "Usuário removido" ficam para a spec de avaliações.
   - `user.deleted` na outbox.
   - Papel de admin no token e no `/me`.
   - 401 sem token.

## Recorte 6: telas do app

**Situação:** pronto.

- **Dados** (`features/auth/data`): `AuthRepository` e `AccountRepository` são interfaces, com as implementações HTTP sobre o `ApiClient`. Os modelos (`Me`, `AuthSession`, `ConnectedSession`) leem o JSON à mão.
- **Sessão:** `SessionController` (Riverpod) carrega a conta ao abrir o app se houver tokens. Quando o `ApiClient` não consegue renovar a sessão, ele avisa pelo `SessionExpiry`, e o controller sai. O roteador manda quem não entrou para "Entrar" ao abrir telas da conta.
- **Telas:**
  - Entrar: com conta pendente, reenvia o código e abre a confirmação.
  - Criar conta: com aceite dos termos.
  - Confirmar e-mail: código de 6 números, reenvio com contagem de 60 s e "Trocar e-mail". A senha fica só na memória até confirmar.
  - Esqueci a senha: e-mail e, depois, código e senha nova.
  - Conta: dados, editar nome e celular, trocar senha, aparelhos conectados com "Sair de todos", sair e excluir conta com confirmação digitada.
- **Fora deste recorte:** a tela de endereços entra com o pedido de orçamento (spec 005); o registro do token FCM, com as notificações (spec 009).
- **Testes:** repositório HTTP, controller de sessão e os fluxos de entrar, conta pendente com código e redirecionamento para "Entrar".

## Riscos

| Risco | Mitigação |
| --- | --- |
| Testes de expiração dependentes do relógio | Host de teste com `FakeTimeProvider` próprio |
| Respostas que vazam a existência de um e-mail (tempo ou conteúdo) | Testes comparando as respostas com e sem conta; verificação de senha falsa no login |
| Código em texto em algum lugar (log, outbox, banco) | Só HMAC no banco; e-mail enviado direto do handler; teste de arquitetura e revisão de logs |
| Chave JWT perdida ou trocada | `kid` no token e suporte a uma chave anterior durante a troca (recorte 2) |
