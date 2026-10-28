# Segurança

O Vitrio guarda dados pessoais (nome, telefone, endereço, localização, fotos de dentro de casas) e media encontros presenciais entre desconhecidos. A segurança é requisito de todas as funcionalidades, não uma etapa no fim. Este documento segue o OWASP API Security Top 10 (2023) e a LGPD, e vale como critério de aceite implícito de toda spec.

## 1. Modelo de ameaças

| Ativo | Ameaça | Controle principal |
| --- | --- | --- |
| Endereço do cliente | Prestador mal-intencionado coleta endereços enviando orçamentos falsos | Endereço só em `Scheduled`; janela de visibilidade de 7 dias; limites de orçamentos por prestador; auditoria de acesso ao endereço |
| Endereço do prestador | Triangulação pela distância exibida ou pela borda do raio (variar o ponto da busca e ver onde o prestador aparece e some) | Ponto público em célula de ~500 m usado em **todo** filtro e distância; distância arredondada; sem coordenadas na resposta |
| Contas | Roubo de sessão, reuso de refresh token | JWT curto, refresh rotativo com detecção de reuso, armazenamento seguro no aparelho |
| Pedidos e pagamentos | Acesso a pedido de outra pessoa (BOLA), confirmação forjada | Toda consulta filtrada pelo dono; só o prestador do pedido confirma; registros imutáveis e auditados |
| Reputação | Avaliações falsas, autoavaliação, ataques de denúncia | Avaliação só com pedido concluído; sinais de mesma pessoa/aparelho; ocultação só com 3 denunciantes distintos |
| Base de prestadores | Raspagem em massa | Rate limit, paginação limitada, sem telefone na listagem |
| Infraestrutura | Credenciais vazadas, dependência vulnerável | Segredos fora do código, varredura de pacotes e de código no CI |

## 2. Autenticação

- **Cadastro e login próprios**, sem provedor externo. Detalhes e critérios em [specs/001-contas-e-autenticacao.md](specs/001-contas-e-autenticacao.md).
- **Senha:** 10 a 128 caracteres (Unicode NFKC), sem regra de composição, recusando a lista de senhas comuns e a senha igual ao e-mail. Hash com o `PasswordHasher` do ASP.NET Core Identity (PBKDF2-HMAC-SHA512, formato v3), refeito no login quando o algoritmo for atualizado. A senha nunca é logada, devolvida nem guardada de outro jeito.
- **E-mail confirmado antes do primeiro acesso:** código de 6 dígitos de gerador criptográfico, válido por 10 min, uso único, até 5 tentativas, guardado só como HMAC-SHA256 com chave do servidor e comparado em tempo constante. No máximo 5 códigos por e-mail por hora, com 60 s entre envios. O mesmo mecanismo serve para redefinir a senha.
- **Sem enumeração de contas:** cadastro, reenvio de código e "Esqueci a senha" respondem sempre igual; o login responde o mesmo erro para e-mail inexistente e senha errada e roda uma verificação de senha falsa para levar o mesmo tempo. O dono de um e-mail já cadastrado recebe aviso quando alguém tenta cadastrá-lo de novo.
- **Força bruta:** rate limit `auth` por IP e bloqueio da conta por 15 min depois de 5 senhas erradas em 15 min (429 com `Retry-After`); redefinir a senha desbloqueia.
- **Redefinição e troca de senha:** redefinir revoga todas as sessões; trocar logado revoga as outras. Os dois enviam o e-mail "Sua senha foi alterada".
- **E-mails de conta:** sem link para entrar (o código é digitado no app), sem dado pessoal além do primeiro nome, sem rastreamento, com o aviso de que o Vitrio nunca pede senha ou código ([design-system/emails.md](design-system/emails.md)). Domínio de envio com SPF, DKIM e DMARC. O código não passa pela outbox nem por log.
- **Token de acesso:** JWT assinado com chave assimétrica (ES256), validade de **10 minutos**, claims mínimas (`sub`, `sid`, `roles`, `iat`, `exp`, `jti`). Sem dados pessoais no token.
- **Refresh token:** valor aleatório de 256 bits, guardado só como hash (SHA-256) no banco, validade de **60 dias deslizantes** e limite absoluto de **1 ano** desde o login, para a pessoa continuar conectada sem entrar toda hora. **Rotação a cada uso**; se um refresh já usado for apresentado de novo, toda a família de tokens daquela sessão é revogada e o usuário precisa entrar de novo.
- **Sessões:** cada login cria um `sid`; logout revoga o `sid`; "Sair de todos os aparelhos" revoga todos; "Aparelhos conectados" lista as sessões e encerra qualquer uma. Suspensão e exclusão de conta revogam tudo na hora. A revogação é gravada no banco (`Session.RevokedAt` e status da conta); a validação do JWT consulta esse estado por trás de um cache de até 1 min, então um token revogado é recusado em até 1 min mesmo depois de reinício e com várias instâncias ([09-cache.md §4](09-cache.md)).
- **Rotação de chaves:** chaves de assinatura em cofre, com `kid`; a API publica as públicas atuais e a anterior durante a troca.
- **No app:** tokens só no `flutter_secure_storage` (Keychain/Keystore), nunca em `SharedPreferences`, logs ou URLs. Refresh concorrente é serializado no `ApiClient`.

