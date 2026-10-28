# StatusTimeline

Linha do tempo do pedido, da criação ao pagamento confirmado.

- Forneça `steps` (`{ title, sub, state }`), com `state` `done`, `current` ou `pending`.
- Etapas do pedido: Pedido enviado → Orçamento recebido → Serviço agendado → Serviço concluído → Pagamento confirmado.
- A etapa atual tem anel `plum` e centro `lime`; concluídas, check em `green`. O texto de apoio diz quem precisa agir: "Aguardando sua resposta", "Carlos vai confirmar o pagamento".
