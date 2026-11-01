# Spec 010 — Nova identidade visual

- **Status:** Approved (2026-10-31)
- **Data:** 2026-10-31
- **Responsável:** Wellington

## Problem

O logo atual (a janela de três vidros) é pouco marcante, e as telas de entrada do app são simples demais: a Conta de quem não entrou tem só um ícone, uma frase e dois botões, e Entrar e Criar conta são só campos. A boas-vindas também não diz que o Vitrio é grátis, que é um dos motivos para a pessoa experimentar. Entre quatro propostas com a letra V ([canvas das opções](https://claude.ai/artifact/BG7QfAXCB7LusdwvzjqZ4N)), o usuário escolheu a **D, "Janela em V"**, em 2026-10-31.

## Scope

### In

- **Símbolo "Janela em V":** quadrado arredondado `lime` com o V em traço `plum` (pontas e junção arredondadas). No ícone adaptativo do Android, a forma do sistema em `lime` com o V dentro da zona segura.
- **Assinatura:** símbolo + "Vitrio" ao lado, em Bricolage Grotesque 800 convertida em curvas. Versões:
  - clara (nome em `plum`), para `surface` e `surface-raised`;
  - escura (nome branco), para `hero` e fundos escuros;
  - mono (tudo em `ink`, V vazado), para impressão em uma cor.
- **Onde o logo antigo sai e o novo entra**, com os mesmos nomes de arquivo:
  - app: símbolos e assinaturas em `assets/brand/`, ícone do app (comum, adaptativo e a camada monocromática dos ícones com tema do Android 13+) e a abertura do Android 12+ (fundo `hero` com o símbolo no centro);
  - e-mails: o PNG da assinatura no cabeçalho;
  - página web de compartilhar (`/p/{id}`, perfil e perfil indisponível): assinatura no lugar do nome em texto e o símbolo como ícone da aba;
  - design system: `docs/design-system/assets/logos/`, o README dos logos e a seção "Logo" do README, e os mesmos itens no artefato do design system "Vitrio";
  - Google Play: ícone de 512 px e imagem de destaque de 1024×500 (fundo `hero`, assinatura, "Os profissionais da sua cidade, numa vitrine só." e os cards de profissionais da boas-vindas).
- **Telas:**
  - Boas-vindas: mesmo layout da prancheta, com a assinatura nova e, logo abaixo do subtítulo, uma linha com o ícone de check em `lime` e "O Vitrio é grátis para todo mundo.";
  - Conta sem login: card `hero` no topo com a assinatura escura, "Peça orçamentos e ofereça seus serviços" e "O Vitrio é grátis para todo mundo."; abaixo, "Entrar" (`primary`) e "Criar conta" (`secondary`);
  - Entrar: no topo do formulário, o símbolo de 56 px, "Que bom te ver de novo" e "Entre para pedir orçamentos e acompanhar seus pedidos."; campos e barra de ação como hoje;
  - Criar conta: o símbolo de 56 px, "Crie sua conta" e "É grátis e leva menos de um minuto."; campos e barra de ação como hoje.
- **Protótipo:** a prancheta Main ganha o logo e a linha de gratuidade; entram pranchetas novas de Abertura, Conta (sem login), Entrar, Criar conta e Destaque da Google Play. Em `docs/design-system/telas.md`, essas telas passam a "prototipada".

### Out

- Abertura animada.
- Ícone pequeno das notificações (spec 009).
- Capturas de tela da Google Play (lançamento).
- Configuração do app para iOS (depois do MVP); o `vitrio-app-icon.svg` das lojas é atualizado mesmo assim.
- Qualquer outra tela além das quatro acima.

### Diferenças aprovadas do protótipo do app

Aprovadas pelo usuário em 2026-10-31: o logo novo em todas as pranchetas, a linha "O Vitrio é grátis para todo mundo." na boas-vindas e o desenho novo de Conta sem login, Entrar e Criar conta (que não tinham prancheta). As pranchetas são atualizadas antes do código, e as telas ficam iguais a elas.

## Domain Model

Sem mudança de domínio, banco ou regra de negócio. A spec troca ativos de marca e o visual de quatro telas.

## API Contract

Sem endpoint novo. Mudam só:

- o PNG do cabeçalho dos e-mails de conta (mesmo `cid`, mesmo tamanho exibido);
- o HTML de `GET /p/{id}`: a assinatura vai como SVG embutido no lugar do texto "Vitrio", e o ícone da aba como `<link rel="icon">` com o SVG do símbolo embutido, sem rota nova.

## Authorization

Nada muda.

## Edge Cases & Failure Modes

- Símbolo legível a partir de 24 px; abaixo disso, não usar. Assinatura com largura mínima de 96 px.
- Tema escuro do celular: o símbolo `lime` funciona sobre `hero` e fundos escuros; a abertura usa o `hero` do tema (claro `#3E1F57`, escuro `#2A1640`).
- Ícone adaptativo: o V dentro da zona segura (círculo central de 66%), legível em todas as máscaras (círculo, quadrado arredondado, gota).
- Ícones com tema ligados (Android 13+): a camada monocromática traz só o V, e o sistema pinta.
- O Gmail não mostra SVG: o e-mail continua com o PNG @2x, gerado a partir do SVG; com imagens bloqueadas, o texto alternativo é "Vitrio".
- Logo é exceção de contraste de texto (WCAG), mas o V em `plum` sobre o quadrado `lime` passa de 7:1, e o quadrado fica sempre inteiro, sem cortar.
- Nenhum PNG é editado à mão: todos saem dos SVGs-fonte por um script versionado.

## Non-Functional Requirements

- Os PNGs do launcher e do e-mail otimizados; o APK de dev não cresce mais que 100 KB por causa dos ícones.
- A abertura não atrasa a primeira tela: é a do sistema, só com cor e ícone.

## Integrations

Nenhuma externa. Os artefatos do protótipo e do design system "Vitrio" no claude.ai são atualizados (publicação nos artefatos do próprio usuário).

## Acceptance Criteria

- **AC-1** Dado o app instalado, quando aparece no lançador do Android, então o ícone é a forma do sistema em `lime` com o V `plum`; com os ícones com tema ligados, aparece o V monocromático.
- **AC-2** Dado o app abrindo no Android 12 ou mais novo, então a abertura mostra o fundo `hero` com o símbolo no centro, no tema claro e no escuro.
- **AC-3** Dada a boas-vindas, então ela mostra a assinatura escura e, logo abaixo do subtítulo, o check `lime` com "O Vitrio é grátis para todo mundo.", igual à prancheta Main.
- **AC-4** Dado um visitante sem login na aba Conta, então vê o card `hero` com a assinatura, "Peça orçamentos e ofereça seus serviços" e "O Vitrio é grátis para todo mundo.", e os botões "Entrar" e "Criar conta", igual à prancheta.
- **AC-5** Dada a tela Entrar, então ela mostra o símbolo, "Que bom te ver de novo" e "Entre para pedir orçamentos e acompanhar seus pedidos." acima dos campos, igual à prancheta; dada a tela Criar conta, o símbolo, "Crie sua conta" e "É grátis e leva menos de um minuto.".
- **AC-6** Dado qualquer e-mail de conta (código de confirmação, redefinição de senha e os demais), então o cabeçalho traz a assinatura nova.
- **AC-7** Dado `GET /p/{id}`, de perfil publicado ou indisponível, então a página mostra o símbolo ao lado de "Vitrio" e tem o símbolo como ícone da aba.
- **AC-8** Nenhum arquivo do repositório contém o desenho antigo; o README dos logos e a seção "Logo" do design system descrevem a Janela em V.
- **AC-9** Os artefatos do protótipo (Main, Abertura, Conta, Entrar, Criar conta, Destaque) e do design system "Vitrio" mostram o logo novo.
- **AC-10** O ícone de 512×512 e a imagem de destaque de 1024×500 da Google Play saem dos SVGs-fonte pelo script.

## Deferred Decisions

| Decisão | Fallback escolhido agora | Revisitar quando |
| --- | --- | --- |
| Frase "O Vitrio é grátis para todo mundo." | Usada na boas-vindas e na Conta | Antes de lançar qualquer cobrança (plano Pro, destaque patrocinado) |
| Abertura animada | Abertura estática do sistema | Depois do MVP |
| Capturas de tela da Google Play | Nenhuma | Preparação do lançamento |

## Open Questions

Nenhuma.
