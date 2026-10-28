# Spec 006 — Confirmação de pagamento

- **Status:** Draft
- **Data:** 2026-10-28
- **Responsável:** Wellington

## Problem

No MVP o pagamento é combinado entre cliente e prestador, fora do app. Mesmo assim, os dois precisam de um registro confiável: o prestador quer controlar o que recebeu e o cliente quer ver que o valor e o meio registrados batem com o que pagou. Só o prestador confirma, informando valor e meio; o cliente vê e pode contestar.

## Scope

### In

- Prestador confirma o recebimento de um pedido `Completed`: valor, meio, data e hora, observação e, se o valor diferir do orçamento, motivo da diferença.
- Registro imutável; anulação pelo próprio prestador em até 24 h, com motivo, para corrigir erro e confirmar de novo.
- Cliente vê o registro no pedido e pode contestar em até 7 dias.
- Suporte resolve contestação: mantém o registro ou anula.
- Resumo de recebimentos do prestador por mês (total e por meio).

### Out

- Cobrança, intermediação, split, repasse e qualquer dado de cartão ou conta bancária.
- Emissão de nota fiscal ou recibo fiscal.

## Domain Model

- `PaymentConfirmation` (entidade do agregado `Order`): `Id`, `OrderId`, `Amount` (`Money`), `Method` (`Pix`, `Cash`, `DebitCard`, `CreditCard`, `BankTransfer`, `Other`), `ReceivedAt`, `Note?` (até 300), `DifferenceReason?` (até 300, obrigatório se `Amount` ≠ valor do orçamento aceito), `ConfirmedAt`, `Status` (`Valid`, `Voided`), `VoidedAt?`, `VoidedBy?` (`Provider`, `Support`), `VoidReason?`.
- `PaymentDispute`: `Id`, `PaymentConfirmationId`, `Reason` (`AmountDiffers`, `MethodDiffers`, `NotPaidYet`, `NeverHired`, `Other`), `Details?` (até 500), `OpenedAt`, `Status` (`Open`, `ResolvedKept`, `ResolvedVoided`), `ResolvedAt?`, `ResolutionNote?`.
- Transições de `Order`: `Completed` → `PaymentConfirmed`; `PaymentConfirmed` → `PaymentDisputed` → `PaymentConfirmed` ou `PaymentVoided`; anulação pelo prestador em 24 h volta o pedido para `Completed` (e permite confirmar de novo). `PaymentVoided` é final: nova confirmação só pelo suporte.
- Eventos (`payment.*`), conteúdo e consumidores: catálogo de [10-eventos.md](../10-eventos.md) §9.
- Leitura: `EarningsSummary` por prestador e mês (`Total`, `Count`, por `Method`), só de confirmações `Valid`. O mês é o de `ReceivedAt` no fuso da cidade do pedido (um Pix recebido às 23h30 de 31/10 em Manaus entra em outubro).

## API Contract

| Verbo | Rota | Request | Response | Erros |
| --- | --- | --- | --- | --- |
| POST | `/api/v1/provider/orders/{id}/payment-confirmation` | header `Idempotency-Key`; `{ amount, method, receivedAt, note?, differenceReason? }` | 201 `paymentConfirmationView` | 400 (valor fora da faixa, data futura ou anterior ao pedido, motivo ausente), 401, 404, 409 estado, 422 chave reutilizada |
| POST | `/api/v1/provider/orders/{id}/payment-confirmation/void` | `{ reason }` | `orderProviderView` | 400, 401, 404, 409 fora das 24 h ou com contestação aberta |
| GET | `/api/v1/provider/earnings?month=YYYY-MM` | — | `{ month, total, count, byMethod: [{ method, total, count }] }` | 400, 401 |
| POST | `/api/v1/orders/{id}/payment/dispute` | `{ reason, details? }` | `orderCustomerView` | 400, 401, 404, 409 fora dos 7 dias ou já contestado |
| POST | `/api/v1/admin/payment-disputes/{id}/resolve` | `{ outcome: "keep" \| "void", note }` | 204 | 400, 401, 403, 404, 409 |

`paymentConfirmationView`: `{ amount, method, receivedAt, confirmedAt, note, differenceReason, status, canDisputeUntil }` (cliente) ou com `canVoidUntil` (prestador).

## Authorization

- Confirmar e anular: política `Provider` e o pedido precisa ser da vitrine do usuário (filtro na consulta; 404 caso contrário).
- Contestar: só o cliente do pedido.
- Resolver contestação: política `Admin`, com motivo obrigatório.
- `status`, `confirmedAt`, `voidedBy` nunca vêm do request.
- Auditoria: confirmação, anulação, contestação e resolução (quem, quando, pedido, valores antes e depois).

