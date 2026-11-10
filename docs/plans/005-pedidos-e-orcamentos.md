# Plano: Pedidos e orçamentos

**Status:** Em andamento
**Spec:** [005-pedidos-e-orcamentos](../specs/005-pedidos-e-orcamentos.md) (Approved)
**Arquitetura:** Clean Architecture com Controllers ([03-arquitetura.md](../03-arquitetura.md))
**Camadas afetadas:**
- Domain: módulo novo `Orders` (agregado `Order`, `Quote`, `OrderPhoto`, value objects, erros e eventos); `Providers` (métodos dos números derivados e o evento `ProviderStatsUpdated`).
- Application:
  - `Orders`: casos de uso do cliente e do prestador, agenda, sugestões, jobs (comandos) e handlers de evento;
  - `Providers`: números do Painel e o cache do perfil no `ProviderStatsUpdated`;
  - `Common`: comportamento de idempotência no pipeline;
  - `Abstractions`: armazenamento privado de fotos e a fila de fotos de pedido.
- Shared: nada novo previsto; o que surgir de técnico e usado por mais de uma camada entra lá.
- Infrastructure: tabelas `orders`, `order_photos` e `idempotency_keys`, colunas cifradas, índices, repositories, bucket privado e a migração.
- Jobs: expirar pedidos e orçamentos, concluir agendados esquecidos, apagar dados de pedidos antigos, recalcular o tempo de resposta e processar fotos de pedido.
- Api: `OrdersController`, `ProviderOrdersController`, agenda e Painel; filtro do cabeçalho `Idempotency-Key`; correção da página de compartilhar.
- App Flutter: widgets novos do design system e as telas de pedidos, orçamento, agenda e Painel, iguais às pranchetas aprovadas.

## Recortes

Todos na branch `feat/pedidos-e-orcamentos`, em vários commits. O PR só abre quando todos estiverem prontos e testados de ponta a ponta.

| # | Recorte | Critérios |
| --- | --- | --- |
| 1 | Base técnica: idempotência no pipeline, armazenamento privado de fotos, `ProviderStatsUpdated` | AC-2 (mecanismo) |
| 2 | Domínio do pedido: estados, prazos, orçamento, ajuste, cancelamento, conclusão, retenção, `allowedActions` e eventos, com testes de domínio | AC-4 a AC-7, AC-9, AC-10, AC-14, AC-15, AC-22 a AC-25, AC-28, AC-29 (regras) |
| 3 | Persistência: tabelas, cifra, índices, repositories e migração | — |
| 4 | API do cliente: fotos, criar, listar, detalhe, aceitar e recusar orçamento, cancelar, concluir, sugestões e cópia de fotos | AC-1 a AC-3, AC-6 a AC-11, AC-18, AC-19, AC-22, AC-23, AC-30, AC-31, AC-36, AC-37 |
| 5 | API do prestador: listas, detalhe com liberação e auditoria, orçamento, recusa, ajuste, cancelar, concluir, agenda e Painel | AC-4, AC-5, AC-12 a AC-14, AC-17, AC-20, AC-24 a AC-29, AC-35 |
| 6 | Jobs e handlers: expiração, conclusão automática, retenção, fotos, tempo de resposta, serviços feitos, cancelamentos tardios e exclusão de conta | AC-8, AC-16, AC-32 a AC-34 |
| 7 | Correção dos marcadores da página de compartilhar | AC-21 |
| 8 | App: widgets do design system (controle segmentado, linha do tempo, card de orçamento, calendário, lista de motivos, ícone `phone`) | — |
| 9 | App do cliente: Pedir orçamento, Acompanhar pedido, pedido recusado com sugestões, folha de cancelamento, Pedidos e o card da Início | AC-36 e as telas |
| 10 | App do prestador: Painel, Pedidos, Responder pedido, folha de recusa, Pedido agendado e Ajustar horário | AC-35 e as telas |
| 11 | App: Agenda em calendário | AC-26, AC-27 nas telas |
| 12 | Fechamento: revisões de back e de front, fluxo de ponta a ponta no emulador e PR | Todos |

## Decisões técnicas

