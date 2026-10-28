# Visão e funcionalidades

## Problema

**Para quem contrata.** Achar um eletricista, uma diarista ou um pedreiro de confiança ainda depende de indicação de conhecido ou de grupo de WhatsApp. A pessoa não sabe quem atende perto, quanto custa, se o profissional é bom nem se vai aparecer.

**Para quem presta serviço.** Autônomos e MEIs vivem de boca a boca. Não têm uma vitrine própria, não acumulam reputação que possam mostrar e perdem clientes por não serem encontrados. Muitos trabalham na informalidade e não têm como provar que são confiáveis.

## Proposta de valor

- **Cliente:** encontrar quem atende perto, ver nota, preço de referência e trabalhos anteriores, pedir orçamento sem compromisso, contratar e avaliar, tudo num lugar só.
- **Prestador:** uma vitrine grátis com fotos, preços e avaliações reais, pedidos chegando pelo app, agenda e histórico de serviços e recebimentos.
- **Os dois:** regras claras que protegem as duas partes (endereço só depois do aceite, avaliação só de quem contratou, pagamento registrado pelo prestador e visível ao cliente).

## Públicos

| Persona | Contexto | O que precisa |
| --- | --- | --- |
| Mariana, cliente | Mora no Centro, usa o celular para tudo, precisa de um eletricista para amanhã | Achar alguém perto e bem avaliado, saber o preço antes, não passar endereço para estranho |
| Carlos, eletricista MEI | 8 anos de profissão, atende num raio de 8 km, divulga só pelo WhatsApp | Receber pedidos qualificados, mostrar trabalhos, ter o selo de MEI, controlar agenda e recebimentos |
| Dona Cida, diarista autônoma | Sem CNPJ, pouca familiaridade com apps | Cadastro simples, perfil que inspire confiança mesmo sem selo, avisos claros de novos pedidos |
| Equipe Vitrio (moderação) | Poucas pessoas no início | Ver denúncias, ocultar conteúdo, suspender perfis, cuidar das categorias |

## Princípios de produto

1. **Confiança visível:** nota, número de avaliações, selo e distância sempre junto do nome.
2. **Qualquer cidade, perto de você:** funciona com GPS (mais próximos primeiro) e sem GPS (cidade escolhida).
3. **Uma ação por tela:** o próximo passo é sempre óbvio.
4. **Sem atravessar o profissional:** o Vitrio não cobra do cliente nem fica com parte do pagamento no MVP; o dinheiro é combinado entre as partes.
5. **Privacidade por padrão:** endereço do cliente só após aceite; endereço do prestador nunca é público.

## Funcionalidades

Legenda: **MVP** entra na primeira versão publicada; **Depois** fica no backlog.

**Plataformas:** o primeiro lançamento é só **Android** (Google Play). O app iOS vem depois, com o mesmo código Flutter. Como o cadastro e o login são próprios, sem login social, a App Store não exige login com Apple.

### 1. Conta e acesso

| Funcionalidade | Descrição | Quando |
| --- | --- | --- |
| Criar conta | Cadastro próprio com nome, e-mail e senha; o e-mail é confirmado com um código de 6 dígitos antes do primeiro acesso | MVP |
| Entrar | E-mail e senha; erro genérico que não revela se o e-mail tem conta | MVP |
| Continuar conectado | O app renova a sessão sozinho; só pede login de novo depois de 60 dias sem uso, 1 ano desde o login ou se a sessão for encerrada | MVP |
| Esqueci a senha | Código por e-mail e senha nova; desconecta todos os aparelhos | MVP |
| Trocar senha | Em Conta, com a senha atual; desconecta os outros aparelhos | MVP |
| Aparelhos conectados | Lista de sessões em Conta, com "Desconectar" e "Sair de todos os aparelhos" | MVP |
| Navegar sem conta | Busca, perfis e avaliações abertos; login só para pedir orçamento, favoritar, avaliar ou anunciar | MVP |
| Dados básicos | Nome de exibição e celular (obrigatório antes do primeiro pedido); cliente aparece com iniciais, sem foto | MVP |
| Endereços salvos | Casa, trabalho etc., cadastrados por CEP com número e complemento | MVP |
| Modo prestador | "Quero oferecer meus serviços" ativa o modo prestador na mesma conta; troca de modo em Conta | MVP |
| Preferências de notificação | Liga/desliga por tipo de aviso | MVP |
| Excluir conta | Dentro do app (exigência das lojas); avaliações feitas viram "Usuário removido" | MVP |
| Exportar meus dados | Pedido de cópia dos dados (LGPD) | Depois |
| Verificar telefone por código | OTP por SMS ou WhatsApp para dar selo "Telefone verificado" | Depois |

### 2. Localização e cidades

