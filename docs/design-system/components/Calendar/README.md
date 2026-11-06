# Calendar

Calendário do mês da Agenda do prestador, com marcas por dia e o dia escolhido.

- Forneça `month` (ano e mês), `selected` (dia escolhido), `today`, `marks` por dia (`scheduled`, `quoted`, `done`) e `onSelect(dia)`; `onPrevious` e `onNext` trocam o mês.
- Fica num card `surface-raised` com raio `radius-card` e borda `line-soft`. Cabeçalho com o mês em `section` entre dois `IconButton` surface ("Mês anterior", "Próximo mês"); linha dos dias da semana em `micro` `ink-muted`; células com 44px de alvo e o número em `label`.
- Marcas de 7px abaixo do número: cheia em `plum` (agendado), vazada com borda 1.5px `plum` (aguardando o cliente) e cheia em `line-strong` (feito). A legenda, com as palavras, fica abaixo da grade.
- Dia escolhido: círculo `plum` com o número em `on-plum`. Hoje: anel 1.5px `plum` com o número em `plum`.
- Cada dia anuncia o dia da semana, a data e quantos itens tem ("Qui, 29 de outubro, 2 itens"); o escolhido é anunciado como selecionado.
- Abaixo do card, a lista do dia escolhido, separada em "Agendados", "Aguardando o cliente" e "Feitos".
- Não: cor por categoria nas marcas, mais de três tipos de marca, número sem marca quando o dia tem item.
