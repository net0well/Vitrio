# Spec 009 — Notificações

- **Status:** Draft
- **Data:** 2026-10-28
- **Responsável:** Wellington

## Problem

O ciclo do pedido depende de as duas partes agirem no tempo certo: o prestador responder em 48 h, o cliente aceitar o orçamento, os dois lembrarem do horário, o prestador confirmar o pagamento e o cliente avaliar. Sem avisos confiáveis, pedidos expiram e o app perde valor. Avisos demais ou de madrugada fazem a pessoa desligar tudo.

## Scope

### In

- Push via FCM para Android, a partir dos eventos do catálogo de [10-eventos.md](../10-eventos.md) §9 (handlers `AccountNotifications`, `OrderNotifications`, `PaymentNotifications` e `ReviewNotifications`). iOS (APNs via FCM) entra com o app iOS.
- Lembretes e convites são criados por job de varredura, conferindo o estado atual do pedido, e não a partir de evento.
- Central de notificações no app (lidas e não lidas, 90 dias).
- Preferências por tipo (pedidos, lembretes, avaliações, novidades da conta); avisos de conta e segurança não podem ser desligados.
- Silêncio noturno (22h–7h no fuso do destinatário) para lembretes e convites.
- Lembretes agendados: 1 h antes do horário (ou às 21h da noite anterior, se 1 h antes cair no silêncio noturno); prestador sem confirmar pagamento 24 h após `Completed`; cliente sem avaliar 2 h após `Completed` (um lembrete só).
- Registro e remoção de dispositivos (rotas da spec 001).

### Out

- SMS e e-mails além dos de conta (código e avisos de senha, da spec 001).
- Notificações de marketing.

## Domain Model

- `Notification`: `Id`, `UserId`, `Type`, `Title`, `Body`, `Data` (ids para navegação, sem dados pessoais), `CreatedAt`, `ReadAt?`, `ExpiresAt` (90 dias).
- `NotificationPreference`: `UserId`, `Category` (`Orders`, `Reminders`, `Reviews`, `Account`), `PushEnabled`.
- `Device` (spec 001).
- `PushDelivery`: `NotificationId`, `DeviceId`, `Status` (`Pending`, `Sent`, `Failed`, `Deferred`), `Attempts`, `LastError?`, `SendAfter`.

Os nomes abaixo são os do `record`; o nome estável, o conteúdo e os demais consumidores estão no catálogo de [10-eventos.md](../10-eventos.md) §9.

| Evento | Destinatário | Tipo | Categoria |
| --- | --- | --- | --- |
| `OrderCreated` | Prestador | Novo pedido | Orders |
| `QuoteSent` | Cliente | Orçamento recebido | Orders |
| `QuoteAccepted` | Prestador | Orçamento aceito | Orders |
| `QuoteDeclined` | Prestador | Orçamento recusado | Orders |
| `OrderDeclined`, `OrderExpired` | Cliente | Pedido não atendido | Orders |
| `QuoteExpired` | Os dois | Orçamento expirado | Orders |
| `OrderCancelled` | O outro lado (os dois, quando o motivo é suspensão ou exclusão de conta) | Pedido cancelado | Orders |
| Lembrete 1 h | Os dois | Lembrete | Reminders |
| `OrderCompleted` | O outro lado (os dois, quando `CompletedBy = System`) | Serviço concluído | Orders |
| Lembrete de confirmação | Prestador | Confirme o pagamento | Reminders |
| `PaymentConfirmed` | Cliente | Pagamento confirmado | Orders |
| `PaymentConfirmationVoided` | Cliente | Confirmação de pagamento anulada | Orders |
| `PaymentDisputed` | Prestador | Pagamento contestado | Account |
| `PaymentDisputeResolved` | Os dois | Contestação resolvida | Account |
| Convite para avaliar | Cliente | Avalie o serviço | Reviews |
| `ReviewPublished` | Prestador | Nova avaliação | Reviews |
| `ReviewReplied` | Cliente | Resposta à avaliação | Reviews |
| `mei.verified`, `mei.verification_failed` (só quando a verificação estava `Pending`) | Prestador | Selo MEI confirmado / verificação não concluída | Account |
| `mei.verification_lost` | Prestador | Selo MEI removido | Account |
| `ContentHidden`, `UserSuspended` | Autor | Conteúdo ocultado / conta suspensa | Account |
| `ContentRestored`, `UserReactivated` | Autor | Conteúdo restaurado / conta reativada | Account |