| Tema | Decisão |
| --- | --- |
| Agregado | `Order` em `Domain/Orders`, raiz com `Quote` como parte do agregado (um orçamento vigente, com `Version`) e `OrderPhoto` como entidade filha. Toda transição é método do agregado que devolve `Result` e gera o evento do catálogo (`order.*`, `quote.*`, `order.rescheduled`). Concorrência por `xmin`. Erros em `OrderErrors` com códigos estáveis (`order.cannot_quote`, `quote.expired`, `order.reschedule_limit`...). |
| Horários | O orçamento guarda o que o prestador escolheu no fuso da cidade (`Date`, `Start`, `Duration`) e o agregado calcula `ScheduledStart`/`ScheduledEnd` em UTC com o `TimeZoneInfo` da cidade do pedido, que o handler lê do repositório de cidades e passa ao método de domínio. `RespondBy` sai do mesmo jeito (fim do período no fuso da cidade, limitado a 48 h). Nada de `DateTime.Now`: sempre `IClock`. |
| Prazos e ações | `RespondBy`, validade do orçamento, janela do endereço (`ScheduledEnd` + 7 dias), conclusão automática e retenção (90 dias) são regras do agregado, com testes de domínio. `allowedActions` sai de um método do agregado (`AllowedActionsFor(parte, agora)`), usado pelas duas visões, para o app não repetir regra. |
| Dados pessoais | `OrderAddress` e `OrderContact` como tipos próprios do EF (`OwnsOne`, porque a cifra percorre os tipos de entidade) com as colunas marcadas como dado pessoal (`IsPersonalData()`, cifradas pelo `PersonalDataProtector` que já existe). `CustomerDisplayName` ("Mariana F.") fica sem cifra e sobrevive à retenção. `ErasePersonalData` zera os dois e preenche `PersonalDataErasedAt`. |
| Idempotência | Tabela `idempotency_keys` (`user_id`, `key`, `request_hash`, `status_code`, `response_body`, `created_at`), única por (`user_id`, `key`). Comandos que precisam dela implementam `IIdempotentCommand` (chave e hash do corpo, montados no controller a partir do cabeçalho). Um `IdempotencyBehavior` roda dentro da transação do comando: mesma chave e mesmo hash devolvem a resposta guardada; hash diferente, 422 `idempotency.key_reused`; corrida pelo índice único (`UniqueConstraintException`) relê a resposta gravada. A chave e a resposta são gravadas na mesma transação do comando. Job diário apaga chaves com mais de 24 h. |
| Fotos do pedido | Mesmo fluxo da vitrine: URL assinada de envio no bucket de envios, confirmação na criação do pedido e processamento por job (`IPhotoProcessingQueue.EnqueueOrderPhoto`), que limpa os metadados e grava o WebP num **bucket privado** novo (`vitrio-private`). Leitura por URL assinada de 10 min, emitida só na visão das duas partes. Interface própria e pequena na Application (`IPrivateMediaStorage`: salvar, URL de leitura, copiar para envios, apagar), implementada no S3 da Infrastructure. A cópia de "Pedir para este" copia o WebP processado para uma chave nova no bucket de envios, que entra no pedido novo pelo fluxo normal. |
| Repositories | `IOrderWritableRepository` (carrega o pedido rastreado por id e dono — cliente ou prestador —, adiciona, e páginas para os jobs) e `IOrderSearchableRepository` (visões do cliente e do prestador, listas por aba, agenda por intervalo, Painel, contagens para os limites, amostra do tempo de resposta), em `Orders/Repositories`. Filtro de dono dentro de cada consulta. Projeção direta para o DTO. |
| Serviços | `IOrderRequestService` (celular, vitrine, serviço, categoria, limites, endereço, fuso e ponto da cidade e chaves das fotos, montando o `OrderRequest`), `ICustomerOrderViewService` (visão do cliente, usada pela consulta e pelas ações) e `IProviderStatsService` (amostra e mediana do tempo de resposta, contagem de serviços feitos e de cancelamentos tardios), cada um com interface e implementação `internal sealed`. Nenhum salva. O fuso fica no próprio pedido (`OrderPlace.TimeZoneId`), então não houve serviço de horários; a URL de leitura das fotos sai do repository de leitura, como as URLs públicas da busca. |
| Sugestões | Reaproveitam `IProviderListingSearchableRepository.SearchAsync` com a categoria do pedido, o ponto do cliente guardado no pedido, `take 3` e um critério novo para excluir o prestador do pedido. Só em `Declined` e `Expired`. A resposta usa o mesmo `ProviderSearchItem` da busca. |
| Agenda | Uma consulta por intervalo (até 42 dias) no fuso da cidade do prestador: `Scheduled`, `Quoted` dentro da validade (pelo horário proposto) e concluídos (`Completed`, `PaymentConfirmed`, `PaymentDisputed`), com `kind`, horário local, título, nome do cliente pela regra da janela e bairro. Índice (`provider_id`, `scheduled_start`). O app agrupa por dia para as marcas e a lista. |
| Painel | `ProviderDashboardResponse` ganha `NewOrders`, `ServicesThisMonth` e `LatestNewOrders` (os 3 `Sent` mais recentes), lidos pelo `IOrderSearchableRepository`. |
| Números do prestador | Métodos no `Provider` (`UpdateResponseTime`, `UpdateCompletedOrders`, `UpdateLateCancellations`) que só geram `ProviderStatsUpdated` quando o valor muda; `InvalidateProviderProfileCache` passa a ouvir esse evento. Handlers `RecalculateProviderResponseTime`, `RecalculateCompletedOrders` e `RecordLateCancellation` recalculam do zero a partir do estado, como no catálogo. O job diário do tempo de resposta faz a janela de 90 dias andar. |
| Exclusão de conta | `ScheduledOrdersBlockDeletion` implementa o `IAccountDeletionBlocker` que já existe (409 com pedido `Scheduled`, como cliente ou prestador). `CancelOpenOrdersOfRemovedUser` ouve `UserDeleted` e cancela os `Sent` e `Quoted` da pessoa com o motivo "Conta excluída". |
| Jobs | No `Vitrio.Jobs`, cada job só envia um comando: `ExpireOrdersCommand` (5 min), `CompleteForgottenOrdersCommand` (1 h), `EraseOrderPersonalDataCommand` (diário), `RecalculateProviderStatsCommand` (diário: tempo de resposta e cancelamentos tardios), `DeleteExpiredIdempotencyKeysCommand` (diário) e o processamento de foto de pedido pela fila. Os comandos carregam em lotes, chamam o domínio e salvam pedido por pedido (sem `ExecuteUpdate`). |
| Auditoria | `IAuditLog` na transação do comando: liberação de endereço (aceite), cada leitura do contato pelo prestador, ajuste de horário, cancelamento do prestador com menos de 2 h e apagamento por retenção. Ações em `Orders/Common/OrderAuditActions`. |
| API | `OrdersController` (`api/v1/orders`, autenticado) e `ProviderOrdersController` (`api/v1/provider/orders` e `api/v1/provider/agenda`, política `Provider`), finos, só com `ISender`. Rate limit como na spec: `orders`, `quotes`, `uploads`, `search` e `profile`, e o limitador global `write` nas outras escritas. `Idempotency-Key` obrigatório em criar pedido e enviar orçamento. Respostas `no-store`. |
| Correção da página de compartilhar | `SharePage.Render` passa a substituir todos os marcadores numa passada só (expressão sobre `{{nome}}` com um dicionário), como o renderizador dos e-mails. |
| App: estrutura | `features/orders` (cliente: pedir, acompanhar, recusado, cancelar, lista e o card da Início), `features/provider_orders` (Pedidos, Responder, recusa, pedido agendado e ajuste) e `features/agenda`, cada uma com `data`, `domain` e `presentation`, controllers Riverpod gerados e textos no ARB. As rotas já existentes (Pedidos, Pedidos do prestador, Agenda) ganham as telas; as de tarefa entram como rotas tipadas novas. |
| App: design system | Widgets novos em `core/design_system/widgets`, um por arquivo: `VitrioSegmented`, `VitrioOrderTimeline`, `VitrioQuoteCard`, `VitrioCalendar` (e o tipo das marcas), `VitrioChoiceList` (motivos, escolha única no padrão de seleção suave) e o ícone `phone` em `VitrioIconName` e em `assets/icons`. Tudo com tokens; nada de cor, tamanho ou raio literal. |
| App: regras | O app não decide estado nem prazo: usa `allowedActions`, o prazo e a validade vindos da API. O aviso de horário sobreposto vem da agenda do dia pedida à API. Dinheiro só exibido e enviado como digitado. Ligar, WhatsApp e mapa abrem por `url_launcher` (`tel:`, `https://wa.me/`, link do Google Maps), só quando a API devolve os dados. |
| Testes | Domínio: cada transição e cada prazo, com relógio falso e fusos. Aplicação e integração: os critérios de aceite com nomes de comportamento, 401 sem token e 404 para pedido alheio em todas as rotas, idempotência, concorrência por `xmin`, auditoria e a outbox dos eventos. App: testes de widget das telas e dos widgets novos, e um fluxo de ponta a ponta no emulador (cliente pede, prestador responde, cliente aceita, prestador vê o endereço e a agenda). |

