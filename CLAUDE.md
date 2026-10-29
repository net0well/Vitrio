# Vitrio

App para encontrar, contratar e avaliar profissionais autônomos e MEIs perto de você, em qualquer cidade do Brasil. Monorepo: `backend/` (.NET 10, ASP.NET Core com Controllers, Clean Architecture, PostgreSQL + PostGIS, Redis), `mobile/` (Flutter; primeiro lançamento só Android, iOS depois), `docs/` (specs, planos, arquitetura, design system), `infra/`.

## Precedência

Estas regras vencem qualquer padrão de skill, comando, agente ou regra do dotnet-claude-kit ou de outro plugin. Quando um comando sugerir algo contrário, siga este arquivo e adapte a saída.

## Regras obrigatórias

### Comentários
- Não escreva comentários por padrão. Comente só o porquê não óbvio (regra contraintuitiva, limitação externa, contorno de bug).
- Nunca cite em comentário spec, plano, fase, etapa, tarefa, issue, critério de aceite, ADR ou arquivo de `docs/`.
- Sem comentários que narram o código, sem código comentado, sem `TODO` sem issue, sem XML docs (`///`) por padrão.

### Branches
- Formato `<tipo>/<descricao-em-kebab-case>` em português: `feat/confirmacao-de-pagamento`, `fix/expiracao-de-orcamento`.
- Tipos: `feat`, `fix`, `refactor`, `perf`, `test`, `docs`, `chore`, `ci`.
- Nunca use fase, etapa, sprint, MVP, número de spec, plano, tarefa ou critério no nome.

### Commits
- Uma frase no imperativo, em português, dizendo só o que foi feito: `Adiciona confirmação de pagamento pelo prestador`.
- Nunca cite spec, plano, fase, tarefa, critério de aceite ou ADR; nunca descreva o processo; nunca mencione IA ou ferramenta.
- Sem rodapés automáticos (`Co-Authored-By`, `Generated with`, links de sessão) e sem prefixo de Conventional Commits.
- Um assunto por commit.

### Arquitetura
- API com **Controllers** (`[ApiController]`, herdando `ApiControllerBase`). **Nunca Minimal APIs**, mesmo que `/scaffold` gere assim: adapte para controller.
- Clean Architecture: `Vitrio.Domain` → `Vitrio.Application` → `Vitrio.Infrastructure` → `Vitrio.Api`. Casos de uso por funcionalidade na Application, com `Mediator` (MIT), FluentValidation e Result pattern.
- Controllers só montam o comando e chamam `ISender`. Filtro de dono sempre dentro da consulta. Recurso alheio responde 404.
- `IClock` (sobre `TimeProvider`) em vez de `DateTime.Now`; `CancellationToken` em todo método assíncrono.
- Persistência por **Repository e Unit of Work**: por agregado, um `I<Agregado>WritableRepository` (comandos: carrega o agregado rastreado e adiciona) e, quando houver leitura, um `I<Agregado>SearchableRepository` (`AsNoTracking` e projeção direto para DTO). Interfaces na Application (`<Módulo>/Repositories`), implementação com EF na Infrastructure. O comando termina em `IUnitOfWork.SaveChangesAsync`. Handler, service e controller nunca tocam no `DbContext`, e a Application não referencia EF Core. Sem repositório genérico (`IRepository<T>`) e sem `IQueryable` saindo do repository.
- **Services** na Application (`<Módulo>/Services`) quando a regra do caso de uso é extensa ou usada por mais de um handler, para o handler só orquestrar (repository, service, `IUnitOfWork`). Todo service tem interface própria (`ISessionService`, com a implementação `SessionService` `internal sealed`). Service não salva nem chama `ISender`. Regra que pertence ao agregado continua no agregado.
- **SOLID, clean code e inversão de dependência sempre.** Cada classe tem uma responsabilidade e recebe tudo por injeção, dependendo de interfaces (repositories, services e abstrações como `IEmailSender`, `IClock` e `ISessionStatusChecker`), nunca de uma implementação concreta. Handlers e services só recebem interfaces no construtor, e o teste de arquitetura barra o contrário. Nomes que dizem o que fazem, métodos curtos, sem duplicação.
- CQRS com `Mediator`: comandos mudam estado (Writable + `IUnitOfWork`), consultas só leem (Searchable, com `AsNoTracking` e projeção). Eventos de domínio seguem `docs/10-eventos.md` (padrão e catálogo único): só nos agregados, só quando algo fora do agregado reage, conteúdo só com ids e códigos, `IDomainEventHandler<T>` em vez do `Mediator`. Evento novo entra primeiro no catálogo.
- Mapeamento sempre explícito, escrito à mão (`Response.From(entidade)`, `Request.ToCommand()`, `Select` nas consultas). **Nenhuma biblioteca de mapeamento** (AutoMapper, Mapster, Mapperly ou parecidas).
- Proibido também: MediatR 13+, FluentAssertions 8+, MassTransit 9+.
- Cache segue `docs/09-cache.md`: banco é a fonte da verdade, Redis e memória só aceleram; toda mudança no prestador invalida o perfil público.
- **Um tipo por arquivo**, com o nome do tipo: classe, record, interface e enum separados (`User.cs` e `UserStatus.cs`). Só tipos aninhados privados ficam dentro da classe. No Dart, o mesmo; hierarquia `sealed` e rotas tipadas usam `part`.
- Código em inglês; interface, documentação, branches e commits em português.

