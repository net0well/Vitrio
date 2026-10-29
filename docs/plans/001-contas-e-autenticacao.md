# Plano: Contas e autenticação

**Status:** Em andamento
**Spec:** [001-contas-e-autenticacao](../specs/001-contas-e-autenticacao.md) (Approved)
**Arquitetura:** Clean Architecture com Controllers ([03-arquitetura.md](../03-arquitetura.md))
**Camadas afetadas:** Domain (`Accounts`), Application (`Accounts`), Infrastructure (persistência, senha, códigos, e-mail, tokens), Api (`AuthController`, `MeController`), app Flutter.

## Recortes

Cada recorte é uma branch e um PR, nesta ordem. Os critérios de aceite da spec ficam mapeados no recorte que os cobre.

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

1. **Domain:** `User` (`PendingVerification` → `Active`), `EmailCode`, `EmailAddress`, `DisplayName`; erros em `AccountErrors`. Testes de unidade das transições e dos value objects.
2. **Application:** `Register`, `VerifyEmail`, `ResendCode` com validadores; abstrações `IPasswordHasher`, `IVerificationCodes`, `IEmailSender`, `ITokenIssuer`; `PasswordPolicy`. Testes de validadores e da política.
3. **Infrastructure:** configurações EF (`users`, `email_codes`, `sessions`, `refresh_tokens`), migração `AddAccounts`; hasher de senha, códigos HMAC, `SmtpEmailSender` e modelos de e-mail; `TokenIssuer` ES256 com emissão de sessão e refresh (hash SHA-256 do refresh).
4. **Api:** `AuthController` com `POST /auth/register`, `/auth/verify-email`, `/auth/resend-code`; `[AllowAnonymous]` e política `auth`; configuração de `Email:Smtp` (Mailpit em Development) e `Auth:CodeKey`.
5. **Testes de integração:** AC-1, AC-2, AC-3, AC-4, AC-9 (cadastro), AC-15, AC-16, AC-26 e respostas iguais com e sem conta.

## Recortes seguintes

Detalhados aqui quando o recorte anterior entrar na `main`.

## Riscos

| Risco | Mitigação |
| --- | --- |
| Testes de expiração dependentes do relógio | Host de teste com `FakeTimeProvider` próprio |
| Respostas que vazam a existência de um e-mail (tempo ou conteúdo) | Testes comparando as respostas com e sem conta; verificação de senha falsa no login |
| Código em texto em algum lugar (log, outbox, banco) | Só HMAC no banco; e-mail enviado direto do handler; teste de arquitetura e revisão de logs |
| Chave JWT perdida ou trocada | `kid` no token e suporte a uma chave anterior durante a troca (recorte 2) |
