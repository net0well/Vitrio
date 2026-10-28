# Plano: Fundação

**Status:** Em andamento (backend pronto; app e CI a seguir)
**Origem:** [07-roadmap.md](../07-roadmap.md), bloco Fundação (sem spec de funcionalidade)
**Arquitetura:** Clean Architecture com Controllers ([03-arquitetura.md](../03-arquitetura.md))
**Camadas afetadas:** todas (projetos novos), `infra/`
**Recorte atual:** backend. O app Flutter e o CI entram em seguida, neste mesmo plano.

## Decisões tomadas para a Fundação

| Tema | Decisão | Impacto nos docs |
| --- | --- | --- |
| Tipos de erro | `ErrorType` ganha `Unprocessable` (422) e `Unavailable` (503). O 422 de `Idempotency-Key` reutilizada fica num filtro próprio, fora do `Result`. | `03-arquitetura.md` §3, `04-padroes.md` §3 |
| Relógio | `IClock` (`UtcNow`) no `Vitrio.Domain`, implementado na Infrastructure sobre `TimeProvider`. Domínio e handlers recebem `IClock`; testes registram `FakeTimeProvider`. Fica no Domain porque as entidades recebem o relógio nos métodos de transição. | `03-arquitetura.md` §3, `04-padroes.md` §1, §2 e §4, `CLAUDE.md` |
| Lugar do `Result` | `Result`, `Result<T>`, `Error` e `ErrorType` ficam em `Vitrio.Domain/Common`, porque métodos de domínio devolvem `Result`. `PagedResult` e behaviors ficam em `Application/Common`. | `03-arquitetura.md` §3 |
| EF Core na Application | `Vitrio.Application` referencia `Microsoft.EntityFrameworkCore` (sem provedor) para `IAppDbContext` expor `DbSet<T>` e os handlers usarem as extensões assíncronas. Npgsql e NetTopologySuite ficam só na Infrastructure. | `03-arquitetura.md` §3 |
| Armazenamento local | MinIO deixou de publicar imagens da edição comunitária. O compose sobe sem storage por enquanto; a escolha (imagem antiga do MinIO, Garage, SeaweedFS) fica para quando os uploads chegarem. | `03-arquitetura.md` §10 |

## Passos do backend

1. **Solução e build** — `backend/` — base para todo o resto.
   - `global.json` na raiz do repositório (SDK 10.0.400, `rollForward: latestFeature`, runner de testes Microsoft.Testing.Platform, exigido pelo xUnit v3 no .NET 10), `Vitrio.slnx`, `Directory.Build.props` (`net10.0`, `Nullable`, `ImplicitUsings`, `TreatWarningsAsErrors`, `AnalysisLevel` latest, `EnforceCodeStyleInBuild`), `Directory.Packages.props` com Central Package Management, `.editorconfig`.
   - Projetos `src/Vitrio.{Domain,Application,Infrastructure,Api}` e `tests/Vitrio.{Domain.Tests,Application.Tests,Api.IntegrationTests,ArchitectureTests}` com as referências da regra de dependência.
   - Testes de arquitetura (NetArchTest): direção das dependências, Domain sem pacotes externos, Application sem Npgsql/Infrastructure, controllers herdam `ApiControllerBase` e só recebem `ISender`, handlers e controllers `sealed`, ninguém usa `DateTime.Now`/`UtcNow`.

2. **Blocos do domínio** — `Vitrio.Domain/Common` — o resto depende deles.
   - `Error`, `ValidationError` (erros por campo), `ErrorType`, `Result`, `Result<T>`.
   - `AggregateRoot` com `Raise`/`DomainEvents`/`ClearDomainEvents`, `IDomainEvent`, `IClock`.
   - Testes de unidade do `Result`.

3. **Application** — `Vitrio.Application` — pipeline antes de qualquer caso de uso.
   - `Mediator` 3 (source generator) e FluentValidation registrados em `AddApplication()`.
   - Behaviors na ordem: log com duração (sem dados pessoais, só nome da mensagem, duração e código de erro) → validação (devolve `ValidationError` dentro do `Result`, sem exceção).
   - `Abstractions/IAppDbContext`, `Abstractions/ICurrentUser`.
   - O behavior de transação entra com o primeiro comando que fizer mais de um `SaveChanges`.

4. **Infrastructure e banco** — `Vitrio.Infrastructure`, `infra/` — a API precisa do banco para o health check.
   - `AppDbContext` com Npgsql + NetTopologySuite, snake_case (`EFCore.NamingConventions`), extensões `postgis`, `unaccent`, `pg_trgm`, `citext`.
   - Tabela `outbox_messages` e interceptor de `SaveChanges` que transforma eventos de domínio em linhas da outbox na mesma transação. O despachante (`BackgroundService`) entra com o primeiro handler de evento.
   - `SystemClock : IClock` sobre `TimeProvider`.
   - Migração inicial `AddExtensionsAndOutbox`.
   - `infra/docker-compose.yml` com PostGIS 17-3.6 e Seq.
   - A tabela `audit_events` entra com a primeira ação auditada (login, na conta).

