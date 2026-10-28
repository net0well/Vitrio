# Vitrio — documentação

Vitrio é um app para encontrar, contratar e avaliar profissionais autônomos e MEIs perto de você, em qualquer cidade do Brasil. O cliente acha quem atende perto (pelo GPS ou escolhendo a cidade), pede orçamento, aceita, recebe o serviço e avalia. O prestador monta a vitrine dele, responde pedidos, confirma o pagamento combinado e constrói reputação.

- **App:** Flutter, um app com modo cliente e modo prestador. Primeiro lançamento só Android; iOS depois.
- **API:** .NET 10, ASP.NET Core com **Controllers**, Clean Architecture, PostgreSQL + PostGIS, Redis como cache.
- **Design:** sistema de design Vitrio (artifact "Vitrio — Sistema de design") e protótipo navegável (artifact "Vitrio — Protótipo do app").

## Mapa dos documentos

| Documento | Para que serve |
| --- | --- |
| [01-visao-e-funcionalidades.md](01-visao-e-funcionalidades.md) | Problema, públicos, proposta de valor e a lista completa de funcionalidades, com o que entra no MVP |
| [02-regras-de-negocio.md](02-regras-de-negocio.md) | Ciclo de vida do pedido, pagamento confirmado pelo prestador, avaliações, localização, verificação MEI, privacidade e moderação |
| [03-arquitetura.md](03-arquitetura.md) | Visão de sistema, solução .NET, modelo de domínio, banco e geolocalização, API, app Flutter, infraestrutura, segurança e decisões (ADRs) |
| [04-padroes.md](04-padroes.md) | Como escrever código no backend e no app: camadas, CQRS, Result, validação, EF Core, API, testes, Flutter |
| [05-antipadroes.md](05-antipadroes.md) | O que não fazer, com o motivo e a alternativa |
| [06-regras-de-desenvolvimento.md](06-regras-de-desenvolvimento.md) | Regras de trabalho para pessoas e para o Claude: comentários, branches, commits, uso do dotnet-claude-kit |
| [07-roadmap.md](07-roadmap.md) | Ordem de construção, dependências entre specs e critérios de pronto |
| [08-seguranca.md](08-seguranca.md) | Modelo de ameaças, autenticação, autorização, limites, uploads, LGPD, auditoria e verificação contínua |
| [09-cache.md](09-cache.md) | Camadas de cache (memória e Redis), o que fica em cache, invalidação, sessões revogadas, idempotência, cabeçalhos HTTP e cache no app |
| [specs/](specs/) | Uma spec por funcionalidade, no formato do `/spec` do dotnet-claude-kit, em status Draft |
| [design-system/](design-system/) | Identidade, tokens, componentes, plano de telas, guia Flutter, logos, ícones e fontes |

## Fluxo de trabalho com o dotnet-claude-kit

1. `/dotnet-init` na pasta `backend/` (escolha **Merge** para manter o `CLAUDE.md` do projeto).
2. Para cada funcionalidade, na ordem do roadmap: `/spec` revisa a spec em Draft desta pasta até ficar **Approved** (sem perguntas abertas).
3. `/plan` lê a spec aprovada e gera o plano em cima da Clean Architecture com Controllers.
4. `/scaffold` gera os arquivos (adaptando endpoints para Controllers), `/tdd` escreve os testes a partir dos critérios de aceite, `/verify` fecha o ciclo antes do commit.
5. Specs e planos vivem em `docs/`. Código, nomes de teste, branches e commits **nunca** citam spec, plano, fase ou número de critério (ver [06-regras-de-desenvolvimento.md](06-regras-de-desenvolvimento.md)).

## Glossário

| Termo | Significado | Nome no código |
| --- | --- | --- |
| Cliente | Quem procura e contrata serviço | `Customer` |
| Prestador | Autônomo ou MEI que oferece serviço | `Provider` |
| Vitrine | Perfil público do prestador | `ProviderProfile` |
| Pedido | Solicitação de orçamento e tudo que vem depois | `Order` |
| Orçamento | Valor e condições enviados pelo prestador | `Quote` |
| Confirmação de pagamento | Registro do prestador de que recebeu, com valor e meio | `PaymentConfirmation` |
| Avaliação | Nota, tags e comentário do cliente sobre um pedido | `Review` |
| Área de atendimento | Cidade base, raio e bairros do prestador | `ServiceArea` |
| Selo MEI verificado | CNPJ ativo e optante pelo MEI na Receita | `MeiVerification` |
| Denúncia | Sinalização de perfil, foto ou avaliação | `Report` |
