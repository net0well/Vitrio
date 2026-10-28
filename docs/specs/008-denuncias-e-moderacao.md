# Spec 008 — Denúncias e moderação

- **Status:** Draft
- **Data:** 2026-10-28
- **Responsável:** Wellington

## Problem

Um app que coloca desconhecidos em contato precisa de um jeito simples de sinalizar golpe, conteúdo ofensivo ou informação falsa, e de uma equipe pequena conseguir agir rápido. Ao mesmo tempo, uma pessoa sozinha não pode derrubar o perfil de um concorrente.

## Scope

### In

- Denunciar vitrine, foto, avaliação, resposta de avaliação ou pedido, com motivo padronizado e detalhes opcionais.
- Uma denúncia por usuário e alvo.
- Ocultação automática de foto, avaliação e resposta com 3 denúncias abertas de pessoas diferentes. Vitrine e pedido nunca são ocultados automaticamente: com 3 denúncias vão para o topo da fila.
- Fila de moderação (endpoints de admin): listar, ver contexto, manter, ocultar, restaurar, suspender e reativar contas, anular registro de pagamento (via spec 006).
- Aviso ao autor do conteúdo ocultado e ao denunciante quando houver decisão.
- Auditoria de toda ação de moderação.

### Out

- Painel web de moderação (no MVP, ferramenta interna simples ou chamadas autenticadas de admin).
- Moderação automática por IA, bloqueio entre usuários.

## Domain Model

- `Report` (agregado): `Id`, `ReporterId`, `TargetType` (`Provider`, `Photo`, `Review`, `ReviewReply`, `Order`), `TargetId`, `Reason` (`FalseInformation`, `Offensive`, `ScamOrOvercharge`, `InappropriatePhoto`, `Spam`, `Other`), `Details?` (até 500), `Status` (`Open`, `Dismissed`, `Actioned`), `CreatedAt`, `ResolvedAt?`, `ResolvedBy?`, `ResolutionNote?`. Único por (`ReporterId`, `TargetType`, `TargetId`).
- `ModerationAction`: `Id`, `AdminId`, `Action` (`Keep`, `Hide`, `Restore`, `SuspendUser`, `ReactivateUser`), `TargetType`, `TargetId`, `Reason`, `SuspendedUntil?`, `CreatedAt`.
- Conteúdos moderáveis ganham `Status` `Hidden` (foto, avaliação, resposta) ou a conta ganha `Suspended` (spec 001).
- Eventos `content.hidden`, `content.restored`, `user.suspended` e `user.reactivated`, com conteúdo e consumidores no catálogo de [10-eventos.md](../10-eventos.md) §9: avisam o autor, recalculam a nota quando o alvo é avaliação, cancelam pedidos na suspensão e invalidam o cache do perfil.
- Itens da fila também nascem de sinais automáticos: avaliação `UnderReview` (mesmo aparelho ou telefone) e prestador com 3 ou mais cancelamentos tardios em 90 dias.

## API Contract

| Verbo | Rota | Request | Response | Erros |
| --- | --- | --- | --- | --- |
| POST | `/api/v1/reports` | `{ targetType, targetId, reason, details? }` | 201 `{ id }` | 400, 401, 404 alvo, 409 já denunciado, 429 |
| GET | `/api/v1/admin/reports?status=open&targetType=&page=` | — | `{ items: [{ id, targetType, targetId, reasonCounts, firstReportedAt, targetPreview, autoHidden }], hasMore }` | 401, 403 |
| GET | `/api/v1/admin/reports/{targetType}/{targetId}` | — | contexto do alvo + todas as denúncias + histórico de ações | 401, 403, 404 |
| POST | `/api/v1/admin/moderation-actions` | `{ action, targetType, targetId, reason, suspendedUntil? }` | 201 | 400, 401, 403, 404, 409 |

## Authorization

- Denunciar: autenticado; não é possível denunciar o próprio conteúdo (409).
- Rotas `/admin/*`: política `Admin` no controller; toda ação exige `reason` e gera `audit_event`.
- O denunciante nunca é revelado ao denunciado.
- Respostas públicas nunca mostram contagem de denúncias.

## Edge Cases & Failure Modes

- Mesma pessoa denuncia de novo: 409 com mensagem "Você já denunciou este conteúdo".
- Três contas recém-criadas denunciando o mesmo alvo em minutos: se for foto, avaliação ou resposta, a ocultação acontece, o item ganha prioridade na fila e as contas são sinalizadas; se for vitrine, ela continua no ar e só o admin decide.
- Alvo removido pelo autor antes da revisão: denúncias encerradas como `Dismissed` com nota automática.
- Suspensão de uma conta: todas as sessões revogadas; pedidos abertos (`Sent`, `Quoted`, `Scheduled`) em que ela é cliente ou prestador cancelados com motivo "Conta suspensa"; o outro lado é avisado (com sugestões, se for cliente).
- Restaurar conteúdo: denúncias abertas daquele alvo encerradas como `Dismissed`.
- Admin tentando moderar a própria conta: 409.

## Non-Functional Requirements

- Ocultação automática em até 1 min após a terceira denúncia.
- Meta operacional: fila revisada em até 24 h úteis.
- Auditoria imutável de toda ação.
- Rate limit `reports`: 10 por dia por usuário.

## Integrations

Eventos para notificações (spec 009). Nenhuma integração externa.

## Acceptance Criteria

- **AC-1** Dado um usuário autenticado, quando denuncia uma avaliação com motivo "Ofensivo", então a denúncia é registrada como `Open`.
- **AC-2** Dado que o usuário já denunciou aquela avaliação, quando denuncia de novo, então recebe 409.
- **AC-3** Dadas denúncias abertas de 3 pessoas diferentes para a mesma foto, quando a terceira é registrada, então a foto some do perfil público em até 1 min.
- **AC-4** Dado um item oculto automaticamente, quando um admin escolhe "Restaurar", então ele volta a aparecer e as denúncias abertas ficam `Dismissed`.
- **AC-5** Dado um admin, quando suspende um prestador por 30 dias com motivo, então a vitrine some da busca e do perfil, os pedidos `Sent`, `Quoted` e `Scheduled` dele são cancelados com motivo, o access token atual é recusado em até 1 min e o login recebe 403.
- **AC-6** Dado um usuário sem papel admin, quando chama qualquer rota `/admin/*`, então recebe 403.
- **AC-7** Dada qualquer ação de moderação, quando a auditoria é consultada, então existe registro com admin, alvo, ação, motivo e data.
- **AC-8** Dado um usuário, quando tenta denunciar a própria vitrine, então recebe 409.
- **AC-9** Dadas denúncias abertas de 3 pessoas diferentes para a mesma vitrine, quando a terceira é registrada, então a vitrine continua publicada e o item aparece no topo da fila.

## Deferred Decisions

| Decisão | Fallback escolhido agora | Revisitar quando |
| --- | --- | --- |
| Painel web de moderação | Ferramenta interna mínima + endpoints | Mais de 20 denúncias por dia |
| Classificação automática de conteúdo | Revisão humana | Volume justificar |

## Open Questions

- Quem são os admins no lançamento e qual canal de contato público para denúncias graves (golpe, ameaça)?
- O denunciante deve receber o resultado em detalhe ou só "Analisamos sua denúncia"? (proposta: só o aviso genérico)