5. **API** — `Vitrio.Api` — composição e regras transversais.
   - Controllers com `UnmappedMemberHandling.Disallow` e enums como string; corpo máximo de 64 KB no Kestrel.
   - `ApiControllerBase` traduzindo `Result` em 200/204 e `Error` em ProblemDetails (`type` = `https://vitrio.app/problems/<codigo>`, `traceId`, `errors` em validação). ProblemDetails também para 404/405/500 do framework, sem stack trace.
   - `Asp.Versioning.Mvc` com versão na URL.
   - Autenticação JWT Bearer configurada por opções validadas no startup (emissor, audiência; chaves ES256 chegam com a conta). `FallbackPolicy` exige usuário autenticado; políticas `Customer`, `Provider` e `Admin` por claim de papel.
   - Rate limiting com as políticas de [08-seguranca.md](../08-seguranca.md) §5, particionado por usuário ou IP, 429 em ProblemDetails com `Retry-After`. Políticas com duas janelas (`auth`, `orders`) usam um limitador encadeado.
   - Cabeçalhos de segurança e `Cache-Control: no-store` em respostas autenticadas; `ForwardedHeaders` só para proxies configurados.
   - `/health/live` e `/health/ready` (banco).
   - OpenAPI nativo integrado ao versionamento (`Asp.Versioning.OpenApi`, um documento por versão) + Scalar só em Development.
   - Serilog com enricher que mascara propriedades sensíveis; OpenTelemetry (ASP.NET Core, HttpClient, Npgsql) com exportação OTLP quando configurada.
   - `CurrentUser : ICurrentUser` lendo as claims do `HttpContext`.

6. **Testes de integração** — `Vitrio.Api.IntegrationTests` — prova de que as regras transversais funcionam.
   - `WebApplicationFactory` + Testcontainers (PostGIS), migração aplicada no início da coleção.
   - `/health/ready` 200 com banco no ar; `/health/live` 200.
   - Controller de teste adicionado só no projeto de testes para exercitar `ApiControllerBase`: cada `ErrorType` vira o status certo com ProblemDetails no formato combinado; validação devolve `errors` por campo em camelCase.
   - Rota protegida sem token ou com token não assinado → 401 em ProblemDetails. Rota inexistente sem token também responde 401, porque a `FallbackPolicy` vale para requisições sem endpoint; assim a API não revela quais rotas existem.
   - Todo action de todo controller tem `[Authorize]` ou `[AllowAnonymous]` explícito e uma política de rate limit.
   - Cabeçalhos de segurança presentes; `no-store` com `Authorization`; campo desconhecido no JSON → 400; limite de 64 KB configurado no Kestrel (o `TestServer` não aplica esse limite, então o teste confere a opção).
   - Interceptor grava eventos de domínio na outbox na mesma transação.
   - Testes de unidade do limitador encadeado e do mascaramento de dados pessoais nos logs.

## Passos do app (próximo recorte)

7. Projeto Flutter em `mobile/` com flavors `dev`, `staging`, `prod`, Riverpod, go_router e `ApiClient` com refresh e ProblemDetails.
8. Design system em `core/design_system/` conforme [design-system/flutter.md](../design-system/flutter.md), fontes, ícones e ícone do app.
9. Telas base: boas-vindas e esqueleto das abas.

## Passos de entrega (próximo recorte)

10. GitHub Actions: build, testes com Testcontainers, `dotnet format --verify-no-changes`, `dotnet list package --vulnerable --include-transitive`, CodeQL, gitleaks, `flutter analyze` e `flutter test`.
11. Dockerfile da API e serviço da API no compose. Na configuração de produção:
    - `ForwardedHeaders:KnownProxies` com o IP do Caddy; sem isso todo cliente cai no mesmo balde de rate limit por IP;
    - `AllowedHosts` com o domínio real da API em vez de `*`;
    - Caddy expõe só `/api/*`; `/health/*` fica acessível apenas para o monitoramento, porque `/health/ready` abre conexão com o banco a cada chamada.

## Pronto quando

- `dotnet build` sem avisos, `dotnet test` verde e `dotnet format --verify-no-changes` limpo.
- `GET /health/ready` responde 200 localmente com o compose e, depois, em staging.
- O app abre em dev apontando para a API local.
- O CI está verde.

## Riscos

| Risco | Mitigação |
| --- | --- |
| `Mediator` 3 com behaviors genéricos e o source generator | Behaviors sem restrição genérica; a falha de validação monta o `Result` certo por reflexão, uma vez por tipo |
| `TreatWarningsAsErrors` com analisadores recomendados travando o começo | Começar com `AnalysisLevel` latest no modo padrão e subir o rigor por regra, com motivo no `.editorconfig` |
| Testcontainers no Windows exige Docker Desktop aberto | Documentado nos comandos do `CLAUDE.md`; o CI roda em Linux |
| Limitador encadeado é código próprio | Testes de integração cobrindo as duas janelas |
