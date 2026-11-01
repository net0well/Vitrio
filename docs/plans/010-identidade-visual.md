# Plano: Nova identidade visual

**Status:** Em andamento
**Spec:** [010-identidade-visual](../specs/010-identidade-visual.md) (Approved)
**Arquitetura:** Clean Architecture com Controllers ([03-arquitetura.md](../03-arquitetura.md)); sem mudança de domínio, Application ou banco
**Camadas afetadas:**
- Infrastructure: o PNG do cabeçalho dos e-mails.
- Api: o HTML e a política de conteúdo da página de compartilhar (`/p/{id}`).
- App Flutter: ativos de marca, ícone, abertura, boas-vindas, Conta sem login, Entrar e Criar conta.
- Infra: script que gera os PNGs a partir dos SVGs, em `infra/brand/`.
- Docs e artefatos: logos e READMEs do design system, `telas.md`, o protótipo e o artefato do design system "Vitrio".

## Recortes

Todos na branch `feat/nova-identidade-visual`, em vários commits. O PR só abre quando todos estiverem prontos e testados de ponta a ponta.

| # | Recorte | Critérios |
| --- | --- | --- |
| 1 | SVGs-fonte do logo novo e script dos PNGs | AC-8, AC-10 |
| 2 | Protótipo: prancheta Main atualizada e pranchetas novas, aprovadas pelo usuário antes do código das telas | AC-9 |
| 3 | App: ícone (com a camada monocromática) e abertura | AC-1, AC-2 |
| 4 | App: boas-vindas, Conta sem login, Entrar e Criar conta | AC-3, AC-4, AC-5 |
| 5 | Backend: e-mails e página de compartilhar | AC-6, AC-7 |
| 6 | Design system: docs e artefato "Vitrio" | AC-8, AC-9 |
| 7 | Fechamento: revisões de back e de front, testes no emulador e PR | Todos |

## Decisões técnicas

| Tema | Decisão |
| --- | --- |
| Fonte da verdade | Os SVGs de `docs/design-system/assets/logos/`. O símbolo usa `viewBox` 100×100: quadrado `#C8F169` com raio 24 e o V `M29 30 L50 71 L71 30` em traço `#3E1F57` de 13, pontas e junção arredondadas. O app copia os SVGs para `mobile/assets/brand/` com os mesmos nomes. |
| Assinatura | Reaproveita as curvas das letras "Vitrio" do SVG atual (Bricolage Grotesque 800 já convertida) e troca só o símbolo, com o respiro de ¼ da altura do símbolo entre ele e o nome. Três versões: clara (nome `plum`), escura (nome branco) e mono (`ink`, V vazado). |
| Ícone adaptativo e monocromático | `vitrio-app-icon-adaptive.svg` traz só o V `plum` dentro da zona segura, sobre fundo `#C8F169`; `vitrio-app-icon-monochrome.svg` traz só o V, para o sistema pintar. `vitrio-app-icon.svg` (lojas) é o quadrado `lime` com o V. |
| PNGs | `infra/brand/render.mjs` (Node, `@resvg/resvg-js` com versão fixa em `infra/brand/package.json`) gera todos os PNGs: os do design system (192, 512 e 1024, adaptativo e monocromático), os de `mobile/assets/launcher/`, a assinatura @2x do e-mail, o símbolo da abertura antiga e os dois da Google Play. Rodar com `npm ci && node render.mjs`, documentado no README de `infra/`. PNG nenhum é editado à mão. |
| Ícone no Android | `flutter_launcher_icons`, que o projeto já usa, com `adaptive_icon_background: "#C8F169"`, a frente com o V e `adaptive_icon_monochrome`; ele gera `res/mipmap-*`, `res/drawable-*` e o `ic_launcher.xml` com `<monochrome>`. |
| Abertura | Android 12+: `values-v31/styles.xml` e `values-night-v31/styles.xml` com `android:windowSplashScreenBackground` em `hero` (claro `#3E1F57`, escuro `#2A1640`), mostrando o ícone adaptativo do app. Antes do 12: `launch_background.xml` (claro e escuro) com o fundo `hero` e o símbolo no centro. Sem pacote novo. |
| Boas-vindas | A linha de gratuidade usa `VitrioIcon(check)` em `lime` e o texto em `on-hero`, abaixo do subtítulo; o resto da prancheta não muda. |
| Conta sem login | Card composto com os tokens de superfície de marca (`hero`, raio `hero`, assinatura escura, `display` e `on-hero-muted`), porque o `HeroCard` tem ação própria e é um por tela; os botões ficam fora do card, como na prancheta. |
| Entrar e Criar conta | O `FormPage` já tem `header`, `heading` e `subtitle`: o símbolo de 56 px vai no `header`, e título e linha de apoio nos outros dois. Textos novos no ARB. |
| E-mails | `Email/Templates/vitrio-wordmark.png` recebe a assinatura escura nova (@2x), com o mesmo `cid`; o `layout.html` só ajusta largura e altura exibidas à proporção nova. |
| Página de compartilhar | A assinatura clara vai como SVG embutido no lugar do texto "Vitrio", nas duas páginas; o ícone da aba é `<link rel="icon" type="image/svg+xml" href="data:…">` com o símbolo. A política de conteúdo dessa página passa a aceitar `data:` em `img-src` (só imagens embutidas; nada externo nem script), porque hoje ela só libera o servidor das fotos. |
| Protótipo | No canvas do protótipo: a prancheta Main ganha a assinatura e a linha de gratuidade; entram Abertura, Conta (sem login), Entrar, Criar conta e Destaque da Google Play, com os componentes do design system. O código das telas só começa depois que o usuário aprova as pranchetas. |