| Funcionalidade | Descrição | Quando |
| --- | --- | --- |
| Pré-permissão de localização | Tela que explica o uso antes do pedido do sistema, com "Escolher cidade" como alternativa de mesmo peso | MVP |
| Detectar cidade pelo GPS | O app manda o ponto; a API devolve a cidade (código IBGE) e o bairro aproximado | MVP |
| Escolher cidade | Busca entre todas as cidades do Brasil (acentos opcionais), com UF, e cidades recentes | MVP |
| Trocar o centro da busca | Procurar em outra cidade sem desligar o GPS; voltar para "Perto de mim" com um toque | MVP |
| Funcionar sem GPS | Ordena por relevância dentro da cidade escolhida, sem mostrar distância | MVP |
| Cidades com poucos prestadores | Mensagem honesta + convite para anunciar e para mandar o Vitrio a um profissional conhecido (compartilhamento do sistema, sem programa de indicação) | MVP |

### 3. Descoberta e busca

| Funcionalidade | Descrição | Quando |
| --- | --- | --- |
| Busca por texto | Profissão, serviço ou nome; tolerante a acento e pequenos erros | MVP |
| Categorias | Grade de categorias na Início; lista completa em "Mais" | MVP |
| Filtros | Distância máxima, nota mínima, MEI verificado, atende hoje, faixa de preço, cidade | MVP |
| Ordenação | Relevância (padrão), mais perto, melhor avaliado, menor preço | MVP |
| "Disponíveis agora" | Atalho para prestadores com "Disponível hoje" ligado | MVP |
| Bem avaliados perto de você | Carrossel na Início | MVP |
| Favoritos | Salvar prestadores | MVP |
| Compartilhar perfil | Link `https://vitrio.app/p/{id}` que abre o app (Android App Links) ou, sem o app, uma página simples com o perfil resumido e o botão da Google Play | MVP |
| Mapa de resultados | Alternativa à lista | Depois |
| Buscas recentes | Atalho para repetir uma busca | Depois |

### 4. Vitrine do prestador (perfil público)

| Funcionalidade | Descrição | Quando |
| --- | --- | --- |
| Cabeçalho | Nome, profissão, anos de experiência, selos (MEI verificado, Atende hoje, tempo de resposta), calculados conforme [02-regras-de-negocio.md §3](02-regras-de-negocio.md) | MVP |
| Métricas | Nota e número de avaliações, serviços feitos pelo app (a partir de 5), distância | MVP |
| Sobre | Texto livre do prestador | MVP |
| Onde e quando atende | Cidade base, raio, bairros, dias e horários, atende emergência | MVP |
| Serviços e preços | Lista com "a partir de" ou "Sob orçamento" | MVP |
| Fotos de trabalhos | Grade com galeria em tela cheia | MVP |
| Avaliações | Resumo com distribuição de notas, tags mais citadas, lista com respostas | MVP |
| Contato por WhatsApp | Botão que abre o WhatsApp com mensagem pronta, se o prestador ativar | MVP |
| Pedir orçamento | Ação principal do perfil | MVP |
| Denunciar perfil | Motivo + detalhes | MVP |

### 5. Onboarding e gestão do prestador

| Funcionalidade | Descrição | Quando |
| --- | --- | --- |
| Onboarding em 4 passos | Sobre você → Monte sua vitrine → Fotos e preços → Revisar e publicar | MVP |
| Categorias | Até 3 categorias por prestador | MVP |
| Área de atendimento | Endereço base (privado, por CEP), raio em km, bairros atendidos | MVP |
| Serviços e preços | Até 20 serviços, cada um com preço "a partir de" ou "Sob orçamento" | MVP |
| Fotos | Até 30 fotos com legenda curta, reordenáveis | MVP |
| Disponibilidade | Dias e faixas de horário; "Atende emergências" | MVP |
| Disponível hoje | Liga/desliga no Painel; aparece em "Disponíveis agora" | MVP |
| Verificação MEI | Consulta de CNPJ (ativo e optante pelo MEI) para o selo | MVP |
| Publicar e pausar | Perfil só aparece publicado e com os campos mínimos; pausar esconde da busca | MVP |
| Completude do perfil | Barra de progresso com a próxima ação sugerida | MVP |
| Painel | Pedidos novos, nota, serviços no mês, última avaliação | MVP |
| Agenda | Serviços agendados por dia | MVP |
| Resumo de recebimentos | Total confirmado no mês e por meio de pagamento | MVP |
| Métricas de desempenho | Visualizações, taxa de resposta, conversão | Depois |

### 6. Pedidos e orçamentos

