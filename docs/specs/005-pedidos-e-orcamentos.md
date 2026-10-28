# Spec 005 — Pedidos e orçamentos

- **Status:** Draft
- **Data:** 2026-10-28
- **Responsável:** Wellington

## Problem

Contratar pelo Vitrio precisa ser simples para o cliente e justo para o prestador: o cliente descreve o que precisa, o prestador responde com valor e horário, o cliente decide. O endereço do cliente só pode ir para o prestador depois do aceite, e os prazos evitam pedidos esquecidos.

## Scope

### In

- Cliente cria pedido para um prestador: serviço (da lista do prestador ou "Outro"), descrição, até 5 fotos, dia (hoje, amanhã, data) e período (manhã, tarde, noite), endereço salvo.
- Prestador responde com orçamento (valor fechado ou visita técnica, data e faixa de horário, material incluso, mensagem) ou recusa.
- Prestador pode substituir o orçamento enquanto o pedido estiver `Quoted`.
- Cliente aceita ou recusa o orçamento; ao aceitar, o pedido fica `Scheduled` e o endereço é liberado ao prestador.
- Cancelar (qualquer lado, com motivo, regras por estado).
- Concluir serviço (qualquer lado, a partir do início da faixa agendada).
- Expirações automáticas (48 h sem resposta; orçamento sem decisão até a validade) e conclusão automática de pedido agendado 7 dias após o fim da faixa.
- Listas de pedidos para cliente e prestador; detalhe do pedido; agenda do prestador.
- Ações permitidas calculadas pela API (`allowedActions`) para o app não duplicar regras.

### Out

- Pagamento (spec 006), avaliações (spec 007), notificações push (spec 009 — esta spec só publica eventos).
- Chat, remarcação com proposta de horário, pedido para vários prestadores de uma vez.

## Domain Model

- `Order` (agregado): `Id`, `CustomerId`, `ProviderId`, `ServiceId?`, `ServiceTitle`, `Description` (20–1000), `PreferredDay` (`Date`), `PreferredPeriod` (`Morning`, `Afternoon`, `Evening`), `AddressSnapshot` (criptografado: logradouro, número, complemento, bairro, cidade, UF, CEP), `CustomerContactSnapshot` (criptografado: nome completo e celular no momento do pedido), `Neighborhood`, `CityIbgeCode`, `CustomerPoint` (privado; centroide da cidade quando o CEP não tem coordenadas, com `CustomerPointIsApproximate`), `CategoryId` (do serviço ou, em "Outro", a principal do prestador), `Status`, `CreatedAt`, `RespondBy`, `FirstResponseAt?` (primeiro orçamento ou recusa), `QuoteValidUntil?` (o que vier primeiro entre envio + 48 h e início da faixa − 1 h), `ScheduledStart?`, `ScheduledEnd?`, `CompletedAt?`, `CompletedBy?` (`Customer`, `Provider`, `System`), `CancelledAt?`, `CancelledBy?`, `CancellationReason?`, `AddressReleasedAt?`.
- Endereço visível ao prestador de `AddressReleasedAt` até `ScheduledEnd` + 7 dias, se o pedido não foi cancelado, em qualquer status (inclusive os de pagamento).
- `Quote` (entidade do pedido): `Type` (`FixedPrice`, `TechnicalVisit`), `Amount` (`Money`), `Date`, `Start`, `End`, `MaterialIncluded`, `Message?`, `SentAt`, `Version`.
- `OrderPhoto`: `Id`, `OrderId`, `ObjectKey` (privado), `Status`.
- Estados e transições: [02-regras-de-negocio.md §4](../02-regras-de-negocio.md).
- Eventos (`order.*` e `quote.*`), conteúdo e consumidores: catálogo de [10-eventos.md](../10-eventos.md) §9. Entre eles, os que recalculam o tempo de resposta (`quote.sent`, `order.declined`, `order.expired`) e os cancelamentos tardios (`order.cancelled`).

## API Contract

