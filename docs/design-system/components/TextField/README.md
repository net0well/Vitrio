# TextField

Campo de uma linha com rótulo visível, dica e erro.

- Forneça `label` e `value`/`onChange(valor)` (ou `defaultValue`). Opcionais: `prefix` ("R$" muda o valor para a fonte de números), `icon`, `hint`, `error`, `maxLength` (mostra contador), `inputMode`.
- Borda `line-strong`; ao focar, `plum` com `focus-ring`; com erro, `danger` e a mensagem no lugar da dica.
- Mensagens de erro dizem o que fazer: "CNPJ inválido. Confira os 14 números."
- Não: placeholder no lugar do rótulo, erro só por cor.
