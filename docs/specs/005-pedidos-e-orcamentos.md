# Spec 005 — Pedidos e orçamentos

- **Status:** Approved (2026-11-06)
- **Data:** 2026-11-06
- **Responsável:** Wellington
- **Correção (2026-11-06):** na implementação, a categoria do pedido "do serviço escolhido" não existia, porque o serviço da vitrine não tem categoria (spec 003). Por decisão do usuário, a categoria do pedido é sempre a principal do prestador; o 02-regras foi corrigido junto.

## Problem

Contratar pelo Vitrio precisa ser simples para o cliente e justo para o prestador: o cliente descreve o que precisa, o prestador responde com valor e horário, o cliente decide. O endereço do cliente só pode ir para o prestador depois do aceite, e os prazos evitam pedidos esquecidos. O prestador atende vários clientes, às vezes mais de um no mesmo dia, e precisa ver a semana e o mês de trabalho num calendário, com o que já está agendado e o que ainda depende do cliente.

## Scope

### In

**Pedido do cliente**

- Cliente cria pedido para um prestador: serviço (da lista do prestador ou "Outro"), título ("Resuma o que precisa", 5 a 80 caracteres), descrição (20 a 500), até 5 fotos, dia (Hoje, Amanhã ou "Escolher data" até 30 dias à frente) e período (manhã 7h–12h, tarde 12h–18h, noite 18h–22h), endereço salvo. Um período de hoje que já acabou não pode ser escolhido.
- Prazo para o prestador responder (`RespondBy`): o fim do período pedido, no fuso da cidade do pedido, limitado a 48 h depois do envio.
- Cliente acompanha o pedido (Acompanhar pedido): linha do tempo (Pedido enviado, Orçamento recebido, Serviço agendado, Serviço concluído), card do orçamento com "Aceitar orçamento" e "Recusar", card "Serviço agendado" com "Cancelar" e "Serviço concluído", e o aviso de que o pagamento é combinado direto com o profissional.
- O botão de conversa ao lado do prestador abre o WhatsApp dele só quando o botão de WhatsApp está ligado na vitrine (mesma regra do perfil público); sem isso, não aparece.
- Pedido recusado ou vencido sem resposta mostra o motivo (na recusa) e até 3 prestadores da mesma categoria perto do endereço do pedido, sem o que recusou ou deixou vencer e sem a vitrine do próprio cliente. "Ver mais" abre a Busca filtrada pela categoria; "Pedir para este" abre Pedir orçamento já preenchido com o mesmo pedido, inclusive as fotos (copiadas para o pedido novo; o cliente pode tirar ou adicionar antes de enviar).
- Lista Pedidos do cliente, com as abas Em andamento (`Sent`, `Quoted`, `Scheduled`) e Finalizados (os demais).
- Início do cliente logado: o card "Seu último pedido" mostra o pedido em aberto mais recente com o status; sem pedido em aberto, some.
- Até a spec 007, "Serviço concluído" não leva à avaliação: o pedido mostra "Serviço concluído" na linha do tempo e nada mais.

**Orçamento e resposta do prestador**

- Prestador responde (Responder pedido) com orçamento — tipo (Valor fechado ou Visita técnica), valor, dia, início (de 30 em 30 min), duração prevista (30 min a 8 h, de 30 em 30 min), "Material incluso" com o campo opcional "Qual material" (até 80 caracteres) e mensagem (até 500) — ou recusa.
- Ao escolher dia e início que cruzam um serviço agendado, a tela avisa "Você já tem um serviço das 14:00 às 15:00 nesse dia". O servidor não bloqueia: o prestador pode ter vários serviços no mesmo dia, inclusive sobrepostos.
- O prestador pode substituir o orçamento enquanto o pedido estiver `Quoted` (a versão aumenta e a validade recomeça).
- Recusa: a folha "Recusar o pedido?" exige um motivo — Sem horário disponível, Fora da minha área, Não faço esse serviço ou Outro. "Outro" exige texto de até 200 caracteres, que passa pelo mesmo filtro de telefone, e-mail e link da vitrine. O cliente vê o motivo (e o texto, em "Outro") junto de "Carlos não pode atender agora".
- Painel do prestador: "pedidos novos" conta os `Sent` aguardando resposta; "serviços no mês" conta os pedidos concluídos (`Completed`, `PaymentConfirmed` e `PaymentDisputed`, como os "serviços feitos" do perfil; pagamento anulado pelo suporte não conta) com `CompletedAt` no mês corrente, no fuso da cidade do prestador; a lista "Novos pedidos" mostra os mais recentes com "Recusar" e "Enviar orçamento".
- Lista Pedidos do prestador, com as abas Novos (`Sent`), Em andamento (`Quoted`, `Scheduled`) e Finalizados (os demais).
- Detalhe do pedido do prestador: antes do aceite, o que está em Responder pedido; depois do aceite, endereço completo, nome completo e celular do cliente, com os atalhos "Ligar", "WhatsApp" (para o celular do cliente) e "Abrir no mapa" (app de mapas do celular), além de "Ajustar horário", "Cancelar" e "Serviço concluído". Os dados e os atalhos somem quando o acesso ao endereço expira ou o pedido é cancelado.