| Verbo | Rota | Request | Response | Erros |
| --- | --- | --- | --- | --- |
| POST | `/api/v1/orders` | header `Idempotency-Key`; `{ providerId, serviceId?, serviceTitle?, description, preferredDay, preferredPeriod, addressId, photoKeys: [..5] }` | 201 `orderCustomerView` | 400, 401, 404 prestador, 409 limite de pedidos abertos, 409 pedir para si mesmo, 422 `order.phone_required` (conta sem celular), 422 chave reutilizada, 429 |
| POST | `/api/v1/orders/photos/upload-url` | `{ contentType, contentLength }` | `{ uploadUrl, objectKey, expiresAt }` | 400, 401, 429 |
| GET | `/api/v1/orders?status=open\|closed&page=` | — | `{ items: [orderCustomerSummary], hasMore }` | 401 |
| GET | `/api/v1/orders/{id}` | — | `orderCustomerView` com `allowedActions` | 401, 404 |
| POST | `/api/v1/orders/{id}/quote/accept` | `{ quoteVersion }` | `orderCustomerView` | 401, 404, 409 estado/versão/validade |
| POST | `/api/v1/orders/{id}/quote/decline` | `{ reason? }` | `orderCustomerView` | 401, 404, 409 |
| POST | `/api/v1/orders/{id}/cancel` | `{ reason, details? }` | `orderCustomerView` | 400, 401, 404, 409 |
| POST | `/api/v1/orders/{id}/complete` | — | `orderCustomerView` | 401, 404, 409 antes do horário |
| GET | `/api/v1/provider/orders?tab=new\|ongoing\|closed&page=` | — | `{ items: [orderProviderSummary], hasMore }` | 401 |
| GET | `/api/v1/provider/orders/{id}` | — | `orderProviderView` (endereço completo só quando liberado) com `allowedActions` | 401, 404 |
| POST | `/api/v1/provider/orders/{id}/quote` | header `Idempotency-Key`; `{ type, amount, date, start, end, materialIncluded, message? }` | `orderProviderView` | 400, 401, 404, 409, 422, 429 |
| POST | `/api/v1/provider/orders/{id}/decline` | `{ reason? }` | `orderProviderView` | 401, 404, 409 |
| POST | `/api/v1/provider/orders/{id}/cancel` | `{ reason, details? }` | `orderProviderView` | 400, 401, 404, 409 |
| POST | `/api/v1/provider/orders/{id}/complete` | — | `orderProviderView` | 401, 404, 409 |
| GET | `/api/v1/provider/agenda?from=&to=` | intervalo ≤ 31 dias | `[{ orderId, start, end, customerFirstName, neighborhood, status }]` | 400, 401 |

Fotos do pedido são lidas por URL assinada de 10 min presente no `orderView`, só para cliente e prestador do pedido.

## Authorization

- Rotas `/orders/*`: autenticado; consulta filtrada por `CustomerId = usuário`.
- Rotas `/provider/orders/*` e `/provider/agenda`: política `Provider`; consulta filtrada por `ProviderId` da vitrine do usuário.
- Pedido de outra pessoa responde 404 em qualquer rota.
- Endereço completo, nome completo e celular do cliente no `orderProviderView` só quando `AddressReleasedAt` existe, o pedido não foi cancelado e ainda não passaram 7 dias de `ScheduledEnd`; cada leitura grava auditoria.
- Antes do aceite, o prestador vê primeiro nome + inicial do cliente ("Mariana F."), bairro, cidade e distância arredondada.
- O app do cliente pede o celular antes de enviar o primeiro pedido, se a conta ainda não tiver um.

## Edge Cases & Failure Modes

- Cliente com 10 pedidos abertos (`Sent`, `Quoted`, `Scheduled`) ou 3 `Sent` na mesma categoria: 409 com mensagem clara.
- Pedido `Scheduled` que ninguém marca como concluído: 7 dias depois do fim da faixa, o job conclui com `CompletedBy = System`, avisa os dois e o endereço deixa de aparecer.
- Orçamento proposto para começar em menos de 1 h: 400 (não daria tempo de o cliente aceitar).
- Endereço sem coordenadas: distância aproximada a partir do centroide da cidade, sem aviso de "fora da área".
- Vitrine pausada ou suspensa entre abrir o perfil e enviar: 409 "Este profissional não está recebendo pedidos agora".
- Endereço fora da área do prestador: permitido, com aviso ao cliente ("fora da área habitual") e ao prestador (distância).
- Prestador envia orçamento depois de o cliente cancelar: 409 por estado (concorrência com `xmin`).
- Cliente aceita um orçamento que acabou de ser substituído: 409 por `quoteVersion`; o app recarrega e mostra o novo valor.
- Orçamento com data no passado ou faixa invertida: 400.
- Aceitar depois de `QuoteValidUntil`: 409 `quote.expired`.
- Job de expiração roda duas vezes: idempotente (só muda quem ainda está no estado e no prazo).
- Concluir antes do início da faixa agendada: 409.
- Cancelamento pelo prestador com menos de 2 h: permitido; o handler de `OrderCancelled` recalcula `LateCancellations90d` do prestador e, com 3 ou mais, abre um item na fila de moderação. Nada aparece no perfil público.
- Repetição do POST com a mesma `Idempotency-Key`: mesma resposta, sem pedido duplicado.
- Fotos ainda processando ao criar o pedido: o pedido é criado; fotos aparecem quando prontas.

## Non-Functional Requirements

- Criar pedido p95 ≤ 300 ms (sem contar upload de fotos).
- Expirações verificadas a cada 5 min; atraso máximo de 10 min.
- Fotos de pedido privadas; URL assinada de 10 min.
- Auditoria: liberação e leituras de endereço, cancelamentos com menos de 2 h.
- Todos os horários exibidos no fuso da cidade do pedido.