## Recorte 1: base técnica

**Situação:** pronto. O `IdempotencyBehavior` roda para todo comando e só age nos que implementam `IIdempotentCommand`; abre a transação por `IDatabaseTransactionFactory` (Application), guarda só o corpo da resposta de sucesso e, na corrida, devolve a resposta de quem gravou primeiro. A documentação citava um comportamento de transação que não existia; `04-padroes.md`, `03-arquitetura.md` e `09-cache.md` foram corrigidos. A limpeza das chaves vencidas entrou aqui (`MaintenanceJobs`, diário), e a checagem de falha dos jobs foi para `JobResults`. Fotos privadas: `IPrivateMediaStorage` com o bucket `vitrio-private`, leitura pela URL assinada do mesmo endereço das URLs de envio.

1. `IIdempotentCommand`, `IdempotencyBehavior` e a tabela `idempotency_keys`, com testes (mesma resposta, 422 com corpo diferente, corrida).
2. `IPrivateMediaStorage` e o bucket `vitrio-private` (criação no `StorageBucketsInitializer`, configuração e Testcontainers).
3. `ProviderStatsUpdated` no domínio e no `InvalidateProviderProfileCache`.

## Recorte 2: domínio do pedido

**Situação:** pronto. O pedido guarda o fuso da cidade (`OrderPlace.TimeZoneId`), e o horário do serviço é o value object `ServiceSchedule` (data, início e duração no fuso da cidade, mais início e fim em UTC), usado pelo orçamento e pelo agendamento; o ajuste troca só o agendamento. Bairro, cidade e o nome curto ficam fora dos dados apagados pela retenção. `CityTime` e `ShortPersonName` ficaram em `Domain/Common`. 43 testes de domínio e os contratos dos 11 eventos.

