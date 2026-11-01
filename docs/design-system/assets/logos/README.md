# Logos

A marca é a **Janela em V**: um quadrado arredondado `lime` (cantos de 24% do lado) com um V em traço `plum`, pontas e junção arredondadas. O mesmo símbolo funciona sobre fundos claros e sobre `hero`, então só a cor do nome muda entre as versões.

## Símbolo e assinatura

- `vitrio-mark.svg` — só o símbolo, para fundos claros (`surface`, `surface-raised`). Tamanho mínimo 24px.
- `vitrio-mark-on-dark.svg` — só o símbolo, para `hero` e fundos escuros (o desenho é o mesmo do claro; o arquivo existe para o código escolher pelo fundo).
- `vitrio-mark-mono.svg` — só o símbolo em tinta única `ink` (#17121F), com o V vazado.
- `vitrio-wordmark.svg` — assinatura: símbolo + "Vitrio" com o nome em `plum`. Para fundos claros. Proporção 184×64; largura mínima 96px.
- `vitrio-wordmark-on-dark.svg` — assinatura com o nome em branco, para `hero` e fundos escuros.
- `vitrio-wordmark-mono.svg` — assinatura em tinta única `ink`, V vazado. Impressão em uma cor, carimbos, documentos.

O nome é desenhado em Bricolage Grotesque 800 convertida em curvas: não redigite o logo com a fonte.

## Ícone do app

- `vitrio-app-icon.svg` — ícone quadrado (1024×1024, fundo `lime` de sangria total e o V `plum`). Lojas e iOS; a loja aplica os cantos.
- `vitrio-app-icon-adaptive.svg` — camada de frente do ícone adaptativo do Android (1024×1024, só o V `plum`, mais grosso, dentro da zona segura). O fundo da camada de trás é `lime`.
- `vitrio-app-icon-monochrome.svg` — camada monocromática dos ícones com tema do Android 13+: o mesmo V em preto; o sistema pinta.
- `vitrio-app-icon-1024.png`, `-512.png`, `-192.png`, `vitrio-app-icon-adaptive-1024.png` e `vitrio-app-icon-monochrome-1024.png` — versões PNG geradas dos SVGs.

## E-mail e loja

- `vitrio-wordmark-on-dark@2x.png` — assinatura escura em PNG (368×128, fundo transparente), só para e-mails, porque o Gmail não exibe SVG. Exibida a 92×32 sobre `hero`.
- `../store/google-play-icon-512.png` — ícone da Google Play (512×512).
- `../store/google-play-feature.svg` e `google-play-feature-1024x500.png` — imagem de destaque da Google Play: fundo `hero`, assinatura escura, "Os profissionais da sua cidade, numa vitrine só." e os cards de profissionais da boas-vindas.

Nenhum PNG é editado à mão: mude o SVG e rode o script de `infra/brand/` (ver `infra/README.md`), que gera os PNGs e copia os SVGs para o app e para a página de compartilhar da API.
