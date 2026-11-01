# E-mails

Os e-mails do Vitrio são poucos e de conta: código de confirmação, código de redefinição de senha e avisos de segurança. Eles seguem a mesma identidade do app (tokens, voz e tom deste design system), com as limitações de clientes de e-mail. O modelo de referência está em [`emails/codigo-de-confirmacao.html`](emails/codigo-de-confirmacao.html) e sua versão em texto em [`emails/codigo-de-confirmacao.txt`](emails/codigo-de-confirmacao.txt).

Capturas do modelo no Mailpit: [tema claro](emails/preview-claro.png), [tema escuro](emails/preview-escuro.png) e [celular, 390 px](emails/preview-celular.png).

## Princípios

- **Um assunto por e-mail e uma informação principal.** No e-mail de código, o código é o protagonista.
- **Nenhum link para entrar.** O código é digitado no app. Sem botão "Entrar" e sem link de confirmação, um golpista não consegue imitar o e-mail com um link falso que funcione igual.
- **Nunca pedir senha nem dado pessoal.** O rodapé de todo e-mail diz isso.
- **Só o necessário:** primeiro nome e, quando houver, o código. Nada de endereço, telefone, pedido ou valor.
- **Sem rastreamento:** nenhum pixel de abertura, nenhum link com parâmetro de campanha.
- **Sempre duas versões:** HTML com este layout e texto puro com o mesmo conteúdo (`multipart/alternative`).

## Estrutura

```text
┌──────────────── fundo surface ────────────────┐
│  ┌──────── cabeçalho hero (plum) ─────────┐   │
│  │  logo on-dark                          │   │
│  ├────────── cartão surface-raised ───────┤   │
│  │  Título (title)                        │   │
│  │  Texto (body, ink-2)                   │   │
│  │  ┌──── bloco do código (plum-soft) ──┐ │   │
│  │  │        4 8 2 9 1 7                │ │   │
│  │  └───────────────────────────────────┘ │   │
│  │  Nota (meta, ink-muted)                │   │
│  └────────────────────────────────────────┘   │
│  Rodapé (caption, ink-muted)                  │
└───────────────────────────────────────────────┘
```

- Largura máxima de 600 px, centralizada; abaixo de 620 px ocupa 100% com margem lateral de 20 px (`sp-20`).
- Layout em tabelas (`role="presentation"`) e CSS inline: é o que funciona no Gmail, no Outlook e no Apple Mail. O bloco `<style>` só complementa (tema escuro e celular).
- Cantos: cabeçalho e cartão juntos formam um card com `radius-card` (20 px); o bloco do código usa `radius-xl` (16 px).
- Espaços: padding do cartão 32 px (20 px no celular), 12 px entre título e texto, 24 px antes do bloco do código, 16 px depois.

## Cores

Só tokens de [tokens.json](tokens.json). Cada classe do modelo tem o valor claro inline e o escuro no `@media (prefers-color-scheme: dark)`.

| Elemento | Token | Claro | Escuro |
| --- | --- | --- | --- |
| Fundo da página | `surface` | `#F6F4F8` | `#141019` |
| Cabeçalho | `hero` | `#3E1F57` | `#2A1640` |
| Cartão | `surface-raised` | `#FFFFFF` | `#1E1826` |
| Borda do cartão | `line-soft` | `#EEEAF2` | `#2A2333` |
| Título | `ink` | `#17121F` | `#F3EFF7` |
| Texto | `ink-2` | `#4F4960` | `#CFC7DA` |
| Nota e rodapé | `ink-muted` | `#6B6578` | `#A79EB4` |
| Fundo do código | `plum-soft` | `#EFE7F6` | `#2E2140` |
| Código | `plum` | `#3E1F57` | `#B994DD` |
| Aviso de segurança (faixa) | `amber-soft` / `amber-ink` | `#FCEFD6` / `#8A5300` | `#3A2A0C` / `#F2C36B` |

- `lime` aparece só no logo (moldura dos vidros), nunca como texto.
- Nada de cor fora dos tokens, e status nunca só por cor: o aviso de segurança tem título escrito.

## Tipografia

Clientes de e-mail não carregam as fontes do app de forma confiável, então cada texto declara a família do design system com uma pilha de reserva:

- Títulos e código: `'Bricolage Grotesque', 'Plus Jakarta Sans', 'Segoe UI', Roboto, Helvetica, Arial, sans-serif`.
- Texto: `'Plus Jakarta Sans', 'Segoe UI', Roboto, Helvetica, Arial, sans-serif`.

| Uso | Estilo | Tamanho / altura | Peso |
| --- | --- | --- | --- |
| Título | `title` | 22 / 26 px | 700 |
| Texto | `body` (maior no e-mail) | 15 / 22 px | 400 |
| Código | `price-lg` | 36 / 40 px, espaçamento 10 px (28 px e 6 px no celular) | 800 |
| Nota | `meta` | 13 / 18 px | 600 |
| Rodapé | `caption` | 12 / 16 px | 600 |

Nada abaixo de 12 px. O código usa só dígitos e espaçamento entre letras, sem espaços de verdade, para copiar e colar inteiro.

## Logo