1. `Order`, `Quote`, `OrderPhoto`, enums e value objects (`PreferredPeriod`, motivos de recusa e de cancelamento, `OrderParty`), `OrderErrors` e os eventos do catálogo.
2. Regras de prazo, validade, janela do endereço, ajuste (até 2, início com 1 h), conclusão, retenção e `allowedActions`.
3. Testes de domínio e de contrato dos eventos.

## Recorte 3: persistência

**Situação:** pronto. Tabelas `orders` e `order_photos` na migração `AddOrders`. Lugar, endereço, contato, orçamento e agendamento são tipos próprios (`OwnsOne`) com colunas de prefixo (`quote_*`, `scheduled_*`); endereço e contato saem cifrados. Como o EF não deixa o mesmo objeto próprio em dois donos, o aceite copia o horário do orçamento para o agendamento (`ServiceSchedule.Copy`). Valor em `numeric(12,2)`, `xmin` para concorrência e índices por cliente, por prestador e status, por prazo, por validade e por início agendado. `IOrderWritableRepository` com o filtro de dono dentro da consulta; o `IOrderSearchableRepository` entra com as leituras dos recortes 4 e 5. Testes de integração de leitura de volta, cifra das colunas, 404 do pedido alheio e conflito de concorrência.

1. Configurações do EF (tipos complexos, colunas cifradas, `xmin`, índices) e repositories.
2. Migração `AddOrders` revisada.

## Recorte 4: API do cliente

**Situação:** pronto. `OrdersController` com criar (idempotente; a chave vem do cabeçalho e o hash é do corpo já lido), URL de envio e cópia de fotos, lista, detalhe, sugestões, aceitar, recusar o orçamento, cancelar e concluir. As ações permitidas saem de `OrderState` no domínio, para a consulta e o agregado usarem a mesma regra; os limites de pedidos abertos ficaram em `OrderLimits`. Vitrine em rascunho ou removida responde 404; pausada, suspensa ou com o dono inativo, 409 "não está recebendo pedidos". As sugestões usam a busca com o critério novo `ExcludedProviderId`. A spec foi corrigida: a categoria do pedido é sempre a principal do prestador, porque o serviço da vitrine não tem categoria. As fotos ficam em processamento até o job do recorte 6. 38 testes de integração (criação, idempotência, limites, 401 em todas as rotas, 404 do pedido alheio, aceite com versão, validade e auditoria, recusa, cancelamento, conclusão, sugestões e cópia de fotos).