**Agenda do prestador em calendário**

- Grade do mês, com setas para trocar de mês, aberta no dia de hoje.
- Marcas por dia: serviço agendado (`Scheduled`) com marca cheia; orçamento enviado que o cliente ainda não decidiu (`Quoted` dentro da validade) com marca vazada; nos dias passados, serviços concluídos (`Completed`, `PaymentConfirmed` e `PaymentDisputed`) em tom neutro.
- Tocar num dia seleciona o dia e mostra logo abaixo os horários dele, com "Agendados" e "Aguardando o cliente" separados; cada item mostra horário, título, cliente e bairro e abre o pedido.

**Ciclo do pedido**

- Cliente aceita ou recusa o orçamento; ao aceitar, o pedido fica `Scheduled` e o endereço é liberado ao prestador.
- **Ajuste de horário pelo prestador:** num pedido `Scheduled`, antes do início, o prestador muda dia, início e duração (início no mínimo 1 h à frente), no máximo 2 vezes por pedido. O cliente é avisado ("Carlos mudou o horário para sex 30/10, 09:00") e, se não concordar, cancela com o motivo "O novo horário não serve". A Agenda, o acesso ao endereço e a conclusão automática passam a seguir o horário novo.
- Cancelar (qualquer lado, com motivo, regras por estado). Motivos do cliente: Resolvi de outro jeito, O horário não serve mais, O novo horário não serve, Achei outro profissional, Outro. Motivos do prestador: Imprevisto, Não consigo chegar no horário, O cliente pediu para cancelar, Outro. Texto de até 200 caracteres, opcional em todos e obrigatório em "Outro"; o outro lado vê o motivo escolhido, nunca o texto.
- Concluir serviço (qualquer lado, a partir do início do horário agendado).
- Expirações automáticas (sem resposta até `RespondBy`; orçamento sem decisão até a validade) e conclusão automática do pedido agendado 7 dias após o fim do horário.
- Ações permitidas calculadas pela API (`allowedActions`) para o app não duplicar regras.
- **Dados derivados do prestador:** tempo de resposta (`ResponseTimeBucket`, o "~15 min" de Pedir orçamento e do perfil), serviços feitos (`CompletedOrders`) e cancelamentos tardios (`LateCancellations90d`), recalculados pelos eventos de pedido, como no catálogo de eventos.
- **Exclusão de conta:** passa a ser bloqueada (409) com pedido `Scheduled`, como cliente ou prestador; na exclusão, os pedidos `Sent` e `Quoted` da pessoa são cancelados com motivo "Conta excluída".
- **Retenção:** 90 dias depois do fim do serviço (ou do encerramento do pedido sem serviço), um job apaga do pedido o endereço completo, o nome completo, o celular e as fotos; ficam bairro, cidade e primeiro nome com a inicial, e o pedido mostra "Fotos removidas".

**Correção herdada**

- Página de compartilhar (`/p/{id}`): hoje os campos são preenchidos um depois do outro, então um texto do prestador que contenha o marcador de outro campo (ex.: `{{place}}` no título) é trocado por esse campo. Cada marcador passa a ser preenchido numa passada só, como nos e-mails de conta, e o texto aparece como foi digitado.

### Out

- Pagamento (spec 006), avaliações (spec 007), notificações push e central de notificações (spec 009 — esta spec só publica eventos), suspensão de conta (spec 008 — o cancelamento dos pedidos abertos na suspensão entra lá).
- Chat, remarcação com proposta e aceite do outro lado, pedido para vários prestadores de uma vez.

### Diferenças aprovadas do protótipo do app