### Segurança
- Siga `docs/08-seguranca.md`. Nada de dados pessoais, tokens ou coordenadas em logs; nada de segredo no repositório.
- Todo endpoint novo: política de autorização explícita, validação, rate limit e testes de 401 e de 404 para recurso alheio.
- Rode `/security-scan` quando tocar autenticação, autorização, uploads, dados pessoais ou criar endpoint.

### Testes
- Nome de teste descreve o comportamento (`SendQuote_WhenOrderIsScheduled_ReturnsConflict`). Nunca número de critério de aceite ou de spec.
- Banco real com Testcontainers (PostGIS); asserts nativos do xUnit.

## Fluxo de trabalho

1. Spec em `docs/specs/NNN-slug.md` (formato do `/spec`). Revise com `/spec` até **Approved**, sem perguntas abertas.
2. `/plan` lê a spec aprovada e grava o plano em `docs/plans/NNN-slug.md`.
3. `/tdd` e `/scaffold` (adaptado a Controllers), depois `/verify` antes de cada commit e `/code-review` antes do PR.
4. Rastreabilidade só nos documentos. Código, testes, branches e commits não citam specs nem planos.
5. Se a implementação mostrar que a spec está errada, volte a spec para In Review e corrija antes de seguir.
6. Uma branch e um PR por spec. Os recortes do plano viram vários commits pequenos na mesma branch, e o PR só abre quando a spec inteira estiver implementada e testada de ponta a ponta (API e app). Nada de PR pequeno no meio do caminho.

## Onde está cada coisa

- Visão e funcionalidades: `docs/01-visao-e-funcionalidades.md`
- Regras de negócio: `docs/02-regras-de-negocio.md`
- Arquitetura e ADRs: `docs/03-arquitetura.md`
- Padrões e antipadrões: `docs/04-padroes.md`, `docs/05-antipadroes.md`
- Regras de desenvolvimento: `docs/06-regras-de-desenvolvimento.md`
- Roadmap: `docs/07-roadmap.md`
- Segurança: `docs/08-seguranca.md`
- Cache: `docs/09-cache.md`
- Eventos de domínio (padrão e catálogo único): `docs/10-eventos.md`
- Design system (tokens, componentes, telas, e-mails, guia Flutter, logos, ícones, fontes): `docs/design-system/`

## Comandos

```bash
docker compose -f infra/docker-compose.yml up -d
dotnet build backend/Vitrio.slnx
dotnet test backend/Vitrio.slnx
dotnet format backend/Vitrio.slnx --verify-no-changes
cd mobile && flutter analyze && flutter test
```