1. URL de envio de foto, criar pedido (celular, limites, própria vitrine, vitrine que não recebe pedidos, endereço, fotos, prazo) e cópia de fotos.
2. Listar, detalhe com `allowedActions`, URLs das fotos e WhatsApp do prestador quando ativo.
3. Aceitar (versão, validade, liberação e auditoria), recusar o orçamento, cancelar, concluir e sugestões.
4. Testes de integração dos critérios, 401 e 404.

## Recorte 5: API do prestador

**Situação:** pronto. `ProviderOrdersController` com listas por aba, detalhe, orçamento (idempotente), recusa, ajuste de horário, cancelamento, conclusão e agenda; o Painel ganhou `newOrders`, `servicesThisMonth` (mês no fuso da cidade do prestador) e `latestNewOrders`. A regra da janela do contato virou `Order.IsContactVisible`, usada pelo agregado e pelas consultas (listas e agenda mostram o nome completo só dentro da janela). Toda resposta que traz o contato grava `order.contact_read`; por isso o detalhe do prestador é o comando `ViewProviderOrderCommand`, e as ações salvam de novo depois de montar a resposta. Ajuste grava `order.rescheduled` e cancelamento do prestador com menos de 2 h grava `order.late_cancellation` (`Order.WasCancelledLate`). As leituras do prestador ficaram em `ProviderOrderReader`, chamado pelo `IOrderSearchableRepository`. O item da agenda ganhou `quoteValidUntil` para o selo "Válido até" da prancheta (spec atualizada). 28 testes de integração (AC-1, AC-4, AC-5, AC-7, AC-12, AC-13, AC-17, AC-20, AC-24 a AC-29, AC-35, 401, 403 e 404 em todas as rotas).

1. Listas por aba e detalhe com a regra da janela e a auditoria de leitura.
2. Enviar e substituir orçamento (idempotente), recusar com motivo, ajustar horário, cancelar e concluir.
3. Agenda por intervalo e números do Painel.
4. Testes de integração dos critérios, 401 e 404.

## Recorte 6: jobs e handlers

**Situação:** pronto. Jobs `ExpireOrders` (5 min), `CompleteForgottenOrders` (1 h), `EraseOrderPersonalData` e `RecalculateProviderStats` (diários) em `OrderJobs`; cada comando esvazia os vencidos em lotes (`Batches.DrainAsync`) e salva pedido por pedido com `IUnitOfWork.TrySaveChangesAsync`, que descarta o pedido em conflito de `xmin` e segue com o resto (ele volta no próximo ciclo). A retenção apaga as fotos privadas antes de salvar e grava `order.personal_data_erased`. As fotos de pedido vão para a fila pelo handler `ProcessOrderPhotos` do `order.created` (depois da gravação, para o job encontrá-las; catálogo atualizado) e são processadas num WebP privado; a limpeza de uploads foi para `Common/CleanUpUploads` e passou a cuidar das fotos de pedido. Tempo de resposta: regra em `ProviderStats` (mediana dos últimos 20 em 90 dias, mínimo 5, faixas `15min`, `30min`, `1h`, `3h`, `1d`), com `Provider.UpdateResponseTime` recebendo a amostra; handlers `RecalculateProviderResponseTime`, `RecalculateCompletedOrders` e `RecordLateCancellation` sobre `IProviderStatsService`; o job diário recalcula quem teve pedido ou cancelamento tardio saindo da janela (com 2 dias de folga para um ciclo perdido) e também faz a janela dos cancelamentos tardios andar. Exclusão de conta: `ScheduledOrdersBlockDeletion` (409 `order.scheduled_blocks_deletion`) e `CancelOpenOrdersOfRemovedUser`. A limpeza das chaves de idempotência já tinha entrado no recorte 1. Os testes dos jobs rodam numa coleção sem paralelismo, porque os jobs mexem em todos os pedidos vencidos do banco. 15 testes de integração (AC-8, AC-16, AC-32, AC-33, AC-34, serviços feitos, cancelamento tardio, fotos) e testes de domínio das faixas.

