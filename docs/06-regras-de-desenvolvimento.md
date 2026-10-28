# Regras de desenvolvimento

Valem para pessoas e para o Claude. **Precedência:** estas regras e o `CLAUDE.md` do projeto vencem qualquer padrão de skill, comando, agente ou regra do dotnet-claude-kit ou de outro plugin. Quando um comando do kit sugerir algo contrário (Minimal API, citar a spec no commit, nome de teste com número de critério), siga este documento e adapte a saída.

## 1. Comentários no código

- Por padrão, **não escreva comentários**. Nomes claros, funções pequenas e testes explicam o quê.
- Comente só quando o **porquê** não é óbvio pelo código: uma regra de negócio contraintuitiva, uma limitação de biblioteca, um contorno de bug externo, um cálculo com fonte (ex.: fórmula da média bayesiana).
- **Nunca** cite em comentário: spec, plano, fase, etapa, tarefa, issue, critério de aceite, ADR, este documento ou qualquer arquivo de `docs/`.
- Sem comentários que narram o código, sem código comentado, sem `TODO` sem issue, sem cabeçalhos de autoria ou data.
- Sem XML docs (`///`) por padrão. A documentação da API vem de atributos (`[EndpointSummary]`, `[EndpointDescription]`, `[ProducesResponseType]`). Use `///` só em bibliotecas reaproveitáveis, se um dia houver.
- O mesmo vale para Dart: sem `//` narrativo e sem `///` por padrão.

## 2. Branches

- Formato: `<tipo>/<descricao-curta-em-kebab-case>`, em português, descrevendo a mudança.
- Tipos: `feat`, `fix`, `refactor`, `perf`, `test`, `docs`, `chore`, `ci`.
- Exemplos válidos: `feat/busca-por-cidade`, `feat/confirmacao-de-pagamento`, `fix/expiracao-de-orcamento`, `chore/atualiza-pacotes`.
- **Proibido** no nome: fase, etapa, sprint, MVP, número de spec, número de plano, número de tarefa ou de critério. Ex. inválidos: `fase-2/pedidos`, `spec-005-pagamento`, `mvp/avaliacoes`, `task-12`, `plan-03`.
- Branches curtas; integre na `main` assim que o recorte estiver pronto e testado.

## 3. Commits

- Uma frase no imperativo, em português, dizendo **o que foi feito**. Até ~72 caracteres no título; corpo opcional explicando o porquê, também sem referências a documentos.
- Exemplos: `Adiciona confirmação de pagamento pelo prestador`, `Corrige expiração de orçamentos no fuso da cidade`, `Remove EXIF das fotos antes de publicar`.
- **Proibido**: citar spec, plano, fase, tarefa, critério de aceite ou ADR ("conforme spec 006", "fase 3", "AC-2"); descrever o processo ("implementando o plano", "seguindo a revisão"); mencionar IA ou ferramenta ("gerado pelo Claude"); rodapés automáticos (`Co-Authored-By`, `Generated with`, links de sessão).
- Sem prefixos de Conventional Commits no título (a mensagem já diz o que foi feito); o tipo fica no nome da branch.
- Um assunto por commit. Formatação e refatoração separadas de mudança de comportamento.

## 4. Pull requests

- Título no mesmo estilo do commit. Descrição: o que mudou, como testar, riscos. Sem citar spec ou plano no título; na descrição, se útil para revisão, pode haver link para `docs/specs/...` (o PR não entra no histórico do código).
- Checklist antes de pedir revisão:
  - `dotnet build` sem avisos e `dotnet test` verde;
  - `dotnet format --verify-no-changes`;
  - `/verify` e `/code-review` do kit sem pendências críticas;
  - `/security-scan` quando o PR toca autenticação, autorização, uploads, dados pessoais ou um endpoint novo;
  - checklist de segurança de [08-seguranca.md](08-seguranca.md) respondido;
  - migração revisada (se houver) e app Flutter compilando (`flutter analyze`, `flutter test`).

## 5. Arquitetura e código

- API com **Controllers**. Nunca Minimal APIs. Ao usar `/scaffold`, peça ou adapte para controller herdando `ApiControllerBase`, com handler, validador e DTOs na Application.
- Clean Architecture com casos de uso por funcionalidade; regras em [03-arquitetura.md](03-arquitetura.md) e [04-padroes.md](04-padroes.md).
- Pacotes com licença comercial (MediatR 13+, AutoMapper 15+, FluentAssertions 8+, MassTransit 9+) não entram; o `/outdated` do kit avisa.
- Código em inglês; interface, documentação, branches e commits em português.
- Sem dados pessoais em logs, sem segredos no repositório.

## 6. Specs e planos com o dotnet-claude-kit

1. **Spec:** cada funcionalidade tem uma spec em `docs/specs/NNN-slug.md` (formato do `/spec`: Problem, Scope, Domain Model, API Contract, Authorization, Edge Cases & Failure Modes, Non-Functional Requirements, Integrations, Acceptance Criteria em Given/When/Then, Deferred Decisions, Open Questions). As specs iniciais estão em **Draft**; rode `/spec` sobre cada uma para fechar as perguntas abertas até **Approved**.
2. **Plano:** `/plan` lê a spec aprovada, detecta a Clean Architecture e mapeia os critérios de aceite para passos. Planos ficam em `docs/plans/` com o mesmo slug da spec.
3. **Construção:** `/tdd` cria testes a partir dos critérios de aceite, **com nomes de comportamento** (não `AC1_...`); `/scaffold` gera os arquivos adaptados a Controllers; `/verify` antes de cada commit.
4. **Rastreabilidade fica nos documentos, não no código:** a spec pode listar os testes que cobrem cada critério e o plano pode listar commits; o código, os testes, as branches e os commits não citam spec nem plano.
5. Se a implementação revelar que a spec está errada, a spec volta para **In Review**, é corrigida e aprovada de novo antes de seguir.

## 7. Definição de pronto

Uma funcionalidade está pronta quando:

- todos os critérios de aceite da spec aprovada têm teste automatizado passando;
- os estados vazio, carregando e erro existem no app;
- autorização testada (401 sem token, 404 para recurso alheio);
- textos revisados no tom do design system;
- métricas e logs do fluxo no lugar, sem dados pessoais;
- documentação atualizada quando a regra de negócio mudou.