Aprovadas pelo usuário em 2026-11-06. As pranchetas são atualizadas (ou criadas) antes do código, e as telas ficam iguais a elas:

- Pedir orçamento: campo "Resuma o que precisa" (título) entre o serviço e a descrição; o contador da descrição continua 0/500.
- Acompanhar pedido: no card "Serviço agendado", "Remarcar" vira "Cancelar"; o selo do orçamento mostra a data e a hora exatas ("Válido até qui 29/10, 13:00") em vez de "Válido por 48 h".
- Responder pedido: "Horário" vira "Início" e "Duração"; com "Material incluso" ligado, aparece o campo opcional "Qual material".
- Pranchetas novas, feitas com os componentes do design system: Pedidos do cliente, Pedidos do prestador, detalhe do pedido do prestador depois do aceite, folha de recusa, folha de cancelamento e Agenda. O calendário entra no design system como componente novo (`Calendar`), com a marca cheia, a vazada e a neutra.

Aprovadas pelo usuário em 2026-11-09, depois do teste no aparelho:

- Telas do prestador (Responder pedido, Ajustar horário, detalhe do pedido e folha de recusa): onde a prancheta diz "a cliente" ou "ela", o app usa o primeiro nome do cliente ("Bruno recebe uma notificação e aceita pelo app.", "Endereço completo liberado quando Bruno aceitar."), porque o app não sabe o gênero de quem pediu.
- Pedidos do prestador, aba Em andamento: o selo do orçamento é "Aguardando · válido até qua 11/11, 13:00", em vez de "Aguardando a cliente · válido até ...", para caber numa linha.

Pedidos do usuário em 2026-11-09, depois de usar o app no aparelho:

- Novo endereço (folha de Pedir orçamento): o CEP tem máscara e, com os 8 números, o app busca o endereço e preenche rua, bairro e cidade; rua e bairro só ficam editáveis quando o CEP não os traz (CEP geral da cidade), e aí são obrigatórios. Número e complemento ficam lado a lado, como no endereço base da vitrine.
- Celular: os campos de celular (Pedir orçamento, Editar dados e o contato da vitrine) formatam enquanto a pessoa digita, como "(16) 99814-7143", e a conta e o detalhe do pedido mostram o celular nesse formato.
- Própria vitrine no modo cliente: a busca, os favoritos e o perfil público marcam a vitrine da própria conta com o selo "Você"; o perfil não oferece "Pedir orçamento" nem WhatsApp e mostra "Esta é a sua vitrine. Você não pode pedir orçamento para você mesmo." com "Editar minha vitrine"; abrir Pedir orçamento da própria vitrine mostra o mesmo aviso. A prévia "Ver como os clientes veem", em Meu perfil, continua mostrando o perfil exatamente como o cliente vê.

Aprovadas pelo usuário em 2026-11-13, na revisão de front:

- Pedir orçamento: a nota "Pedir orçamento é grátis e sem compromisso." fica acima do botão "Enviar pedido", como no `ActionBar` do design system e nas pranchetas de Responder pedido e Ajustar horário (a prancheta de Pedir orçamento a põe abaixo).
- Acompanhar pedido, card "Serviço agendado" do cliente: abaixo dos botões, enquanto "Serviço concluído" está desabilitado, aparece "Você poderá marcar como concluído a partir das 14:00.", como no card do prestador.

## Domain Model