- Arquivo: [`assets/logos/vitrio-wordmark-on-dark@2x.png`](assets/logos/vitrio-wordmark-on-dark@2x.png) (368 × 128), exibido a 92 × 32 px sobre o cabeçalho `hero`. Gerado a partir do `vitrio-wordmark-on-dark.svg`, porque o Gmail não exibe SVG.
- Vai **embutido no e-mail** como anexo inline (`cid:vitrio-wordmark`), não por URL: aparece mesmo com imagens remotas bloqueadas e não avisa ninguém quando o e-mail é aberto.
- `alt="Vitrio"`, sem link.

## Tema escuro e celular

- `<meta name="color-scheme" content="light dark">` e `supported-color-schemes` no `<head>`.
- O `@media (prefers-color-scheme: dark)` troca as cores pelos valores escuros da tabela acima (Apple Mail, Outlook no Mac e no iOS). O Gmail aplica a própria inversão; os contrastes dos tokens continuam legíveis.
- O `@media (max-width: 620px)` reduz paddings e o tamanho do código.

## Acessibilidade

- `<html lang="pt-BR">`, `<title>` igual ao assunto e um único `<h1>`.
- Tabelas de layout com `role="presentation"`; texto real, nunca texto dentro de imagem.
- Contraste dos tokens (4.5:1 para texto) nos dois temas.
- Pré-cabeçalho escondido com o resumo do e-mail, para quem vê só a lista de mensagens e para leitores de tela.

## Voz e conteúdo

Segue "Voz e tom" do [README](README.md): você, frases curtas, sem exclamação, sem urgência fabricada. Explica em uma linha por que a pessoa recebeu o e-mail e o que fazer se não foi ela.

- **Remetente:** `Vitrio <nao-responda@vitrio.app>`.
- **Rodapé de todos:** "O Vitrio nunca pede sua senha nem códigos por e-mail, telefone ou WhatsApp. Este é um e-mail automático; respostas não são lidas." e, na linha de baixo, "Vitrio · vitrio.app".

### Catálogo

| E-mail | Assunto | Pré-cabeçalho | Título | Texto | Destaque | Nota |
| --- | --- | --- | --- | --- | --- | --- |
| Código de confirmação | `{{code}} é seu código do Vitrio` | Use este código para confirmar seu e-mail. | Confirme seu e-mail | Olá, {{firstName}}. Digite este código no app para terminar seu cadastro. | Bloco do código | O código vale por 10 minutos. Se você não criou uma conta no Vitrio, ignore este e-mail: nada é criado sem o código. |
| Código de redefinição | `{{code}} é seu código para redefinir a senha` | Use este código para criar uma senha nova. | Redefina sua senha | Olá, {{firstName}}. Digite este código no app para criar uma senha nova. | Bloco do código | O código vale por 10 minutos. Se você não pediu, ignore este e-mail: sua senha continua a mesma. |
| Senha alterada | Sua senha do Vitrio foi alterada | Se não foi você, redefina a senha agora. | Sua senha foi alterada | Olá, {{firstName}}. A senha da sua conta foi alterada em {{changedAt}}, e os outros aparelhos foram desconectados. | Faixa de aviso: "Não foi você? Abra o app, toque em Esqueci a senha e crie uma nova." | — |
| Tentativa de cadastro | Alguém tentou criar uma conta com seu e-mail | Sua conta continua segura. | Seu e-mail já tem conta | Olá, {{firstName}}. Alguém tentou criar uma conta no Vitrio com este e-mail, que já é seu. Nada mudou na sua conta. | Faixa de aviso: "Esqueceu a senha? No app, toque em Esqueci a senha." | Se foi você, é só entrar com sua senha. |

- `{{firstName}}` é o primeiro nome de `DisplayName`; `{{changedAt}}` vem no formato "qui 29/10 às 14:05", no fuso de São Paulo.
- Todos os valores são escapados para HTML antes de entrar no modelo.

## Implementação

- Envio por SMTP com MailKit, depois do commit e direto do handler (o código nunca fica gravado em texto). Em desenvolvimento, o Mailpit do `infra/docker-compose.yml` recebe tudo (SMTP `localhost:1025`, caixa em `http://localhost:8025`).
- Os modelos de produção ficam em `backend/src/Vitrio.Infrastructure/Email/Templates/` (um HTML de layout, o conteúdo de cada e-mail e a versão em texto), derivados do modelo de referência desta pasta. Mudança visual começa aqui, no design system.
- Cabeçalhos: `Auto-Submitted: auto-generated` e nenhum `List-Unsubscribe` (são e-mails de conta, não de marketing).

## Como validar

1. Subir o Mailpit (`docker compose -f infra/docker-compose.yml up -d mailpit`) e disparar o e-mail pelo fluxo do app ou pelo teste de integração.
2. Conferir na caixa do Mailpit: versões HTML e texto, logo embutido, tema claro e escuro.
3. Rodar a verificação de HTML do Mailpit (aba "HTML Check" ou `GET /api/v1/message/{id}/html-check`); compatibilidade abaixo de 85% exige ajuste.
4. Antes do lançamento, abrir pelo menos no Gmail (Android e web), no Outlook e no Apple Mail.
