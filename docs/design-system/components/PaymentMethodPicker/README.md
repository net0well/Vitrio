# PaymentMethodPicker

Escolha da forma de pagamento quando o prestador confirma o recebimento.

- Forneça `value`/`onChange(forma)` (ou `defaultValue`). As formas padrão são Pix, Dinheiro, Cartão de débito, Cartão de crédito, Transferência e Outro; troque com `methods` se a regra mudar.
- Só o prestador usa este componente, na tela Confirmar pagamento. O cliente vê o resultado em `PaymentReceipt`.
- Pix aparece com o ícone `qr` e a palavra; nunca com o logo oficial.
