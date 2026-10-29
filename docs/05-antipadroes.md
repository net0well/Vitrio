# Antipadrões

O que não entra no código do Vitrio, por que, e o que fazer no lugar. Revisões de código e o `/code-review` do kit usam esta lista.

## Arquitetura

| Não faça | Por quê | Faça |
| --- | --- | --- |
| Minimal APIs (`app.MapGet`, `MapGroup`, `IEndpointGroup`), mesmo quando o `/scaffold` gerar assim | Decisão do projeto (ADR-001); mistura dois estilos e quebra a organização por controller | Controller com `[ApiController]` herdando `ApiControllerBase`; adapte a saída do scaffold |
| Controller com regra de negócio, `DbContext` ou `if` de estado | Regra espalhada e sem teste de unidade | Controller só monta comando e chama `ISender` |
| `Domain` referenciando EF Core, ASP.NET ou pacotes externos | Domínio deixa de ser testável e portável | Domínio puro; mapeamento na Infrastructure |
| Repositório genérico (`IRepository<T>`) sobre EF Core | Duplica o que o `DbContext` já é e esconde consultas | `IAppDbContext` com `DbSet`s e consultas no handler |
| "Services" anêmicos que fazem tudo (`OrderService` com 30 métodos) | Classe gigante, dependências demais | Um handler por caso de uso |
| Pasta `Helpers`/`Utils` genérica | Vira depósito sem dono | Coloque perto de quem usa ou crie um value object |
| Vários tipos no mesmo arquivo (enum junto da classe, interface junto da implementação, records de request e response juntos) | Tipo difícil de achar pelo nome do arquivo e arquivos que crescem sem limite | Um tipo por arquivo, com o nome do tipo; no Dart, `part` quando a linguagem exige a mesma biblioteca |
| MediatR 13+, FluentAssertions 8+, MassTransit 9+ | Licenças comerciais | `Mediator`, asserts do xUnit, outbox/Wolverine |
| Biblioteca de mapeamento (AutoMapper, Mapster, Mapperly ou parecidas) | Esconde quais campos saem e entram, quebra em silêncio quando um nome muda e facilita vazar campo interno | `From` no DTO de resposta, `ToCommand` no request, `Select` nas consultas, tudo escrito à mão |
| Cache como fonte da verdade (sessão revogada, chave de idempotência, contador que precisa sobreviver) | Reinício ou segunda instância perdem o dado e reabrem a falha | Banco como fonte; cache só acelera leitura ([09-cache.md](09-cache.md)) |
| Broker de mensageria no MVP | Infraestrutura sem necessidade | Outbox + handlers in-process (ADR-009) |

## API

| Não faça | Por quê | Faça |
| --- | --- | --- |
| Receber entidade do domínio no corpo ou devolver entidade | Mass assignment e vazamento de campos internos | DTO de request e de response por caso de uso |
| Pegar `userId`, `customerId`, `providerId` ou `status` do corpo | Qualquer um vira outra pessoa | Ids de quem chama vêm do token (`ICurrentUser`); ids de recurso, da rota |
| Responder 403 para recurso de outro usuário | Confirma que o recurso existe | 404 |
| Mensagens de exceção ou stack trace na resposta | Expõe internals | ProblemDetails padronizado com `traceId` |
| Endpoint sem `[Authorize]` ou `[AllowAnonymous]` explícito | Ninguém sabe se era para ser público | Política padrão exige login; público é marcado |
| Paginação sem limite ou com total em busca pública | Raspagem e consultas caras | `pageSize` ≤ 30, até 10 páginas, `hasMore` |
| `POST` sensível sem `Idempotency-Key` | Pedido ou confirmação duplicados em reconexão | Chave obrigatória em criar pedido, orçamento e confirmação de pagamento |
| Rotas com verbo (`/createOrder`) ou singular inconsistente | API imprevisível | Recursos no plural, ações como sub-recurso |
| Coordenadas exatas do prestador na resposta | Expõe onde a pessoa mora | Ponto público e distância arredondada |

## Domínio e Application

| Não faça | Por quê | Faça |
| --- | --- | --- |
| Setters públicos e mudar `Status` de fora | Pula invariantes da máquina de estados | Métodos de domínio (`order.AcceptQuote()`) que devolvem `Result` |
| Exceção para regra de negócio esperada | Controle de fluxo caro e escondido | `Result` com `Error` tipado |
| `DateTime.Now`/`UtcNow` | Impossível testar horários e prazos | `IClock` (sobre `TimeProvider`) |
| Buscar o recurso e depois comparar o dono | Fácil esquecer; abre BOLA | Filtro de dono na própria consulta |
| Validação só no app | O app é contornável | FluentValidation em todo comando |
| `async void`, `.Result`, `.Wait()` | Deadlock e exceção perdida | `async Task`/`ValueTask` de ponta a ponta |
| Esquecer `CancellationToken` | Trabalho continua após o cliente desistir | Propague em toda chamada assíncrona |
| Selo ou número derivado sem regra de recálculo, ou atualizado por incremento | O selo fica para sempre ou diverge quando um evento chega duas vezes | Regra escrita em [02-regras-de-negocio.md §3](02-regras-de-negocio.md), recálculo do zero por evento e job, e invalidação do cache |
| Gerar evento de domínio em toda entidade | Ruído na outbox e handlers sem propósito | Evento só nas transições a que alguém reage |