- `Order` (agregado): `Id`, `CustomerId`, `ProviderId`, `ServiceId?`, `ServiceTitle` (nome do serviço escolhido, ou "Outro"), `Title` (5–80), `Description` (20–500), `PreferredDay` (`Date`), `PreferredPeriod` (`Morning`, `Afternoon`, `Evening`), `AddressSnapshot` (cifrado: logradouro, número, complemento, bairro, cidade, UF, CEP), `CustomerContactSnapshot` (cifrado: nome completo e celular no momento do pedido), `CustomerDisplayName` ("Mariana F."), `Neighborhood`, `CityIbgeCode`, `CustomerPoint` (privado; centroide da cidade quando o CEP não tem coordenadas, com `CustomerPointIsApproximate`), `CategoryId` (a categoria principal do prestador no momento do pedido, com ou sem serviço escolhido), `Status`, `CreatedAt`, `RespondBy`, `FirstResponseAt?` (primeiro orçamento ou recusa), `DeclineReason?` (`NoAvailability`, `OutsideArea`, `NotMyService`, `Other`), `DeclineDetails?` (até 200, só em `Other`), `QuoteValidUntil?` (o que vier primeiro entre envio + 48 h e início − 1 h), `ScheduledStart?`, `ScheduledEnd?`, `RescheduleCount` (0–2), `CompletedAt?`, `CompletedBy?` (`Customer`, `Provider`, `System`), `CancelledAt?`, `CancelledBy?` (`Customer`, `Provider`, `System`), `CancellationReason?` (código), `CancellationDetails?`, `AddressReleasedAt?`, `PersonalDataErasedAt?`.
- `RespondBy` = o menor entre `CreatedAt` + 48 h e o fim do período pedido no dia pedido (12h, 18h ou 22h), no fuso da cidade do pedido.
- Endereço, nome completo e celular visíveis ao prestador de `AddressReleasedAt` até `ScheduledEnd` + 7 dias, se o pedido não foi cancelado, em qualquer status (inclusive os de pagamento).
- `Quote` (entidade do pedido): `Type` (`FixedPrice`, `TechnicalVisit`), `Amount` (`Money`, R$ 1,00 a R$ 100.000,00), `Date`, `Start` (de 30 em 30 min), `Duration` (30 min a 8 h, de 30 em 30 min), `End` (= `Start` + `Duration`), `MaterialIncluded`, `MaterialDetails?` (até 80, só com material incluso), `Message?` (até 500), `SentAt`, `Version`.
- `OrderPhoto`: `Id`, `OrderId`, `ObjectKey` (privado), `Status`.
- Fim do serviço para a retenção: `ScheduledEnd` quando houve agendamento; senão, o momento em que o pedido foi encerrado (recusa, expiração, cancelamento ou recusa do orçamento).
- Estados e transições: [02-regras-de-negocio.md §4](../02-regras-de-negocio.md). O ajuste de horário não muda o estado: o pedido continua `Scheduled`.
- Eventos (`order.*` e `quote.*`), conteúdo e consumidores: catálogo de [10-eventos.md](../10-eventos.md) §9. Esta spec acrescenta `order.rescheduled` (`OrderId`, `CustomerId`, `ProviderId`, `RescheduleCount`) e o `ReasonCode` em `order.declined`; os dois entram primeiro no catálogo.

## API Contract

| Verbo | Rota | Request | Response | Erros |
| --- | --- | --- | --- | --- |
| POST | `/api/v1/orders` | header `Idempotency-Key`; `{ providerId, serviceId?, title, description, preferredDay, preferredPeriod, addressId, photoKeys: [..5] }` | 201 `orderCustomerView` | 400, 401, 404 prestador, 409 limite de pedidos abertos, 409 pedir para si mesmo, 409 vitrine não recebe pedidos, 422 `order.phone_required`, 422 chave reutilizada, 429 |
| POST | `/api/v1/orders/photos/upload-url` | `{ contentType, contentLength }` | `{ uploadUrl, objectKey, expiresAt }` | 400, 401, 429 |
| POST | `/api/v1/orders/{id}/photos/copies` | — | `[{ objectKey, previewUrl }]` (cópias pendentes, que entram em `photoKeys` do pedido novo e somem em 24 h se não usadas) | 401, 404, 409 fotos já removidas, 429 |
| GET | `/api/v1/orders?status=open\|closed&page=` | — | `{ items: [orderCustomerSummary], hasMore }` (mais recentes primeiro) | 401 |
| GET | `/api/v1/orders/{id}` | — | `orderCustomerView` com `allowedActions`, motivo da recusa e `providerWhatsApp?` (só com o botão ligado na vitrine) | 401, 404 |
| GET | `/api/v1/orders/{id}/suggestions` | — | até 3 `providerSearchItem` (só em `Declined` e `Expired`) | 401, 404, 409 estado |
| POST | `/api/v1/orders/{id}/quote/accept` | `{ quoteVersion }` | `orderCustomerView` | 401, 404, 409 estado/versão/validade |
| POST | `/api/v1/orders/{id}/quote/decline` | — | `orderCustomerView` | 401, 404, 409 |
| POST | `/api/v1/orders/{id}/cancel` | `{ reason, details? }` | `orderCustomerView` | 400, 401, 404, 409 |
| POST | `/api/v1/orders/{id}/complete` | — | `orderCustomerView` | 401, 404, 409 antes do horário |
| GET | `/api/v1/provider/orders?tab=new\|ongoing\|closed&page=` | — | `{ items: [orderProviderSummary], hasMore }` | 401, 403 |
| GET | `/api/v1/provider/orders/{id}` | — | `orderProviderView` (endereço, nome completo e celular só quando liberados) com `allowedActions` | 401, 403, 404 |
| POST | `/api/v1/provider/orders/{id}/quote` | header `Idempotency-Key`; `{ type, amount, date, start, durationMinutes, materialIncluded, materialDetails?, message? }` | `orderProviderView` | 400, 401, 403, 404, 409, 422, 429 |
| POST | `/api/v1/provider/orders/{id}/decline` | `{ reason, details? }` | `orderProviderView` | 400, 401, 403, 404, 409 |
| POST | `/api/v1/provider/orders/{id}/reschedule` | `{ date, start, durationMinutes }` | `orderProviderView` | 400, 401, 403, 404, 409 estado/limite/início já passou |
| POST | `/api/v1/provider/orders/{id}/cancel` | `{ reason, details? }` | `orderProviderView` | 400, 401, 403, 404, 409 |
| POST | `/api/v1/provider/orders/{id}/complete` | — | `orderProviderView` | 401, 403, 404, 409 |
| GET | `/api/v1/provider/agenda?from=&to=` | intervalo ≤ 42 dias (as 6 semanas da grade) | `[{ orderId, kind: scheduled\|quoted\|done, date, start, end, title, customerName, neighborhood, quoteValidUntil? }]` (`quoteValidUntil` só em `quoted`, para o selo "Válido até") | 400, 401, 403 |
| GET | `/api/v1/provider/dashboard` | — | o atual, com `newOrders`, `servicesThisMonth` e `latestNewOrders` preenchidos | 401, 403 |