## Edge Cases & Failure Modes

- Confirmar pedido que não está `Completed`: 409 com a ação esperada ("Marque o serviço como concluído primeiro").
- Valor diferente do orçamento sem motivo: 400 no campo `differenceReason`.
- Valor fora de R$ 1,00 a R$ 100.000,00: 400.
- `receivedAt` no futuro ou antes da criação do pedido: 400.
- Duas confirmações simultâneas (dois aparelhos): a segunda recebe 409 por concorrência ou a mesma resposta se usar a mesma `Idempotency-Key`.
- Anular com contestação aberta: 409 (decisão fica com o suporte).
- Contestar após 7 dias: 409, com orientação de suporte por e-mail.
- Pedido cancelado: não aceita confirmação.
- Conta do cliente excluída: registro mantido anonimizado; contestação não é mais possível.

## Non-Functional Requirements

- Valores com `numeric(12,2)` e aritmética em `decimal`; exibição em BRL no fuso da cidade do pedido.
- Registros nunca são apagados; anulação é uma mudança de status com auditoria.
- Retenção de 5 anos, anonimizada após exclusão de conta. A auditoria de confirmação, anulação, contestação e resolução também fica 5 anos.
- Resumo de recebimentos p95 ≤ 200 ms (índice por `provider_id, received_at`).
- `PaymentConfirmationVoided` e a resolução que anula recalculam os "serviços feitos pelo app" do prestador.

## Integrations

Nenhuma externa. Eventos na outbox para notificações (spec 009).

## Acceptance Criteria

- **AC-1** Dado um pedido `Completed` com orçamento aceito de R$ 180,00, quando o prestador confirma R$ 180,00 via Pix, então o pedido vira `PaymentConfirmed` e o cliente vê "Pagamento confirmado: R$ 180,00 · Pix".
- **AC-2** Dado o mesmo pedido, quando o prestador confirma R$ 210,00 sem motivo, então recebe 400 no campo `differenceReason`.
- **AC-3** Dado o mesmo pedido, quando confirma R$ 210,00 com motivo "Material extra", então o registro guarda o motivo e o cliente o vê.
- **AC-4** Dado um pedido `Scheduled`, quando o prestador tenta confirmar, então recebe 409.
- **AC-5** Dado um pedido de outra vitrine, quando o prestador tenta confirmar, então recebe 404.
- **AC-6** Dada uma confirmação feita há 2 h, quando o prestador anula com motivo, então ela fica `Voided`, o pedido volta a `Completed` e uma nova confirmação é possível.
- **AC-7** Dada uma confirmação feita há 25 h, quando o prestador tenta anular, então recebe 409.
- **AC-8** Dada uma confirmação válida há 3 dias, quando o cliente contesta com motivo, então o pedido vira `PaymentDisputed` e a contestação entra na fila do suporte.
- **AC-9** Dada uma contestação aberta, quando o suporte resolve com `void`, então a confirmação fica `Voided`, o pedido vira `PaymentVoided` e o valor sai do resumo de recebimentos.
- **AC-10** Dadas confirmações válidas de R$ 180 (Pix) e R$ 90 (Dinheiro) e uma anulada de R$ 50 no mês, quando o prestador consulta o resumo, então o total é R$ 270,00 com 2 registros.
- **AC-11** Dado o mesmo `Idempotency-Key` reenviado, quando confirma de novo, então recebe a mesma resposta e só existe uma confirmação.
- **AC-12** Dada qualquer confirmação, anulação, contestação ou resolução, quando a auditoria é consultada, então existe um evento com autor, data e valores.
- **AC-13** Dado um pedido `PaymentVoided`, quando o prestador tenta confirmar de novo, então recebe 409.
- **AC-14** Dada uma confirmação com `ReceivedAt` às 23h30 de 31/10 no fuso de Manaus, quando o prestador consulta o resumo de outubro, então ela está incluída.

## Deferred Decisions

| Decisão | Fallback escolhido agora | Revisitar quando |
| --- | --- | --- |
| Pagamento dentro do app | Fora do app, com registro do prestador | Pedidos com pagamento confirmado > 1.000/mês ou demanda por garantia |
| Recibo para o cliente | Card no pedido, sem PDF | Pedido recorrente de clientes |
| Lembrete ao prestador para confirmar | Push 24 h depois de `Completed` sem confirmação (spec 009) | — |

## Open Questions

- Pagamentos parciais (sinal + restante) entram no MVP? (proposta: não; uma confirmação por pedido)
- Contestação procedente suspende o prestador automaticamente a partir de quantas ocorrências? (proposta: revisão manual a partir de 2 em 90 dias)