## 3. Autorização

- **Negar por padrão.** `FallbackPolicy` exige usuário autenticado; endpoints públicos são marcados com `[AllowAnonymous]` explicitamente (busca, perfis, categorias, cidades). O teste de integração lista todas as rotas e falha se alguma não tiver marcação esperada; `get_endpoint_map` do kit confere no `/security-scan`.
- **Políticas:** `Customer`, `Provider` (tem vitrine em `Draft`, `Published` ou `Paused`; pausada continua gerenciando a vitrine e os pedidos em andamento; conta suspensa não chega a autenticar), `Admin`. `Admin` só por claim emitida pela API a partir de tabela interna, nunca por request.
- **Autorização por recurso (contra BOLA):** todo handler que lê ou altera um recurso filtra pelo dono **na própria consulta** (`where o.CustomerId == currentUser.Id`), em vez de buscar e comparar depois. Recurso de outro usuário responde **404**, não 403, para não confirmar que existe.
- **Autorização por propriedade (contra BOPLA):** requests e responses são DTOs explícitos por caso de uso. Nada de bind direto em entidade. Campos como `status`, `ratingScore`, `meiVerified`, `role`, `customerId` nunca vêm do cliente.
- **Funções privilegiadas (contra BFLA):** rotas `/admin` em controller separado com `[Authorize(Policy = Admin)]` no nível da classe; ações de moderação exigem motivo e geram auditoria.
- **Endereço:** o DTO de pedido para o prestador só inclui endereço completo quando `AddressReleasedAt` existe, o pedido não foi cancelado e ainda não passaram 7 dias do fim da faixa agendada, qualquer que seja o status (inclusive `PaymentConfirmed`, `PaymentDisputed` e `PaymentVoided`). Cada leitura do endereço completo grava auditoria (quem, quando, pedido).

## 4. Validação de entrada e saída

- FluentValidation em todo comando e consulta: tamanhos máximos, formatos (CEP, CNPJ com dígito verificador, telefone E.164), faixas (raio 1–50, nota 1–5, valor R$ 1–100.000), enums fechados.
- Limite de tamanho de corpo: 64 KB por padrão (`[RequestSizeLimit]`); nenhum endpoint recebe arquivo.
- Texto livre: normalizado (trim, Unicode NFC, remoção de caracteres de controle), sem HTML; o app exibe como texto puro. Avaliações e mensagens recusam telefone, e-mail e link.
- Coordenadas: latitude −34 a 6 e longitude −74 a −28 (território brasileiro com folga); fora disso, 400.
- Respostas não vazam detalhes: ProblemDetails padronizado, sem stack trace, sem mensagens do banco; `traceId` para suporte.
- JSON: `UnmappedMemberHandling.Disallow` nos requests para rejeitar campos desconhecidos.

## 5. Limites de uso (contra abuso e raspagem)

Rate limiting nativo do ASP.NET Core, particionado por usuário autenticado ou, sem login, por IP:

| Política | Rotas | Limite |
| --- | --- | --- |
| `auth` | `/auth/*` | 10/min por IP, 30/h por IP; além disso, no handler, 5 códigos de e-mail por endereço por hora e bloqueio da conta após 5 senhas erradas em 15 min |
| `search` | `GET /providers`, `/cities` | 60/min por usuário ou IP |
| `profile` | `GET /providers/{id}` e subrotas | 120/min |
| `write` | demais POST/PUT/PATCH/DELETE | 60/min por usuário |
| `orders` | `POST /orders` | 10/h e 30/dia por cliente |
| `quotes` | `POST /provider/orders/{id}/quote` | 60/h por prestador |
| `reports` | `POST /reports` | 10/dia por usuário |
| `uploads` | `upload-url` | 60/h por usuário |

Respostas 429 com `Retry-After`. Os contadores ficam na memória de cada instância: com uma instância os limites valem como escritos; ao escalar para mais de uma, passam para o Redis ([09-cache.md §6](09-cache.md)). Além disso: paginação máxima de 30 itens e 10 páginas na busca; telefone do prestador nunca aparece na listagem, só no perfil e só se ele ativar o WhatsApp.

## 6. Idempotência e concorrência

- `Idempotency-Key` (UUID) obrigatório em criar pedido, enviar orçamento e confirmar pagamento. A API guarda chave + hash do corpo + resposta **no banco** (tabela `idempotency_keys`, na mesma transação do comando) por 24 h; repetição devolve a mesma resposta; mesma chave com corpo diferente devolve 422. Nunca em cache: reinício ou segunda instância permitiriam duplicar.
- Concorrência otimista (`xmin`) em pedidos, vitrines e avaliações; conflito devolve 409 e o app recarrega.
- Transições de estado validadas no domínio; o banco tem `check constraint` nos estados e `unique` em (pedido) para avaliação e orçamento ativo.

## 7. Uploads

- Upload direto ao storage por URL assinada de `PUT`, com `Content-Type` e `Content-Length` fixados na assinatura, 5 minutos de validade e chave aleatória.
- Após o upload, um job: confere a assinatura binária (magic bytes) contra o tipo declarado, rejeita acima de 10 MB ou 8000 px, **remove todos os metadados (EXIF com GPS incluído)**, reencoda em WebP com SkiaSharp e só então publica. O arquivo original é apagado.
- Buckets sem listagem pública. Fotos de pedido são privadas e servidas por URL assinada de leitura de 10 minutos, emitida só para cliente e prestador do pedido.
- Uploads não confirmados em 24 h são apagados.

## 8. Integrações externas (contra SSRF e dependência)

- A API só chama destinos fixos por configuração (provedores de login, BrasilAPI, FCM, storage). Nenhum endpoint aceita URL vinda do usuário.
- `IHttpClientFactory` com clientes nomeados, `BaseAddress` fixo, timeout de 5 s, retry com backoff e circuit breaker (Polly via `Microsoft.Extensions.Http.Resilience`).
- Respostas externas são validadas como qualquer entrada (o CNPJ devolvido tem de ser o consultado; campos obrigatórios presentes).
- Falha da BrasilAPI não bloqueia o cadastro: o selo fica pendente e o job tenta de novo.

## 9. Transporte e cabeçalhos

- HTTPS obrigatório (TLS 1.2+) com HSTS (`max-age` 1 ano, `includeSubDomains`) no proxy; HTTP só redireciona.
- Cabeçalhos: `X-Content-Type-Options: nosniff`, `Referrer-Policy: no-referrer`, `X-Frame-Options: DENY`, `Content-Security-Policy: default-src 'none'; frame-ancestors 'none'` nas respostas da API; `Cache-Control: no-store` em toda resposta por padrão e sempre nas autenticadas. Só categorias e cidades declaram cache público ([09-cache.md §7](09-cache.md)).
- CORS desligado para a API pública (o app não é navegador). Quando houver painel web, CORS só para o domínio do painel, com credenciais e métodos explícitos.
- OpenAPI, Scalar e painel do Hangfire **desligados em produção** ou atrás de autenticação de admin e lista de IPs.
- `ForwardedHeaders` configurado só para o proxy conhecido (sem confiar em `X-Forwarded-For` de qualquer origem), para o rate limit por IP funcionar.