## Recorte 1: SVGs-fonte e script dos PNGs

**Situação:** pronto. O nome reaproveita as curvas das letras do SVG antigo, 20 px depois do símbolo. O texto da imagem de destaque vira contorno no script com o `fontkit` (o `resvg` ignora o peso das fontes variáveis).

1. `docs/design-system/assets/logos/`: os seis SVGs de símbolo e assinatura, `vitrio-app-icon.svg`, `vitrio-app-icon-adaptive.svg` e `vitrio-app-icon-monochrome.svg` (novo); `docs/design-system/assets/store/google-play-feature.svg` (novo, 1024×500).
2. `infra/brand/package.json`, `package-lock.json` e `render.mjs`; README de `infra/` com o comando.
3. PNGs gerados e copiados: design system, `mobile/assets/launcher/`, e-mail e Google Play (`docs/design-system/assets/store/`).
4. Cópia dos SVGs para `mobile/assets/brand/`.

## Recorte 2: protótipo

1. Prancheta Main com a assinatura escura nova e a linha "O Vitrio é grátis para todo mundo.".
2. Pranchetas novas: Abertura, Conta (sem login), Entrar, Criar conta e Destaque da Google Play.
3. Aprovação do usuário, com a captura de cada prancheta, antes do recorte 4.

## Recorte 3: ícone e abertura

1. `pubspec.yaml`: configuração do `flutter_launcher_icons` (fundo, frente e monocromático); recursos gerados.
2. Estilos da abertura (claro e escuro, Android 12+ e anteriores) e o `launch_background.xml`.
3. Conferência no emulador: ícone no lançador, ícones com tema ligados e abertura nos dois temas.

## Recorte 4: telas

1. Boas-vindas com a linha de gratuidade.
2. Conta sem login com o card de marca.
3. Entrar e Criar conta com símbolo, título e linha de apoio.
4. Testes de widget das quatro telas (textos e presença do logo) e os fluxos no emulador.

## Recorte 5: backend

1. PNG do e-mail e o tamanho exibido no `layout.html`; teste do anexo da assinatura.
2. Página de compartilhar com a assinatura embutida, o ícone da aba e a política de conteúdo; testes das duas páginas.

## Recorte 6: design system

1. README dos logos e seção "Logo" do README descrevendo a Janela em V; `telas.md` com as telas prototipadas.
2. Artefato do design system "Vitrio": logos e README atualizados.

## Recorte 7: fechamento

1. Revisão de back: `/code-review`, `/security-scan` (atenção à política de conteúdo) e `/arch-check`; e-mails conferidos no Mailpit; a página de compartilhar aberta no navegador.
2. Revisão de front: cada tela no emulador, por captura, contra as pranchetas e o design system; código do app contra os padrões.
3. Plano como concluído e PR com o resultado das revisões e dos testes.

## Riscos

| Risco | Mitigação |
| --- | --- |
| O V perder leitura em 24 px ou nas máscaras do Android | Conferir os PNGs gerados em 24, 48 e 192 px e o ícone no lançador com máscaras diferentes |
| `@resvg/resvg-js` desenhar o nome diferente do navegador | As letras já são curvas, sem fonte; comparar o PNG com o SVG aberto no navegador |
| A abertura do Android 12+ cortar o ícone | Usar o ícone adaptativo, que já respeita a zona segura; conferir no emulador |
| `flutter_launcher_icons` sobrescrever ajustes manuais do `ic_launcher.xml` | Não editar recursos gerados à mão; toda mudança passa pela configuração no `pubspec.yaml` |
| Clientes de e-mail mostrarem o PNG borrado | Manter o @2x e o tamanho exibido proporcional |
