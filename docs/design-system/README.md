Vitrio é a vitrine dos profissionais autônomos e MEIs de qualquer cidade do Brasil: o cliente encontra quem está perto (pelo GPS ou escolhendo a cidade), pede orçamento, contrata e avalia; o prestador monta seu perfil, responde pedidos, confirma o pagamento combinado e constrói reputação. O app é nativo em Flutter. Este sistema é a fonte da identidade, dos tokens e dos padrões de tela.

## Essência

A marca é uma vitrine de rua: vidro limpo, profissionais à mostra, nada escondido. Quatro princípios decidem qualquer dúvida de design:

- **Confiança visível.** Nota, número de avaliações, selo MEI verificado e distância aparecem sempre junto do nome. Nunca esconda reputação atrás de um toque.
- **Perto de você.** Distância, bairro e cidade são informação de primeira linha. Ordene por proximidade quando o GPS estiver ativo; mostre a cidade escolhida quando não estiver.
- **Uma ação por tela.** Cada tela tem uma única ação principal em `plum`. O resto é secundário (`line`) ou link (`link`).
- **O profissional é protagonista.** Prestadores são pessoas com ofício, não produtos. Use o nome, o rosto (ou iniciais) e as palavras deles.

## Voz e tom

Fale como um vizinho prestativo: direto, gentil, sem jargão.

- Trate por **você**. Frases curtas. Sentence case: "Pedir orçamento", nunca "Pedir Orçamento".
- Botões começam com verbo no infinitivo: "Enviar pedido", "Aceitar orçamento", "Confirmar pagamento", "Publicar avaliação".
- Números no padrão brasileiro: `R$ 180,00`, `4,9`, `1,2 km`, `14:00–15:00`, `qui 29/10`.
- Sem emoji na interface, sem ponto de exclamação, sem urgência fabricada ("Corra!", "Últimas vagas!") e sem superlativos ("o melhor eletricista").
- Explique regras que protegem as pessoas em uma linha, no momento em que importam: "O endereço completo só é enviado ao profissional depois que você aceitar o orçamento."
- Em erros, diga o que aconteceu e o que fazer: "Não conseguimos enviar seu pedido. Verifique a conexão e tente de novo."

| Faça | Evite |
| --- | --- |
| Pedir orçamento é grátis e sem compromisso. | Peça já seu orçamento GRÁTIS!!! |
| Carlos confirmou o pagamento: R$ 180,00 via Pix. | Pagamento processado com sucesso. |
| 28 eletricistas perto de você | Encontramos incríveis 28 profissionais |
| Você ainda não fez nenhum pedido. Que tal começar pela busca? | Nenhum registro encontrado. |

## Cores

Roxo jabuticaba (`plum`) é a marca; verde-limão (`lime`) é o acento. O resto é neutro e funcional.

- Fundo de tela em `surface`; cards, campos e barras fixas em `surface-raised`; caixas dentro de cards em `surface-inset`.
- Ação principal: preenchimento `plum` com texto `on-plum` e ícone de seta em `plum-accent`. Uma por tela.
- Seleção forte (chip de filtro ativo): preenchimento `plum` com check em `plum-accent`. Seleção suave (bairros, tags de avaliação, forma de pagamento): `plum-soft` com borda 1.5px `plum` e texto `plum`.
- `lime` só como preenchimento com texto `on-lime`: botão de destaque sobre `hero`, selo "Novo", etapa atual da linha do tempo, números de destaque sobre `hero`. Nunca use `lime` como texto sobre fundo claro.
- Superfícies de marca (boas-vindas, cabeçalho do painel do prestador, card "Disponíveis agora", capa do perfil) usam `hero` com texto `on-hero` e `on-hero-muted`; blocos translúcidos sobre ele em `hero-glass`.
- Texto: `ink` para títulos e nomes, `ink-2` para texto corrido, `ink-muted` para metadados, `placeholder` dentro de campos vazios.
- Status sempre com palavra ou ícone além da cor: `green`/`green-soft` para verificado, concluído e pagamento confirmado; `amber-ink`/`amber-soft` para prazos e novidades; `danger`/`danger-soft` para erros, cancelamento e contestação; `info`/`info-soft` para avisos neutros.
- Estrelas: preenchimento `amber` com contorno 1.2px `amber-edge`, sempre ao lado do número da nota.
- Tons de avatar (fundo/texto): `plum-soft`/`plum`, `green-soft`/`green`, `amber-soft`/`amber-ink`, `info-soft`/`info`. Escolha o tom de forma determinística pelo id da pessoa, nunca aleatória a cada render.
- Ponto de notificação: 10px em `alert` com anel de 2px da superfície por trás.