## 10. Segredos e configuração

- Nada de segredo no repositório. Dev: `dotnet user-secrets`. Produção: variáveis de ambiente injetadas pelo cofre do provedor (ou Docker secrets), com rotação documentada.
- Segredos: chaves de assinatura do JWT, chave HMAC dos códigos de e-mail, credenciais do banco, do SMTP e do storage, senha do Redis, credencial do Firebase.
- Usuário do banco da API com privilégios mínimos (DML no schema da aplicação); migrações rodam com outro usuário, no pipeline.
- `gitleaks` no CI e pre-commit para barrar segredos.

## 11. Dados pessoais e LGPD

- **Minimização:** só o que a funcionalidade precisa. CPF não é coletado no MVP. Localização do aparelho não é armazenada.
- **Base legal e transparência:** termos e política de privacidade no primeiro acesso e em Conta; consentimento explícito para localização e notificações (permissões do sistema).
- **Direitos do titular:** ver, corrigir e excluir dados pelo app; exportação por solicitação no MVP.
- **Proteção em repouso:** banco e storage com criptografia do provedor; endereços completos e telefone com criptografia de coluna via ASP.NET Core Data Protection (chaves no cofre), com hash determinístico para busca de duplicidade de telefone.
- **Retenção:** pedidos e confirmações de pagamento anonimizados mantidos por 5 anos; logs de aplicação 30 dias; auditoria 1 ano, exceto a de pagamentos (confirmação, anulação, contestação e resolução), guardada 5 anos junto com os registros que ela explica.
- **Logs sem dados pessoais:** sem nome, telefone, endereço, e-mail, tokens ou coordenadas exatas. Ids e `traceId` bastam. Serilog com destructuring policy que mascara propriedades sensíveis.
- **Incidentes:** procedimento escrito (detectar, conter, avaliar, comunicar a ANPD e titulares quando houver risco relevante, corrigir).

## 12. Auditoria

Tabela `audit_events` só de inserção (o usuário da API não tem `update`/`delete` nela): leitura de endereço completo, confirmação e anulação de pagamento, contestação, ações de moderação, mudança de papel, cadastro e confirmação de e-mail, login (sucesso e bloqueio), redefinição e troca de senha, revogação de sessões, exclusão de conta. Campos: quando, quem (id), ação, alvo, ip truncado, `traceId`.

## 13. App mobile

- Sem segredos no binário (chaves de API de terceiros que precisem ir no app são restritas por pacote/bundle id).
- Build de release com `--obfuscate --split-debug-info`; logs de depuração desligados.
- Deep links validados contra rotas conhecidas; nada de abrir URL arbitrária vinda de push ou API.
- Sem WebView para conteúdo de terceiros; links externos no navegador do sistema.
- Captura de tela permitida (não é app financeiro); dados sensíveis (endereço) não ficam em cache persistente.
- Depois do MVP: Play Integrity e App Attest para dificultar automação de cadastro e avaliações.

## 14. Verificação contínua

| Quando | O quê |
| --- | --- |
| Cada PR | Testes de autorização (cada endpoint protegido responde 401 sem token e 404 para recurso de outro usuário), `/security-scan` do kit, CodeQL, `dotnet list package --vulnerable --include-transitive`, gitleaks |
| Semanal | Dependabot para NuGet, pub e Actions; scan de imagem Docker |
| Antes de cada release | OWASP ZAP baseline contra staging; revisão do mapa de endpoints (`authorized`/`anonymous`) |
| Semestral | Revisão deste documento e teste de restauração de backup |

## 15. Checklist de segurança por funcionalidade

Toda spec responde, na seção Authorization e Non-Functional:

1. Quem pode chamar cada endpoint e como a consulta filtra pelo dono?
2. Que dados pessoais entram e saem, e algum pode ser removido?
3. Que limites de tamanho, faixa e taxa se aplicam?
4. A operação precisa de idempotência ou trava de concorrência?
5. O que vai para auditoria?
6. O que acontece quando uma integração externa falha?
