# Spec 007 — Avaliações

- **Status:** Draft
- **Data:** 2026-10-28
- **Responsável:** Wellington

## Problem

A nota é o que faz o cliente escolher um prestador e o que dá ao prestador uma reputação que ele não tinha como provar. Ela só vale se for de quem contratou de verdade, difícil de manipular e justa com quem tem poucas avaliações.

## Scope

### In

- Cliente avalia um pedido a partir de `Completed` e por até 30 dias depois da conclusão: nota 1–5, tags, comentário, até 3 fotos, "Recomendo".
- Uma avaliação por pedido; edição por 7 dias.
- Prestador responde uma vez (editável por 7 dias).
- Nota exibida com média bayesiana; "Novo no Vitrio" com menos de 3 avaliações.
- Resumo no perfil: distribuição de notas, tags mais citadas, total de recomendações.
- Lista paginada de avaliações do prestador, com filtro por nota.

### Out

- Avaliação do cliente pelo prestador.
- Avaliações sem pedido (importadas, convidadas).
- Denúncia de avaliação (spec 008).

## Domain Model

- `Review` (agregado): `Id`, `OrderId` (único), `ProviderId`, `CustomerId`, `Rating` (1–5), `Tags` (subconjunto de `Punctual`, `Careful`, `FairPrice`, `ExplainedWell`, `LeftClean`, `Polite`), `Comment?` (até 400), `Recommends`, `PhotoKeys` (até 3), `CreatedAt`, `UpdatedAt?`, `EditableUntil`, `Status` (`Visible`, `UnderReview`, `Hidden`), `AuthorDisplayName` (primeiro nome + inicial, congelado na publicação). `UnderReview` não aparece nem conta na nota até a moderação decidir.
- `ReviewReply`: `ReviewId`, `Text` (até 400), `CreatedAt`, `UpdatedAt?`, `EditableUntil`.
- Agregados de leitura no `Provider` (recalculados do zero, só com avaliações `Visible`, pelo handler `RecalculateProviderRating`, que consome `review.published`, `review.updated`, `content.hidden` e `content.restored`; ocultar gera `content.hidden`, e restaurar ou aprovar uma avaliação em revisão gera `content.restored`, ambos com `TargetType = Review`). Catálogo em [10-eventos.md](../10-eventos.md) §9: `ReviewCount`, `RatingSum`, `RatingScore` (bayesiana), `RatingDistribution` (1–5), `TagCounts`, `RecommendCount`.
- Média bayesiana: `(C × m + soma) / (C + n)`, `C = 5`, `m` = média global da categoria principal. Um job diário recalcula `m` de cada categoria e, em seguida, `RatingScore` de todos os prestadores; sem isso, quem não recebe avaliação nova ficaria com a nota calculada sobre um `m` antigo.

## API Contract

| Verbo | Rota | Request | Response | Erros |
| --- | --- | --- | --- | --- |
| POST | `/api/v1/orders/{id}/review` | `{ rating, tags, comment?, recommends, photoKeys? }` | 201 `reviewView` | 400 (texto com contato, nota fora da faixa), 401, 404, 409 já avaliado ou estado inválido |
| PUT | `/api/v1/reviews/{id}` | idem | `reviewView` | 400, 401, 404, 409 fora dos 7 dias |
| POST | `/api/v1/reviews/{id}/reply` | `{ text }` | 201 `replyView` | 400, 401, 404, 409 já respondida |
| PUT | `/api/v1/reviews/{id}/reply` | `{ text }` | `replyView` | 400, 401, 404, 409 fora dos 7 dias |
| POST | `/api/v1/reviews/photos/upload-url` | `{ contentType, contentLength }` | `{ uploadUrl, objectKey, expiresAt }` | 400, 401, 429 |
| GET | `/api/v1/providers/{id}/reviews?rating=&page=` | — | `{ summary: { ratingScore, reviewCount, distribution, topTags, recommendCount }, items: [reviewView], hasMore }` | 404 |

`reviewView`: `{ id, authorDisplayName, authorInitials, rating, tags, comment, recommends, photos, serviceTitle, createdAt, reply? }`.

## Authorization

- Criar e editar: só o cliente do pedido (filtro na consulta); 404 para pedido alheio.
- Responder: só o prestador avaliado (política `Provider` + filtro por `ProviderId`).
- Leitura: anônima, política `profile`; avaliações `Hidden` não aparecem.
- `status`, `authorDisplayName` e agregados nunca vêm do request.