- `customerName` na agenda e nas listas do prestador: "Mariana F." antes do aceite; nome completo enquanto o acesso estiver liberado.
- A tela Responder pedido consulta a agenda do dia escolhido (`from = to = date`) para o aviso de horário sobreposto.
- Fotos do pedido são lidas por URL assinada de 10 min presente no `orderView`, só para cliente e prestador do pedido.
- Horários vão como `date` (`AAAA-MM-DD`) e `start`/`end` (`HH:mm`) no fuso da cidade do pedido; os instantes (`createdAt`, `respondBy`, `validUntil`, `completedAt`...) vão com o deslocamento da cidade do pedido (`2026-11-10T13:00:00-03:00`), e o app mostra a hora como veio, sem converter para o fuso do aparelho.
- `responseTime` no perfil e na busca: `15min`, `30min`, `1h`, `3h` ou `1d` (a faixa da mediana); nulo com menos de 5 pedidos na amostra ou mediana acima de 1 dia. O app mostra "~15 min", "~30 min", "~1 h", "~3 h" e "~1 dia".
- `DELETE /me` com pedido `Scheduled` (como cliente ou prestador): 409 `order.scheduled_blocks_deletion`.

## Authorization

- Rotas `/orders/*`: autenticado; consulta filtrada por `CustomerId = usuário`.
- Rotas `/provider/orders/*`, `/provider/agenda` e `/provider/dashboard`: política `Provider`; consulta filtrada por `ProviderId` da vitrine do usuário.
- Pedido de outra pessoa responde 404 em qualquer rota, inclusive nas cópias de fotos e nas sugestões.
- Endereço completo, nome completo e celular do cliente no `orderProviderView` só quando `AddressReleasedAt` existe, o pedido não foi cancelado, ainda não passaram 7 dias de `ScheduledEnd` e os dados não foram apagados pela retenção; cada leitura grava auditoria.
- Antes do aceite, o prestador vê primeiro nome + inicial do cliente ("Mariana F."), bairro, cidade e distância arredondada.
- O telefone do prestador só vai para o cliente (`providerWhatsApp`) quando o botão de WhatsApp está ligado na vitrine.
- O app do cliente pede o celular antes de enviar o primeiro pedido, se a conta ainda não tiver um.
- Rate limit em todos os endpoints, como nos módulos existentes: `orders` em `POST /orders`; `quotes` em `POST /provider/orders/{id}/quote`; `uploads` na URL de envio e na cópia de fotos; `search` nas sugestões; `profile` nas leituras privadas (listas, detalhe, agenda e Painel); as demais escritas (aceitar, recusar, cancelar, concluir e ajustar horário) ficam no limite global de escrita (`write`, 60/min).