O tema escuro existe para quem usa o sistema escuro do celular. Cada token já tem os dois valores; não crie cores fora dos tokens.

## Tipografia

Duas famílias, ambas OFL e incluídas como arquivos variáveis em `fonts/`:

- **Bricolage Grotesque** (`display`) para títulos e números: `display-xl`, `display`, `title`, `section`, `price-lg`, `price`.
- **Plus Jakarta Sans** (`sans`) para todo o resto: `label-lg`, `body-strong`, `label`, `body`, `meta`, `caption`, `micro`.

Regras:

- Um `display-xl` no máximo por tela, e só em telas de marca.
- Valores em reais e notas sempre em `display` (`price-lg`, `price`): números são a parte mais lida do app.
- Pesos: 800 para rótulos e botões, 700 para títulos, 600 para metadados, 400 para texto corrido.
- Nada menor que `micro` (11px). Dicas e contadores usam `caption` com peso 600 em `ink-muted`.

## Espaço e layout

- Base de 2/4px. Margem lateral das telas `sp-20`. Entre seções `sp-24`. Padding de cards `sp-16`; cards de decisão `sp-20`. Entre itens de lista `sp-12`.
- Desenhe para 390px de largura e teste em 360px. Uma coluna. Grades só para categorias (4 colunas), métricas (3) e fotos (3).
- Alvo de toque mínimo `size-tap` (44px), inclusive em botões só de ícone.
- Estrutura de tela: barra superior (44px, voltar à esquerda, título centralizado, ação opcional à direita), conteúdo rolável, e uma de duas bases fixas: navegação inferior (telas de topo) ou barra de ação com o botão principal de largura total (telas de tarefa). Nunca as duas juntas.

## Forma e profundidade

- Raios: `radius-card` para cards de conteúdo, `radius-hero` para cards de decisão, `radius-xl` para o botão principal e a busca, `radius-lg` para campos, `radius-md` para botões médios e caixas internas, `radius-full` para chips, badges e switch.
- Avatares de **prestador** são quadrados arredondados (raio ≈ 31% do lado); avatares de **cliente** são círculos. Assim o olho distingue quem oferece de quem contrata.
- Bordas: cards em repouso `line-soft` 1px; card selecionado ou de decisão 1.5px `plum`; pagamento confirmado 1.5px `green`; campos `line-strong` 1px, `plum` ao focar.
- Sombras: o app é plano. `shadow-sm` em cards de lista, `shadow-md` só no card que pede decisão, `shadow-lg` só em folhas inferiores e na tela de boas-vindas.
- Foco: `focus-ring` em todo controle focável sobre superfícies claras; `focus-ring-on-hero` sobre `hero`.

## Estados

- **Pressionado:** `plum-deep` e escala 0.98 em 150ms.
- **Desabilitado:** 40% de opacidade, sem sombra, cursor padrão. Prefira explicar o motivo em uma linha a deixar um botão desabilitado sem contexto.
- **Carregando:** blocos de skeleton em `track` com o mesmo raio do conteúdo final; nunca spinners em tela cheia para listas.
- **Vazio:** uma frase que explica, uma ação que resolve ("Você ainda não fez nenhum pedido." + "Buscar profissionais").
- **Erro:** mensagem em `danger` junto do campo, com ícone; erros de rede em faixa `danger-soft` no topo da área afetada com "Tentar de novo".
- **Sem localização:** quando o GPS estiver desligado ou negado, mostre a cidade escolhida no seletor de localização e ordene por relevância; ofereça "Usar minha localização" sem bloquear a busca.