## Edge Cases & Failure Modes

- Pedido ainda `Scheduled`, `Cancelled` ou `Expired`: 409.
- Mais de 30 dias depois da conclusão: 409 `review.window_closed`.
- Segunda avaliação para o mesmo pedido: 409 (índice único em `OrderId`).
- Comentário com telefone, e-mail ou link: 400 com mensagem ("Avaliações não podem ter contato ou links").
- Edição após 7 dias: 409.
- Pedido com pagamento contestado: avaliação permitida normalmente.
- Cliente e prestador ligados ao mesmo aparelho (`InstallationId`) ou ao mesmo telefone informado (sem verificação no MVP): avaliação aceita como `UnderReview`, fora da lista e do cálculo até a moderação decidir; vai para a fila de moderação.
- Avaliação ocultada por moderação: sai do cálculo e da lista; volta se restaurada, e a nota é recalculada nos dois casos (`content.hidden` e `content.restored`).
- Conta do autor excluída: autor vira "Usuário removido", fotos permanecem.
- Recalcular nota com muitas avaliações simultâneas: handler idempotente que recalcula a partir das avaliações visíveis, não por incremento cego.

## Non-Functional Requirements

- Nota do prestador atualizada em até 1 min após a publicação (outbox).
- Listagem p95 ≤ 150 ms com paginação por `CreatedAt` desc.
- Fotos de avaliação públicas, processadas como as da vitrine (sem EXIF).

## Integrations

Storage e job de imagens. Eventos para notificações (spec 009).

## Acceptance Criteria

- **AC-1** Dado um pedido `Completed` do cliente, quando ele avalia com nota 5, tags e comentário, então a avaliação aparece no perfil com autor "Primeiro nome + inicial".
- **AC-2** Dado um pedido `Scheduled`, quando o cliente tenta avaliar, então recebe 409.
- **AC-3** Dado um pedido já avaliado, quando o cliente tenta avaliar de novo, então recebe 409.
- **AC-4** Dado um comentário com um número de telefone, quando o cliente envia, então recebe 400.
- **AC-5** Dada uma avaliação com 3 dias, quando o cliente edita a nota, então a nova nota vale e a nota do prestador é recalculada.
- **AC-6** Dada uma avaliação com 8 dias, quando o cliente tenta editar, então recebe 409.
- **AC-7** Dada uma avaliação do seu perfil, quando o prestador responde, então a resposta aparece abaixo; uma segunda resposta recebe 409.
- **AC-8** Dado um prestador com 2 avaliações visíveis, quando aparece na busca ou no perfil, então mostra "Novo no Vitrio" em vez da nota; com 3, mostra a nota.
- **AC-9** Dado um prestador com notas 5, 5, 4 e média global da categoria 4,6, quando a nota é calculada, então o valor é (5 × 4,6 + 14) / 8 = 4,625, exibido como 4,6.
- **AC-10** Dado um pedido de outro cliente, quando alguém tenta avaliá-lo, então recebe 404.
- **AC-11** Dada uma avaliação ocultada por moderação, quando a lista e a nota são consultadas, então ela não aparece nem conta.
- **AC-12** Dada uma avaliação ocultada que um admin restaura, quando a nota é consultada, então ela volta a contar.
- **AC-13** Dado um cliente avaliando do mesmo aparelho que o prestador usa, quando publica, então a avaliação fica `UnderReview`, não aparece e não muda a nota.
- **AC-14** Dado um pedido concluído há 31 dias, quando o cliente tenta avaliar, então recebe 409.
- **AC-15** Dada uma mudança na média da categoria, quando o job diário roda, então a nota de um prestador sem avaliações novas é recalculada com o novo `m`.

## Deferred Decisions

| Decisão | Fallback escolhido agora | Revisitar quando |
| --- | --- | --- |
| Avaliar o cliente | Não existe | Prestadores pedirem filtro de clientes |
| Moderação prévia de comentários | Publicação imediata + denúncia + filtro de contato | Taxa de ocultação > 2% |

## Open Questions

- Lista final de tags (proposta atual: Pontual, Caprichoso, Preço justo, Explicou bem, Deixou tudo limpo, Educado) e se varia por categoria.
- Mostrar a data exata ou relativa ("há 2 dias")? (proposta: relativa até 30 dias, depois mês/ano)
