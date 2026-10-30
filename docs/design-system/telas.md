# Plano de telas e fluxos

Um só app com dois modos: **cliente** (padrão) e **prestador** (ativado em "Quero oferecer meus serviços"). Funciona em qualquer cidade do Brasil: com o GPS ativo mostra os profissionais mais próximos; sem ele, o cliente escolhe a cidade. Telas marcadas como **prototipada** já existem no canvas de protótipo ([Vitrio — Protótipo do app](https://claude.ai/artifact/Letmw6AR9zfrFK7gaA79xt)) e o app as reproduz **igual**: mesma estrutura, componentes e textos, com os dados reais no lugar dos exemplos. As demais seguem os mesmos componentes. Diferença só com aprovação registrada na spec.

## Navegação

| Modo | Abas da navegação inferior (`BottomNav`) |
| --- | --- |
| Cliente | Início · Buscar · Pedidos · Conta |
| Prestador | Painel · Pedidos · Agenda · Meu perfil |

- Telas de topo usam `BottomNav`. Telas de tarefa (pedir orçamento, responder, avaliar, confirmar pagamento) usam `TopBar` + `ActionBar` e escondem a navegação.
- Troca de modo fica em Conta ("Entrar como prestador" / "Voltar para cliente"). O modo ativo fica salvo no aparelho.

## Localização e cidade

- **GPS ativo:** a busca usa o ponto atual, ordena por relevância (a proximidade é um dos pesos; "Mais perto" fica disponível na ordenação) e mostra "1,2 km · Bairro". A pílula de localização da Início mostra "Bairro, Cidade" detectados.
- **GPS negado ou desligado:** a pílula mostra a cidade escolhida; a busca ordena por relevância (nota, número de avaliações, perfil completo) e mostra só bairro e cidade, sem distância.
- **Filtrar por cidade:** a pílula abre a folha "Escolher cidade" (busca por nome com acentos opcionais, cidades recentes, "Usar minha localização"). Escolher outra cidade não desliga o GPS; só muda o centro da busca até o usuário voltar para "Perto de mim".
- **Privacidade do prestador:** o endereço base do prestador nunca aparece. O app mostra bairro, cidade e distância arredondada (uma casa decimal até 10 km, inteiro acima).
- **Pré-permissão:** antes do pedido do sistema, uma tela explica em uma frase por que a localização ajuda ("Para mostrar quem atende perto de você"). Os dois caminhos são iguais em peso: "Usar minha localização" (`Button` primary) e "Escolher cidade" (`Button` secondary).

## Ciclo de vida do pedido

O pagamento é combinado entre cliente e prestador fora do app. **Só o prestador confirma o recebimento**, informando valor e meio de pagamento; o cliente vê o registro e pode contestar.

| Status | Quem move | O cliente vê | O prestador vê | Próxima ação |
| --- | --- | --- | --- | --- |
| Enviado | Cliente envia | "Aguardando orçamento" | Card "Novo" no Painel | Prestador: Enviar orçamento ou Recusar |
| Recusado | Prestador | "Carlos não pode atender agora" + sugestões de outros | Some da lista | Fim |
| Expirado | Sistema (48 h sem resposta) | "Sem resposta" + sugestões | Some da lista | Fim |
| Orçamento enviado | Prestador | `QuoteCard` com valor, horário e validade | "Aguardando cliente" | Cliente: Aceitar ou Recusar |
| Orçamento recusado | Cliente | Pedido arquivado | Aviso | Fim |
| Agendado | Cliente aceita | Data, horário, valor combinado; endereço liberado ao prestador | Agenda + endereço completo (até 7 dias após o horário) | Qualquer um: Cancelar; ao terminar, Serviço concluído. Sem ação de ninguém, o pedido é concluído sozinho 7 dias após o horário |
| Cancelado | Cliente ou prestador, com motivo | Motivo | Motivo | Fim |
| Concluído | Quem marcar primeiro; o outro é avisado | "Serviço concluído" + convite para avaliar | "Confirme o pagamento" | Prestador: Confirmar pagamento; Cliente: Avaliar |
| Pagamento confirmado | Só o prestador | `PaymentReceipt` (valor, meio, data) + "Não reconheço este pagamento" | Registro no histórico e no faturamento do mês | Cliente: Avaliar, se ainda não avaliou |
| Pagamento contestado | Cliente | "Contestação aberta" | Aviso com o motivo | Suporte analisa |

Regras que aparecem na interface:

- O orçamento vale 48 h ou até 1 h antes do horário proposto, o que vier primeiro (`Badge` warning "Válido até qui 29/10, 13:00").
- O endereço completo do cliente só vai para o prestador quando o orçamento é aceito.
- A confirmação de pagamento traz o valor do orçamento já preenchido; se o prestador mudar o valor, "Motivo da diferença" vira obrigatório (ex.: material extra).
- Meios de pagamento: Pix, Dinheiro, Cartão de débito, Cartão de crédito, Transferência, Outro.
- A avaliação fica disponível de Concluído até 30 dias depois, só para quem contratou pelo app. Uma avaliação por pedido; pode ser editada em até 7 dias. O prestador pode responder uma vez.
- Com menos de 3 avaliações, busca e perfil mostram o `Badge` "Novo no Vitrio" no lugar da nota.

## Inventário de telas

### Cliente

| Tela | Situação | Componentes principais | Estados que precisa ter |
| --- | --- | --- | --- |
| Boas-vindas | prototipada | `Button` accent + on-hero, logo | — |
| Entrar | a desenhar | `TextField` de e-mail e de senha (com "Mostrar senha"), `Button` primary "Entrar", links "Esqueci a senha" e "Criar conta" | carregando, e-mail ou senha incorretos, conta bloqueada por 15 min, e-mail não confirmado (leva ao código) |
| Criar conta | a desenhar | `TextField` de nome, e-mail e senha, aceite de termos e política, `ActionBar` "Criar conta" | carregando, senha fraca (explica a regra), sem conexão |
| Confirmar e-mail | a desenhar | Campo de 6 dígitos com colar automático, "Reenviar código" com contagem de 60 s, "Trocar e-mail" | código errado, código vencido, muitos códigos (espera), e-mail fora do ar |
| Esqueci a senha | a desenhar | `TextField` de e-mail, `ActionBar` "Enviar código"; depois, código e senha nova na mesma tela | enviado (mesma mensagem exista o e-mail ou não), código errado, senha fraca |
| Aparelhos conectados (Conta) | a desenhar | Lista de sessões com aparelho e último acesso, "Desconectar", "Sair de todos os aparelhos" | só este aparelho |
| Permitir localização | a desenhar | ilustração do símbolo, `Button` primary + secondary | — |
| Escolher cidade (folha) | a desenhar | `SearchField`, lista de cidades, "Usar minha localização" | sem resultado, carregando |
| Início | prototipada | pílula de localização, `SearchField`, `HeroCard`, `CategoryTile`, cards resumidos, `BottomNav` | sem prestadores na cidade, sem GPS |
| Categorias (folha) | a desenhar | `CategoryTile` em grade de 4 colunas com todas as categorias; abre a Busca filtrada | carregando, erro |
| Busca e resultados | prototipada | `TopBar` com busca, `Chip`, `ProviderCard`, `BottomNav` | carregando (skeleton), vazio, erro, cidade sem prestadores |
| Filtros (folha) | a desenhar | distância máxima (só com GPS), nota mínima (Qualquer, 3+, 4+, 4,5+), MEI verificado, atende hoje, preço máximo em faixas (Qualquer, Até R$ 50, 100, 200, 500), cidade | — |
| Favoritos | a desenhar | `TopBar`, `ProviderCard`; aberta pela linha "Favoritos" da Conta | carregando, vazio ("Você ainda não salvou nenhum profissional." + "Buscar profissionais"), erro |
| Perfil do prestador | prototipada | capa `hero`, `Avatar`, `Badge`, métricas, serviços e preços, fotos, `ReviewCard`, `ActionBar` | perfil sem fotos, sem avaliações ("Novo no Vitrio") |
| Galeria de trabalhos | a desenhar | grade 3×, visualizador em tela cheia | — |
| Todas as avaliações | a desenhar | resumo de notas, filtros por nota, `ReviewCard` | vazio |
| Pedir orçamento | prototipada | `Chip`, `TextArea`, fotos, `SegmentedControl`, períodos, `ActionBar` | erro de envio, sem endereço salvo |
| Pedidos | a desenhar | abas Em andamento / Finalizados, cards de pedido com `Badge` de status | vazio |
| Detalhe do pedido | prototipada (até Agendado) | `StatusTimeline`, `QuoteCard`, `PaymentReceipt` | todos os status da tabela acima |
| Avaliar serviço | prototipada | `RatingInput`, `Chip` soft, `TextArea`, fotos, `Switch`, `ActionBar` | editar avaliação existente |
| Conta | a desenhar | dados, endereços, notificações, privacidade, excluir conta, trocar de modo | — |
| Notificações | a desenhar | lista agrupada por dia | vazio |
| Denunciar (folha) | a desenhar | motivos em lista, `TextArea` opcional | enviado |

### Prestador

| Tela | Situação | Componentes principais | Estados que precisa ter |
| --- | --- | --- | --- |
| Onboarding 1 · Sobre você | a desenhar | foto opcional, nome de exibição, profissão, bio | — |
| Onboarding 2 · Monte sua vitrine | prototipada | `Chip` (categorias, até 3), cidade base, raio, bairros, WhatsApp, verificação MEI | CNPJ inválido, CNPJ inativo, não optante pelo MEI |
| Onboarding 3 · Fotos e preços | a desenhar | upload de fotos, lista de serviços com "a partir de" ou "Sob orçamento" | — |
| Onboarding 4 · Revisar e publicar | a desenhar | prévia do `ProviderCard` e do perfil, `ActionBar` "Publicar" | campos obrigatórios faltando |
| Painel | prototipada | cabeçalho `hero` com `Switch` de disponibilidade e métricas, progresso do perfil, cards de pedido | sem pedidos |
| Pedidos | a desenhar | abas Novos / Em andamento / Finalizados | vazio por aba |
| Responder pedido | prototipada | resumo do pedido, `SegmentedControl`, `TextField` de valor, dia e horário, `Switch` de material, `TextArea`, `ActionBar` | pedido expirado |
| Agenda | a desenhar | lista por dia com horário, cliente, bairro e status | dia livre |
| Confirmar pagamento | a desenhar | `TextField` de valor (prefixo R$), `PaymentMethodPicker`, data, motivo da diferença, `ActionBar` "Confirmar recebimento" | valor diferente do orçamento |
| Minhas avaliações | a desenhar | resumo, `ReviewCard` com "Responder" | vazio |
| Editar perfil | a desenhar | mesmas seções do onboarding | — |

## Notificações

O push que aparece na tela bloqueada é genérico (sem nome, valor ou endereço); o texto completo fica na central de notificações, dentro do app autenticado.

| Evento | Para | Push (tela bloqueada) | Central do app |
| --- | --- | --- | --- |
| Novo pedido | Prestador | Você recebeu um novo pedido | Novo pedido de Mariana: Chuveiro desarmando o disjuntor |
| Orçamento recebido | Cliente | Você recebeu um orçamento | Carlos enviou um orçamento de R$ 180,00 |
| Orçamento aceito | Prestador | Seu orçamento foi aceito | Mariana aceitou seu orçamento para amanhã, 14:00 |
| Lembrete | Ambos | Você tem um serviço em 1 hora | Seu serviço com Carlos é em 1 hora |
| Serviço concluído | O outro lado | Um serviço foi marcado como concluído | Carlos marcou o serviço como concluído |
| Confirme o pagamento | Prestador | Confirme o pagamento de um serviço | Confirme o pagamento do serviço de Mariana |
| Pagamento confirmado | Cliente | Um pagamento foi confirmado | Carlos confirmou o pagamento de R$ 180,00 via Pix |
| Convite para avaliar | Cliente | Como foi o serviço? | Como foi o serviço de Carlos? Sua avaliação ajuda outros vizinhos |
| Nova avaliação | Prestador | Você recebeu uma avaliação | Beatriz avaliou seu serviço com 5 estrelas |
| Resposta à avaliação | Cliente | Responderam sua avaliação | Carlos respondeu à sua avaliação |

## Textos de referência

| Onde | Texto |
| --- | --- |
| Pré-permissão de localização | Para mostrar quem atende perto de você, o Vitrio usa sua localização só enquanto o app está aberto. |
| Sem GPS, na Início | Mostrando profissionais de Jaboticabal, SP. Trocar cidade |
| Cidade sem prestadores (Início e Busca) | Ainda não tem profissionais do Vitrio em Jaboticabal, SP. Conhece alguém que faz um bom trabalho? Mande o Vitrio para essa pessoa. |
| Endereço no pedido | O endereço completo só é enviado ao profissional depois que você aceitar o orçamento. |
| Pedido enviado | Pedir orçamento é grátis e sem compromisso. |
| Pagamento (cliente) | O pagamento é combinado direto com o profissional. O Vitrio não cobra taxa de você. |
| Confirmar pagamento | O cliente verá esse registro no pedido. Confira valor e forma antes de confirmar. |
| Contestar | Não reconheço este pagamento |
| Avaliação pública | Sua avaliação aparece no perfil com seu primeiro nome. |
| Selo MEI | CNPJ ativo e optante pelo MEI. O selo já aparece no seu perfil. |