## Edge Cases & Failure Modes

- Cliente com 10 pedidos abertos (`Sent`, `Quoted`, `Scheduled`) ou 3 `Sent` na mesma categoria: 409 com mensagem clara.
- Período de hoje que já acabou, ou data além de 30 dias: 400.
- `RespondBy`, sempre o menor entre criação + 48 h e o fim do período pedido: pedido para hoje à tarde feito às 16h vence hoje às 18h; para amanhã de manhã feito hoje às 10h vence amanhã às 12h; para daqui a 5 dias vence em 48 h.
- Pedido `Scheduled` que ninguém marca como concluído: 7 dias depois do fim do horário, o job conclui com `CompletedBy = System`, avisa os dois e o endereço deixa de aparecer.
- Orçamento proposto para começar em menos de 1 h: 400 (não daria tempo de o cliente aceitar).
- Orçamento com data no passado, início fora dos passos de 30 min, duração fora de 30 min a 8 h ou horário que passa da meia-noite: 400.
- "Qual material" com material desligado: 400.
- Endereço sem coordenadas: distância aproximada a partir do centroide da cidade, sem aviso de "fora da área".
- Vitrine pausada ou suspensa entre abrir o perfil e enviar: 409 "Este profissional não está recebendo pedidos agora". Pedidos já existentes continuam: o prestador pausado ainda responde, conclui e cancela.
- Endereço fora da área do prestador: permitido, com aviso ao cliente ("fora da área habitual") e ao prestador (distância).
- Prestador envia orçamento depois de o cliente cancelar: 409 por estado (concorrência com `xmin`).
- Cliente aceita um orçamento que acabou de ser substituído: 409 por `quoteVersion`; o app recarrega e mostra o novo valor.
- Aceitar depois de `QuoteValidUntil`: 409 `quote.expired`.
- Orçamento que cruza outro serviço agendado: aceito pelo servidor; só o app avisa.
- Ajuste de horário depois do início, num pedido que não está `Scheduled` ou pela terceira vez: 409. Ajuste e cancelamento do cliente ao mesmo tempo: o segundo recebe 409 por concorrência.
- Ajuste de horário recalcula a janela do endereço (novo `ScheduledEnd` + 7 dias) e a conclusão automática.
- Texto da recusa em "Outro" com telefone, e-mail ou link: 400 com a mesma mensagem da vitrine.
- Job de expiração, de conclusão ou de retenção rodando duas vezes: idempotente (só muda quem ainda está no estado e no prazo).
- Concluir antes do início do horário agendado: 409.
- Cancelamento pelo prestador com menos de 2 h: permitido; o handler de `OrderCancelled` recalcula e guarda `LateCancellations90d`; a fila de moderação da spec 008 abre o item a partir de 3 na janela (decisão de 2026-11-06, porque a fila nasce lá). Nada aparece no perfil público.
- Repetição do POST com a mesma `Idempotency-Key`: mesma resposta, sem pedido ou orçamento duplicado.
- Fotos ainda processando ao criar o pedido: o pedido é criado; as fotos aparecem quando prontas.
- "Pedir para este" depois de as fotos do pedido antigo terem sido apagadas pela retenção: 409 na cópia; o app abre o pedido sem fotos.
- Sugestões sem nenhum prestador parecido perto: lista vazia; o app mostra só "Buscar outros eletricistas".
- Tempo de resposta com o prazo novo: pedido que venceu sem resposta conta como o prazo inteiro que tinha (`RespondBy` − criação), não mais como 48 h fixas.
- Exclusão de conta com pedido `Scheduled` (como cliente ou prestador): 409 explicando que é preciso concluir ou cancelar antes.

## Non-Functional Requirements

- Criar pedido p95 ≤ 300 ms (sem contar upload de fotos); agenda de 42 dias p95 ≤ 200 ms.
- Expirações verificadas a cada 5 min (atraso máximo de 10 min); conclusão automática a cada 1 h; retenção uma vez por dia.
- Fotos de pedido privadas; URL assinada de 10 min.
- Auditoria: liberação e leituras de endereço, cancelamentos com menos de 2 h, ajustes de horário e apagamento por retenção.
- Todos os horários exibidos e calculados no fuso da cidade do pedido.

## Integrations