| Funcionalidade | Descrição | Quando |
| --- | --- | --- |
| Pedir orçamento | Serviço, descrição, até 5 fotos, dia (hoje, amanhã, data) e período, endereço salvo | MVP |
| Responder com orçamento | Valor fechado ou visita técnica, data e horário, material incluso, mensagem | MVP |
| Recusar pedido | Com motivo opcional; o cliente recebe sugestões de outros prestadores | MVP |
| Validade | Pedido sem resposta expira em 48 h; orçamento vale 48 h | MVP |
| Aceitar ou recusar orçamento | Ao aceitar, o pedido fica agendado e o endereço completo é liberado | MVP |
| Cancelar | Qualquer lado, antes de concluir, com motivo | MVP |
| Concluir serviço | Quem marcar primeiro; o outro lado é avisado | MVP |
| Lista de pedidos | Cliente: Em andamento / Finalizados. Prestador: Novos / Em andamento / Finalizados | MVP |
| Lembretes | 1 h antes do horário agendado, para os dois (na véspera às 21h se esse horário cair no silêncio noturno) | MVP |
| Remarcar com proposta de novo horário | Um lado propõe, o outro aceita | Depois |
| Chat dentro do app | Mensagens entre cliente e prestador por pedido | Depois |

### 7. Pagamento confirmado pelo prestador

| Funcionalidade | Descrição | Quando |
| --- | --- | --- |
| Combinar fora do app | O app informa que o pagamento é combinado direto entre as partes | MVP |
| Confirmar recebimento | Só o prestador: valor recebido (pré-preenchido com o orçamento), meio (Pix, Dinheiro, Cartão de débito, Cartão de crédito, Transferência, Outro), data e observação | MVP |
| Motivo da diferença | Obrigatório quando o valor confirmado difere do orçamento | MVP |
| Registro visível ao cliente | Card "Pagamento confirmado" no pedido | MVP |
| Contestar | O cliente pode marcar "Não reconheço este pagamento" em até 7 dias; abre caso para o suporte | MVP |
| Pagamento dentro do app | Cobrança, split e repasse | Depois |

### 8. Avaliações

| Funcionalidade | Descrição | Quando |
| --- | --- | --- |
| Avaliar serviço | Nota 1–5, tags, comentário (até 400), até 3 fotos, "Recomendo" | MVP |
| Elegibilidade | Só o cliente do pedido, de Concluído até 30 dias depois; uma por pedido | MVP |
| Editar | Até 7 dias após publicar | MVP |
| Responder | O prestador responde uma vez, publicamente | MVP |
| Nota do prestador | Média bayesiana exibida com uma casa decimal; "Novo no Vitrio" com menos de 3 avaliações | MVP |
| Tags mais citadas | Contagem por tag no perfil | MVP |
| Denunciar avaliação | Pelo prestador ou por qualquer usuário | MVP |
| Cliente avaliado pelo prestador | Reputação do cliente | Depois |

### 9. Notificações

| Funcionalidade | Descrição | Quando |
| --- | --- | --- |
| Push | Novo pedido, orçamento recebido e aceito, lembrete, concluído, pagamento confirmado, convite para avaliar, nova avaliação, resposta | MVP |
| Central de notificações | Lista no app com lidas e não lidas | MVP |
| E-mails de conta | Código de confirmação, código de redefinição de senha, aviso de senha alterada e de tentativa de cadastro, no layout de [design-system/emails.md](design-system/emails.md) | MVP |
| Outros e-mails | Boas-vindas, resumo de pedidos e avisos que hoje vão por push | Depois |

### 10. Confiança, segurança e moderação

| Funcionalidade | Descrição | Quando |
| --- | --- | --- |
| Denúncias | Perfil, foto, avaliação ou pedido; motivos padronizados | MVP |
| Ocultação automática | Foto, avaliação ou resposta com 3 denúncias abertas de pessoas diferentes fica oculta até revisão; vitrine denunciada vai para o topo da fila e só sai do ar por decisão da moderação | MVP |
| Moderação | Fila de denúncias, ocultar, restaurar, suspender e reativar perfis | MVP (endpoints + ferramenta interna) |
| Limites de uso | Rate limit em login, pedidos, avaliações e denúncias | MVP |
| Painel administrativo web | Interface para moderação, categorias e métricas | Depois |
| Bloquear usuário | Prestador bloqueia cliente e vice-versa | Depois |

### 11. Monetização

Nada é cobrado no MVP. Caminhos previstos, sempre do lado do prestador e nunca pelo contato do cliente: destaque na busca identificado como "Patrocinado", plano Pro (mais fotos, métricas, link próprio), e no futuro pagamento pelo app com taxa transparente.

## Fora do MVP

App iOS, login social (Google, Apple), pagamento dentro do app, chat interno, remarcação com proposta, mapa, programa de indicação, painel administrativo web, avaliação de clientes, verificação de telefone, outros idiomas e versão web para clientes.

## Indicadores

- **Norte:** pedidos com pagamento confirmado por semana.
- **Funil do cliente:** busca → perfil → cadastro ou login → e-mail confirmado → pedido → orçamento recebido → aceito → concluído → avaliado. O abandono no cadastro é o sinal para rever o login social.
- **Saúde do prestador:** tempo até o primeiro orçamento, taxa de resposta em 48 h, prestadores com perfil completo.
- **Confiança:** taxa de contestação de pagamento, denúncias por mil pedidos.