## EF Core e banco

| Não faça | Por quê | Faça |
| --- | --- | --- |
| Lazy loading | N+1 escondido | `Include` explícito ou projeção |
| Carregar entidade para listar | Memória e tráfego à toa | `AsNoTracking()` + `Select` para DTO |
| Aplicar migrações no startup em produção | Corrida entre instâncias e deploy lento | Script idempotente aplicado no pipeline |
| `ToList()` antes de filtrar | Filtra em memória | Filtros no `IQueryable` |
| Distância calculada em C# sobre todos os prestadores | Não escala | `ST_DWithin` com índice GiST |
| Filtrar ou medir distância pelo endereço base do prestador | Variando o ponto da busca dá para achar a borda do raio e o endereço exato | Sempre `public_location` |
| `ExecuteUpdate`/`ExecuteDelete` em agregado que gera evento | Pula o interceptor: o evento some, o cache não é invalidado e as notificações não saem | Carregar em páginas, chamar o método de domínio e salvar ([10-eventos.md](10-eventos.md) §4) |
| Evento com nome, contato, endereço, texto livre ou valor | Espalha dado pessoal pela outbox e envelhece | Só ids e códigos; o handler lê o estado atual |
| Evento fora do catálogo | Ninguém sabe quem consome nem o que acontece | Evento novo entra primeiro em [10-eventos.md](10-eventos.md) §9 |
| `LIKE '%texto%'` para busca | Sem índice e sem acento | Full-text em português com `unaccent` e trigram |

## Segurança e privacidade

| Não faça | Por quê | Faça |
| --- | --- | --- |
| Logar nome, telefone, endereço, e-mail, token ou coordenada | Vazamento por log | Ids e `traceId`; política de mascaramento no Serilog |
| Segredo em `appsettings.json`, código ou app | Vaza no repositório e no binário | user-secrets em dev, cofre/variáveis em produção |
| Guardar refresh token em texto puro | Banco vazado vira sessão roubada | Hash SHA-256 e rotação com detecção de reuso |
| Token no `SharedPreferences` ou em URL | Fácil de extrair | `flutter_secure_storage`, header `Authorization` |
| Aceitar upload pela API ou publicar a foto como veio | DoS e vazamento de GPS no EXIF | URL assinada + job que valida, limpa metadados e reencoda |
| Mostrar endereço do cliente antes do aceite | Coleta de endereços por prestador mal-intencionado | Endereço só em `Scheduled` e com auditoria |
| Chamar URL vinda do usuário | SSRF | Destinos fixos por configuração |
| Swagger e painel do Hangfire abertos em produção | Mapa da API e controle de jobs expostos | Desligados ou atrás de admin + IP |

## Testes

| Não faça | Por quê | Faça |
| --- | --- | --- |
| Nome de teste com número de critério de aceite ou de spec (`AC3_...`) | Acopla código a documento; vira lixo quando a spec muda | Nome que descreve o comportamento |
| Mock de `DbContext` | Testa o mock, não a consulta | PostGIS real com Testcontainers |
| Teste que depende de ordem ou de dado de outro teste | Instável | Dados criados por teste com builders |
| Só testar o caminho feliz | Regras de estado quebram em produção | Um teste por transição inválida e por regra de autorização |

## Flutter

| Não faça | Por quê | Faça |
| --- | --- | --- |
| Cor, fonte, tamanho ou raio literais | Quebra tema escuro e consistência | Tokens do design system |
| Chamar HTTP direto da tela | Mistura UI e dados | Repositório + controller Riverpod |
| Lógica de negócio na UI (ex.: decidir se pode avaliar) | Regras divergem da API | A API diz o que é permitido (`canReview`, `allowedActions`) |
| Pedir permissão de localização sem contexto | Usuário nega para sempre | Pré-permissão explicando o uso, com alternativa "Escolher cidade" |
| `setState` espalhado em telas complexas | Estado imprevisível | Notifiers com estados explícitos |
| Ignorar estados vazio, carregando e erro | Tela em branco ou travada | Os quatro estados em toda tela de dados |
| Ícone sem rótulo, alvo menor que 44 px | Inacessível | `tooltip`/`Semantics` e tamanho mínimo |

## Processo, comentários, branches e commits

| Não faça | Por quê | Faça |
| --- | --- | --- |
| Comentário que narra o código (`// busca o pedido`) | Ruído que envelhece | Código legível; comente só o porquê não óbvio |
| Comentário citando spec, plano, fase, tarefa ou critério (`// AC-3 da spec 005`) | Acopla código a documento e envelhece | Nenhuma referência a documentos no código |
| Código comentado, `TODO` sem dono | Lixo permanente | Apague; abra issue |
| Branch com fase, spec ou número de tarefa (`fase-2`, `spec-005`, `mvp/pagamento`, `task-12`) | Nome perde sentido e expõe planejamento | `feat/confirmacao-de-pagamento` |
| Commit citando spec, plano, fase, IA ou processo ("conforme spec 005", "implementa fase 2", "gerado pelo Claude") | Histórico deve dizer o que mudou | "Adiciona confirmação de pagamento pelo prestador" |
| Rodapés automáticos no commit (`Co-Authored-By`, "Generated with") | Ruído no histórico | Mensagem só com o que foi feito |
| Commits gigantes misturando assuntos | Revisão impossível, reversão difícil | Um assunto por commit, PRs pequenos |