1. Comandos e jobs de expiração, conclusão automática, retenção, chaves de idempotência e fotos de pedido.
2. Handlers dos números derivados e o recálculo diário do tempo de resposta.
3. Bloqueio da exclusão com pedido agendado e cancelamento dos pedidos abertos da conta excluída.

## Recorte 7: página de compartilhar

**Situação:** pronto. `SharePage.Render` preenche os marcadores numa passada só, com a mesma expressão do renderizador dos e-mails, e falha se o modelo pedir um marcador sem valor. Teste de integração com o título "Eletricista {{place}}" na página `/p/{id}`.

1. Substituição numa passada só e o teste do título com `{{place}}`.

## Recorte 8: widgets do design system

**Situação:** pronto. `VitrioSegmented<T>` e `VitrioChoiceList<T>` usam o mesmo `VitrioOption<T>`; `VitrioOrderTimeline` recebe `VitrioTimelineStep`s com `stateLabel` para o leitor de tela; `VitrioQuoteCard` usa `VitrioQuoteRow` e o tamanho novo de botão `decision` (52, raio `lg`), que o componente define e que serve também ao card de serviço agendado; `VitrioCalendar` recebe os textos por `VitrioCalendarLabels` (mês, dias da semana, setas, legenda e o anúncio de cada dia) e as marcas por dia (`VitrioCalendarMark`), com semana começando no domingo como a prancheta. Onde a prancheta usa uma cor fora dos tokens (borda da etapa pendente), vale o token do componente (`line-strong`). Ícone `phone` copiado do design system. 12 testes de widget e o guia Flutter atualizado.

1. `VitrioSegmented`, `VitrioOrderTimeline`, `VitrioQuoteCard`, `VitrioCalendar`, `VitrioChoiceList` e o ícone `phone`, com testes de widget e o guia Flutter atualizado.

## Recorte 9: app do cliente

**Situação:** pronto. Pedir orçamento (aberto pelo "Pedir orçamento" do perfil, ao lado do WhatsApp, e por "Pedir para este" com o pedido anterior e as cópias das fotos), Seu pedido (linha do tempo, card do orçamento com a versão no aceite, serviço agendado com "Cancelar" e "Serviço concluído" liberado a partir do início, horário ajustado, recusado ou sem resposta com até 3 sugestões e "Ver mais" na Busca filtrada pela categoria, e os encerramentos), a folha de cancelamento, Pedidos (Em andamento e Finalizados, com convite para entrar sem conta) e o card "Seu último pedido" da Início. O app não tinha endereços nem pedia celular: a folha de endereço (escolher um salvo ou cadastrar CEP, número, complemento e nome) e o campo de celular em Pedir orçamento, que salva na conta antes do envio, seguem os componentes do design system, porque esses estados não têm prancheta. Diferença de componente: a nota "Pedir orçamento é grátis e sem compromisso." fica acima do botão, como no `ActionBar` do design system, e não abaixo, como na prancheta. O `VitrioQuoteCard` passou a quebrar o selo de validade para a linha de baixo quando não cabe, e o `VitrioBadge` corta o texto com reticências em vez de estourar. 10 testes de widget das telas. No teste no emulador, os horários saíam no fuso do aparelho (o emulador está em UTC): a API passou a mandar os instantes dos pedidos com o deslocamento da cidade do pedido (`CityInstant`) e o app guarda a hora como veio (`parseCityTime`), comparando com o "agora" da cidade; também saíram dali o erro do celular que não sumia ao digitar, o texto repetido do card sem endereço e o "Cancelar pedido" duplicado junto do orçamento.

1. Pedir orçamento (com o título, o celular antes do primeiro pedido e o preenchimento por "Pedir para este").
2. Acompanhar pedido em todos os status, pedido recusado com sugestões e a folha de cancelamento.
3. Pedidos (Em andamento e Finalizados) e o card "Seu último pedido" da Início.

## Recorte 10: app do prestador

