# Select

Campo de escolha numa lista fechada, com rótulo visível, como o `TextField`.

- Forneça `label`, `value`, `options` (valor e rótulo) e `onChange(valor)`.
- Mesma caixa do `TextField`: `size-control-lg` de altura, raio `radius-lg`, fundo `surface-raised` e borda `line-strong`. O valor escolhido fica em `label` com peso 700 e `ink`; à direita, `chevron-down` de 16px em `ink-muted`.
- A lista abre sobre a tela em `surface-raised` com raio `radius-lg`; a opção em foco usa `plum-soft`.
- Usado em: dia, início e duração de Responder pedido e de Ajustar horário. Início e Duração dividem a linha, com `sp-10` entre as caixas.
- Não: Select para duas ou três opções (use `SegmentedControl`), placeholder no lugar do rótulo, caixa sem valor escolhido.