## Movimento

Curto e funcional: 150–220ms, `ease-out`, sem quique. Use para troca de estado (chip, switch, segmentado), entrada de folhas inferiores e avanço da linha do tempo do pedido. Respeite "reduzir movimento" do sistema.

## Iconografia

- Conjunto próprio em `assets/Icons/`: grade de 24px, traço 2px (2.2–2.4 em tamanhos de 14–18px), pontas e junções arredondadas, sem preenchimento. A única exceção é a estrela de nota, preenchida.
- Ícones herdam a cor do texto ao lado. Ícone de categoria vai numa caixa de 40px (`size-icon-box`) em `plum-soft` com o ícone em `plum` e raio `radius-md`.
- Sem emoji e sem logos de terceiros. WhatsApp usa o ícone `message` com a palavra "WhatsApp"; Pix usa `qr` com a palavra "Pix".
- Botões só de ícone sempre têm rótulo acessível ("Voltar", "Notificações", "Salvar nos favoritos").

## Imagens

- As fotos são dos trabalhos reais enviados pelos prestadores: grade de 3 colunas, proporção 1:1, raio `radius-xl`, legenda curta opcional.
- Sem banco de imagens com pessoas sorrindo e sem ilustrações genéricas. Na falta de foto, use um bloco de cor da paleta com o ícone `image` a 45% de opacidade.
- Foto de perfil é opcional; sem ela, iniciais sobre um tom de avatar.

## Logo

A marca é uma vitrine: uma janela com três vidros, um alto à esquerda e dois à direita.

- `vitrio-wordmark.svg` (vidros `lime`, moldura e nome em `plum`) sobre fundos claros; `vitrio-wordmark-on-dark.svg` (moldura `lime`, nome branco) sobre `hero` e fundos escuros; `vitrio-wordmark-mono.svg` em `ink` para impressão em uma cor.
- Só o símbolo (`vitrio-mark*.svg`) quando o espaço for menor que 96px de largura; tamanho mínimo do símbolo: 24px.
- Ícone do app: `vitrio-app-icon.svg` (iOS e lojas) e `vitrio-app-icon-adaptive.svg` (camada de frente do ícone adaptativo do Android, já dentro da zona segura).
- Área de respiro: a largura de um vidro estreito (¼ da altura do símbolo) em volta de tudo.
- Não recolora fora das três versões, não aplique sombra, não gire, não coloque sobre fotos sem uma camada de `hero` por trás.

## Acessibilidade

- Texto com contraste mínimo 4.5:1 sobre o fundo indicado em cada token, nos dois temas; bordas de campo, anéis de foco e ícones com significado, 3:1.
- Tudo que é tocável é botão ou link de verdade, com alvo de 44px.
- Status nunca só por cor: "MEI verificado" tem escudo, "Concluído" tem check, "Recusado" tem a palavra.
- Estrelas sempre acompanhadas da nota em número; o seletor de nota anuncia "1 estrela" … "5 estrelas" e mostra a palavra (Péssimo, Ruim, Regular, Bom, Excelente).
- Campos sempre com rótulo visível ou associado; contadores e dicas ligados ao campo.

## Como usar este sistema

- **App (Flutter):** siga a seção "Implementação em Flutter": tokens viram `VitrioColors`, `VitrioText`, `VitrioSpace` e `VitrioRadius`, e cada componente abaixo tem um widget correspondente.
- **Web e protótipos:** os componentes estão em `components/bundle.js` como `window.Vitrio` (React 18) e usam as classes de `components/bundle.css` sobre as variáveis de `tokens.css`.
- **Telas e fluxos:** a seção "Plano de telas e fluxos" lista cada tela, seus estados e as regras de negócio que aparecem na interface.