**Situação:** pronto. Módulo `provider_orders` no app: Painel com "pedidos novos", "serviços no mês" e os pedidos novos mais recentes (o primeiro em destaque, com a descrição e as fotos; "Ver todos" abre Pedidos), Pedidos com as abas Novos (contador), Em andamento e Finalizados, Responder pedido (tela de tarefa, sem a barra de navegação) com o aviso de horário calculado pela agenda do dia, a folha de recusa (aberta pelo card ou pela tela), o detalhe do pedido dentro da aba Pedidos (agendado com Ligar, WhatsApp e Abrir no mapa; com orçamento enviado e "Substituir orçamento"; encerrado com o motivo) e Ajustar horário. Dia, início e duração usam um campo de seleção montado com os tokens, porque o design system não tem um; as opções de início respeitam a antecedência de 1 h e a duração para antes da meia-noite. A folha de cancelamento recebe os motivos de cada lado. O `VitrioSectionHeader` ganhou o contador ("Novos pedidos 3") e o `VitrioSegmented` perdeu folga lateral para "Em andamento" caber nas três abas. Celular e CEP do cliente saem formatados. Onde a prancheta diz "a cliente" ou "ela", o app usa o primeiro nome do cliente, e o selo do orçamento na lista ficou "Aguardando · válido até qua 11/11, 13:00" para caber numa linha; as duas diferenças de texto foram aprovadas e estão na spec. 9 testes de widget das telas. No emulador, com a conta de teste e pedidos criados na API local: painel, resposta com o aviso de horário, recusa com "Outro", detalhe agendado, ajuste de horário e folha de cancelamento; dali saíram a cor dos ícones da barra de status em Pedidos, o rótulo cortado das abas, o selo cortado, o formato do dia ("Qua, 11/11") e o destaque do item escolhido nas listas.

1. Painel com os números e os novos pedidos.
2. Pedidos (Novos, Em andamento, Finalizados), Responder pedido com o aviso de horário e a folha de recusa.
3. Pedido agendado (Ligar, WhatsApp, Abrir no mapa) e Ajustar horário.

## Recorte 11: Agenda

**Situação:** pronto. A aba Agenda usa o `VitrioCalendar` com as marcas cheia (agendado), vazada (orçamento aguardando o cliente) e neutra (feito), lê a agenda do mês visível (`/provider/agenda` do primeiro ao último dia) e mostra abaixo os itens do dia escolhido em "Agendados", "Aguardando o cliente" e "Feitos"; cada item abre o pedido na aba Pedidos. Abre no dia de hoje do aparelho; no emulador, que fica em UTC, isso pode ser o dia seguinte ao da cidade à noite. As telas sem barra de topo (Pedidos e Agenda) acertam a cor dos ícones da barra de status, porque o Painel deixa os ícones claros. 4 testes de widget; no emulador, com os pedidos da conta de teste, as marcas, a lista do dia e a troca de mês; dali saiu a coluna da hora, que quebrava "até 15:00" em duas linhas.

1. Agenda em calendário com as marcas, a lista do dia e a navegação entre meses.

## Recorte 12: fechamento

1. Revisão de back: `/code-review`, `/security-scan` e `/arch-check`; cada endpoint testado na API local, com os casos de sucesso e de erro.
2. Revisão de front: cada tela no emulador, por captura, contra as pranchetas aprovadas e o design system; código do app contra os padrões.
3. Fluxo de ponta a ponta no emulador e os quatro fluxos que já existem.
4. Plano como concluído e PR com o resultado das revisões e dos testes.

## Riscos

| Risco | Como tratar |
| --- | --- |
| Fusos e horário de verão na conta de `RespondBy`, validade e agenda | Toda conversão no agregado com `TimeZoneInfo` da cidade; testes com fusos diferentes de São Paulo e com virada de dia |
| Liberar endereço ou celular fora da janela | Regra única no agregado, usada pela consulta do prestador, com testes de 7 e 8 dias e de pedido cancelado |
| Jobs disputando o mesmo pedido com uma ação do usuário | `xmin` em todo salvamento; o job ignora quem mudou de estado e tenta de novo no próximo ciclo |
| Escopo grande no app | Widgets do design system primeiro, depois cliente, prestador e agenda, cada recorte com seus testes |

## Decisões tomadas no plano

- Cancelamentos tardios (2026-11-06): nesta spec, `RecordLateCancellation` recalcula e guarda `LateCancellations90d`; a fila de moderação da spec 008 abre o item a partir de 3 na janela. A spec 005 e o catálogo de eventos foram ajustados.