## Integrations

- Storage S3 compatível (fotos privadas) e job de processamento.
- Publicação de eventos na outbox (consumidos pela spec 009).

## Acceptance Criteria

- **AC-1** Dado um cliente autenticado com endereço salvo e um prestador publicado, quando cria um pedido válido, então o pedido nasce `Sent`, `RespondBy` = criação + 48 h e o prestador o vê na aba Novos com bairro e distância, sem endereço completo.
- **AC-2** Dado o mesmo `Idempotency-Key` reenviado com o mesmo corpo, quando cria o pedido, então recebe a mesma resposta e só existe um pedido.
- **AC-3** Dado um cliente que é dono da vitrine, quando pede orçamento para ela, então recebe 409.
- **AC-4** Dado um pedido `Sent`, quando o prestador envia orçamento válido para daqui a 3 dias, então o pedido vira `Quoted`, `QuoteValidUntil` = envio + 48 h, `FirstResponseAt` é preenchido e o cliente vê valor, data, faixa e material.
- **AC-5** Dado um pedido `Quoted`, quando o prestador substitui o orçamento, então `quoteVersion` aumenta, a validade é recalculada pela mesma regra e `FirstResponseAt` não muda.
- **AC-6** Dado um pedido `Quoted` com versão 2, quando o cliente aceita informando a versão 1, então recebe 409.
- **AC-7** Dado um pedido `Quoted` dentro da validade, quando o cliente aceita, então o pedido vira `Scheduled`, o endereço completo aparece na visão do prestador e uma auditoria de liberação é gravada.
- **AC-8** Dado um pedido `Sent` sem resposta por 48 h, quando o job roda, então o pedido vira `Expired` e o cliente vê sugestão de outros prestadores.
- **AC-9** Dado um pedido `Scheduled`, quando o cliente cancela sem motivo, então recebe 400; com motivo, o pedido vira `Cancelled`.
- **AC-10** Dado um pedido `Scheduled` cuja faixa ainda não começou, quando alguém marca como concluído, então recebe 409; depois do início, o pedido vira `Completed` com `CompletedBy`.
- **AC-11** Dado o id de um pedido de outro cliente, quando consulta `GET /orders/{id}`, então recebe 404.
- **AC-12** Dado um pedido `Sent`, quando o prestador consulta o detalhe, então a resposta não contém logradouro, número, complemento, CEP, celular nem sobrenome do cliente (só "Mariana F.").
- **AC-19** Dado um cliente sem celular na conta, quando tenta criar um pedido, então recebe 422 `order.phone_required` e nenhum pedido é criado.
- **AC-20** Dado um pedido aceito, quando o prestador consulta o detalhe dentro da janela, então vê nome completo e celular do cliente, e uma auditoria de leitura é gravada.
- **AC-13** Dado um pedido cuja faixa agendada terminou há 8 dias, em qualquer status, quando o prestador consulta o detalhe, então o endereço completo não aparece mais.
- **AC-14** Dado qualquer pedido, quando o app consulta o detalhe, então `allowedActions` lista só as ações válidas para quem pergunta no estado atual.
- **AC-15** Dado um orçamento enviado às 15h para amanhã às 8h, quando é criado, então `QuoteValidUntil` é amanhã às 7h; às 7h01 o aceite recebe 409 `quote.expired`.
- **AC-16** Dado um pedido `Scheduled` cuja faixa terminou há 7 dias sem conclusão, quando o job roda, então o pedido vira `Completed` com `CompletedBy = System` e os dois recebem aviso.
- **AC-17** Dado um pedido `PaymentConfirmed` com faixa terminada há 3 dias, quando o prestador consulta o detalhe, então o endereço completo aparece.
- **AC-18** Dado um cliente com 3 pedidos `Sent` em Elétrica, quando cria o quarto em Elétrica (com serviço "Outro" ou não), então recebe 409; em Hidráulica, o pedido é criado.

## Deferred Decisions

| Decisão | Fallback escolhido agora | Revisitar quando |
| --- | --- | --- |
| Remarcar com proposta de horário | Cancelar e pedir de novo, ou combinar pelo WhatsApp e o prestador ajusta a faixa com aviso ao cliente | Remarcações passarem de 10% dos agendamentos |
| Pedido para vários prestadores de uma vez | Um pedido por prestador | Clientes pedirem para comparar orçamentos lado a lado |
| Discordância de "Concluído" | Suporte por e-mail em 48 h | Volume justificar fluxo no app |

## Open Questions

- O prestador pode ajustar a faixa agendada depois do aceite sem nova aprovação do cliente? (proposta: só em até 2 h de diferença, com aviso)
- Pedidos para "Hoje" deveriam ter prazo de resposta menor que 48 h? (proposta: até o fim do dia)