- Storage S3 compatível (fotos privadas, cópia entre pedidos e apagamento por retenção) e job de processamento.
- Publicação de eventos na outbox (consumidos pela spec 009 para avisos; nesta spec, pelos recálculos do prestador e pelo cancelamento na exclusão de conta).
- App de mapas, telefone e WhatsApp do celular, abertos por link (`geo:`/`https://maps.google.com`, `tel:`, `https://wa.me/`).

## Acceptance Criteria

- **AC-1** Dado um cliente autenticado com endereço salvo e um prestador publicado, quando cria um pedido válido, então o pedido nasce `Sent`, com `RespondBy` pela regra do período, e o prestador o vê na aba Novos com título, bairro e distância, sem endereço completo.
- **AC-2** Dado o mesmo `Idempotency-Key` reenviado com o mesmo corpo, quando cria o pedido, então recebe a mesma resposta e só existe um pedido.
- **AC-3** Dado um cliente que é dono da vitrine, quando pede orçamento para ela, então recebe 409.
- **AC-4** Dado um pedido `Sent`, quando o prestador envia orçamento válido para daqui a 3 dias, às 14:00 com duração de 2 h, então o pedido vira `Quoted`, `QuoteValidUntil` = envio + 48 h, `FirstResponseAt` é preenchido e o cliente vê valor, data, 14:00–16:00 e o material.
- **AC-5** Dado um pedido `Quoted`, quando o prestador substitui o orçamento, então `quoteVersion` aumenta, a validade é recalculada pela mesma regra e `FirstResponseAt` não muda.
- **AC-6** Dado um pedido `Quoted` com versão 2, quando o cliente aceita informando a versão 1, então recebe 409.
- **AC-7** Dado um pedido `Quoted` dentro da validade, quando o cliente aceita, então o pedido vira `Scheduled`, o endereço completo aparece na visão do prestador e uma auditoria de liberação é gravada.
- **AC-8** Dado um pedido `Sent` sem resposta até `RespondBy`, quando o job roda, então o pedido vira `Expired` e o cliente vê até 3 sugestões de prestadores da mesma categoria, sem o que deixou vencer.
- **AC-9** Dado um pedido `Scheduled`, quando o cliente cancela sem motivo, então recebe 400; com motivo, o pedido vira `Cancelled` e o prestador vê o motivo, sem o texto.
- **AC-10** Dado um pedido `Scheduled` cujo horário ainda não começou, quando alguém marca como concluído, então recebe 409; depois do início, o pedido vira `Completed` com `CompletedBy`.
- **AC-11** Dado o id de um pedido de outro cliente, quando consulta `GET /orders/{id}`, `GET /orders/{id}/suggestions` ou `POST /orders/{id}/photos/copies`, então recebe 404.
- **AC-12** Dado um pedido `Sent`, quando o prestador consulta o detalhe, então a resposta não contém logradouro, número, complemento, CEP, celular nem sobrenome do cliente (só "Mariana F.").
- **AC-13** Dado um pedido cujo horário agendado terminou há 8 dias, em qualquer status, quando o prestador consulta o detalhe, então o endereço completo não aparece mais.
- **AC-14** Dado qualquer pedido, quando o app consulta o detalhe, então `allowedActions` lista só as ações válidas para quem pergunta no estado atual.
- **AC-15** Dado um orçamento enviado às 15h para amanhã às 8h, quando é criado, então `QuoteValidUntil` é amanhã às 7h; às 7h01 o aceite recebe 409 `quote.expired`.
- **AC-16** Dado um pedido `Scheduled` cujo horário terminou há 7 dias sem conclusão, quando o job roda, então o pedido vira `Completed` com `CompletedBy = System`.
- **AC-17** Dado um pedido `PaymentConfirmed` com horário terminado há 3 dias, quando o prestador consulta o detalhe, então o endereço completo aparece.
- **AC-18** Dado um cliente com 3 pedidos `Sent` em Elétrica, quando cria o quarto em Elétrica (com serviço "Outro" ou não), então recebe 409; em Hidráulica, o pedido é criado.
- **AC-19** Dado um cliente sem celular na conta, quando tenta criar um pedido, então recebe 422 `order.phone_required` e nenhum pedido é criado.
- **AC-20** Dado um pedido aceito, quando o prestador consulta o detalhe dentro da janela, então vê nome completo e celular do cliente, e uma auditoria de leitura é gravada.
- **AC-21** Dado um prestador publicado com o título "Eletricista {{place}}", quando alguém abre `/p/{id}`, então a página mostra "Eletricista {{place}}" como foi digitado, sem trocar o marcador pela cidade.
- **AC-22** Dado um pedido para hoje à tarde criado às 16h, quando é criado, então `RespondBy` é hoje às 18h; dado um pedido para hoje à manhã criado às 13h, então recebe 400.
- **AC-23** Dado um pedido para daqui a 31 dias, quando é criado, então recebe 400.
- **AC-24** Dado um pedido `Sent`, quando o prestador recusa sem motivo, então recebe 400; com "Outro" e texto com telefone, 400; com "Fora da minha área", o pedido vira `Declined` e o cliente vê o motivo.
- **AC-25** Dado um orçamento com "Qual material" preenchido e material desligado, quando é enviado, então recebe 400.
- **AC-26** Dado um prestador com um pedido `Scheduled` amanhã das 14:00 às 15:00, quando consulta a agenda do mês, então o dia de amanhã tem o item `scheduled`; dado também um orçamento `Quoted` para depois de amanhã, esse dia tem o item `quoted`; um orçamento vencido não aparece.
- **AC-27** Dado um prestador com um serviço concluído ontem, quando consulta a agenda do mês, então ontem tem o item `done`; pedidos cancelados não aparecem.
- **AC-28** Dado um pedido `Scheduled` para amanhã, quando o prestador ajusta o horário para depois de amanhã às 09:00 com 1 h, então o pedido continua `Scheduled` com o novo horário, `RescheduleCount` = 1, o evento `order.rescheduled` é publicado e a janela do endereço passa a contar do novo fim.
- **AC-29** Dado um pedido já ajustado 2 vezes, quando o prestador ajusta de novo, então recebe 409; dado um pedido cujo horário já começou, o ajuste também recebe 409.
- **AC-30** Dado um pedido `Declined` com 2 fotos, quando o cliente pede as cópias, então recebe 2 chaves novas; ao criar o pedido novo com elas, o pedido novo tem 2 fotos e o antigo continua com as suas.
- **AC-31** Dado um pedido `Declined` em Elétrica e dois outros eletricistas publicados perto, quando o cliente consulta as sugestões, então recebe os dois, sem o que recusou; num pedido `Quoted`, recebe 409.
- **AC-32** Dado um pedido encerrado há 91 dias com endereço e fotos, quando o job de retenção roda, então endereço, nome completo, celular e fotos são apagados, `PersonalDataErasedAt` é preenchido e o pedido continua com bairro, cidade e "Mariana F.".
- **AC-33** Dado um usuário com pedido `Scheduled`, quando tenta excluir a conta, então recebe 409; sem agendados, a exclusão cancela os pedidos `Sent` e `Quoted` dele com motivo "Conta excluída".
- **AC-34** Dado um prestador com pedidos respondidos, quando um orçamento é enviado, então o tempo de resposta dele é recalculado pela mediana da amostra e aparece no perfil e em Pedir orçamento.
- **AC-35** Dado um prestador com 2 pedidos concluídos neste mês e 1 no mês passado, quando consulta o Painel, então "serviços no mês" mostra 2 e "pedidos novos" mostra a quantidade de `Sent` aguardando resposta.
- **AC-36** Dado um cliente com um pedido `Scheduled` e outro `Sent` mais novo, quando abre a Início, então o card "Seu último pedido" mostra o `Sent`; sem pedido em aberto, o card não aparece.
- **AC-37** Dado um prestador com o botão de WhatsApp desligado, quando o cliente consulta o pedido, então a resposta não contém o telefone do prestador.

## Deferred Decisions

| Decisão | Fallback escolhido agora | Revisitar quando |
| --- | --- | --- |
| Remarcar com proposta e aceite do outro lado | O prestador ajusta o horário (até 2 vezes) com aviso ao cliente, que pode cancelar; ou cancelar e pedir de novo | Ajustes passarem de 10% dos agendamentos |
| Pedido para vários prestadores de uma vez | Um pedido por prestador; "Pedir para este" nas sugestões reaproveita o pedido | Clientes pedirem para comparar orçamentos lado a lado |
| Discordância de "Concluído" | Suporte por e-mail em 48 h | Volume justificar fluxo no app |
| Avaliar a partir do pedido concluído | Sem botão até a spec 007 | Spec 007 |

## Open Questions