Textos de referência em `docs/design-system/telas.md`.

## API Contract

| Verbo | Rota | Request | Response | Erros |
| --- | --- | --- | --- | --- |
| GET | `/api/v1/me/notifications?page=` | — | `{ items: [{ id, type, title, body, data, createdAt, read }], unreadCount, hasMore }` | 401 |
| POST | `/api/v1/me/notifications/{id}/read` | — | 204 | 401, 404 |
| POST | `/api/v1/me/notifications/read-all` | — | 204 | 401 |
| GET | `/api/v1/me/notification-preferences` | — | `[{ category, pushEnabled, locked }]` | 401 |
| PUT | `/api/v1/me/notification-preferences` | `[{ category, pushEnabled }]` | 204 | 400 (tentar desligar `Account`), 401 |

## Authorization

- Tudo autenticado e filtrado pelo usuário; notificação de outro usuário responde 404.
- Payload do push sem dados pessoais (ex.: "Você recebeu um novo pedido"); o texto completo, com nomes e valores, fica só na central de notificações, lida com o usuário autenticado.

## Edge Cases & Failure Modes

- Token FCM inválido ou desinstalado: dispositivo removido após resposta definitiva do FCM.
- FCM indisponível: retry com backoff (até 6 tentativas em 1 h); a notificação continua na central.
- Evento processado duas vezes pela outbox: deduplicação por (`eventId`, `userId`, `type`).
- Silêncio noturno: entrega `Deferred` para as 7h do fuso do destinatário; se o evento perder sentido até lá (lembrete de um serviço já concluído), é descartado.
- Usuário com vários aparelhos: envia para todos os ativos.
- Usuário em modo cliente recebe aviso de prestador: o toque abre o app no modo certo.
- Preferência desligada: notificação fica só na central.

## Non-Functional Requirements

- Do evento ao push em até 30 s no p95 (fora do silêncio noturno).
- Central p95 ≤ 150 ms.
- Limpeza diária de notificações com mais de 90 dias.
- Logs de entrega sem conteúdo da mensagem, só ids e status.

## Integrations

- Firebase Cloud Messaging (Admin SDK) com credencial de serviço no cofre.
- APNs via FCM, com o app iOS.

## Acceptance Criteria

- **AC-1** Dado um pedido novo, quando o evento é processado, então o prestador recebe push "Novo pedido" e a notificação aparece na central como não lida.
- **AC-2** Dado um prestador com a categoria Orders desligada, quando chega um pedido, então não há push, mas a notificação aparece na central.
- **AC-3** Dado um lembrete que cairia às 23h no fuso do destinatário, quando é agendado, então é entregue às 7h ou descartado se o pedido já estiver concluído.
- **AC-4** Dado o mesmo evento entregue duas vezes pela outbox, quando processado, então só existe uma notificação.
- **AC-5** Dado um token FCM inválido, quando o envio falha de forma definitiva, então o dispositivo é removido.
- **AC-6** Dado qualquer push enviado, quando o payload é inspecionado, então não contém nome, endereço, valor nem telefone.
- **AC-7** Dado um usuário, quando tenta desligar a categoria Account, então recebe 400.
- **AC-8** Dado um pedido `Completed` sem confirmação de pagamento por 24 h, quando o job roda, então o prestador recebe um único lembrete.
- **AC-9** Dado um serviço agendado para as 7h30, quando o lembrete é agendado, então ele sai às 21h da noite anterior, no fuso do destinatário.
- **AC-10** Dado um orçamento recusado pelo cliente, quando o evento é processado, então o prestador recebe a notificação "Orçamento recusado".

## Deferred Decisions

| Decisão | Fallback escolhido agora | Revisitar quando |
| --- | --- | --- |
| E-mail transacional | Só push e central | Taxa de opt-out de push > 30% |
| Agrupar notificações | Uma por evento | Prestadores com muitos pedidos reclamarem |

## Open Questions

- O horário de silêncio noturno deve ser configurável pelo usuário? (proposta: fixo 22h–7h no MVP)
