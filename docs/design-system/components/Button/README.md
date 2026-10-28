# Button

Botão de ação. `primary` (plum) é a ação principal e aparece no máximo uma vez por tela.

- Variantes: `primary`, `secondary` (contornado, para a alternativa: Recusar, Remarcar), `ghost` (link de ação: Ver todas), `danger` (Cancelar pedido), `accent` e `on-hero` (só sobre superfície `hero`).
- `size="lg"` (56px) na barra de ação e nas telas de marca; `md` (44px) dentro de cards.
- Rótulo com verbo no infinitivo, sentence case: "Enviar pedido", "Aceitar orçamento", "Confirmar recebimento".
- `trailingIcon="arrow-right"` ou `"send"` no botão principal; o ícone ganha `plum-accent`.
- Passe `href` para navegar (vira link); `onClick` para ações.
- Não: dois `primary` na mesma tela, botão desabilitado sem explicar por quê, caixa alta.
