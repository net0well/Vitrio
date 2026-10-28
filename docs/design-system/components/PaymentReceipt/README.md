# PaymentReceipt

Registro do pagamento confirmado pelo prestador, visto pelos dois lados.

- Forneça `amount`, `method`, `date`, `confirmedBy`; opcionais `note` (motivo de valor diferente) e `onDispute` (só para o cliente).
- O pagamento é combinado fora do app; este card é o registro, não um comprovante bancário. O texto deixa isso claro.
- "Não reconheço este pagamento" abre a contestação no suporte; só aparece para o cliente e até 7 dias depois.
