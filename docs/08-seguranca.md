# Segurança

O Vitrio guarda dados pessoais (nome, telefone, endereço, localização, fotos de dentro de casas) e media encontros presenciais entre desconhecidos. A segurança é requisito de todas as funcionalidades, não uma etapa no fim. Este documento segue o OWASP API Security Top 10 (2023) e a LGPD, e vale como critério de aceite implícito de toda spec.

## 1. Modelo de ameaças

| Ativo | Ameaça | Controle principal |
| --- | --- | --- |
| Endereço do cliente | Prestador mal-intencionado coleta endereços enviando orçamentos falsos | Endereço só em `Scheduled`; janela de visibilidade de 7 dias; limites de orçamentos por prestador; auditoria de acesso ao endereço |
| Endereço do prestador | Triangulação pela distância exibida | Ponto público em célula de ~500 m; distância arredondada; sem coordenadas na resposta |
| Contas | Roubo de sessão, reuso de refresh token | JWT curto, refresh rotativo com detecção de reuso, armazenamento seguro no aparelho |
| Pedidos e pagamentos | Acesso a pedido de outra pessoa (BOLA), confirmação forjada | Toda consulta filtrada pelo dono; só o prestador do pedido confirma; registros imutáveis e auditados |
| Reputação | Avaliações falsas, autoavaliação, ataques de denúncia | Avaliação só com pedido concluído; sinais de mesma pessoa/aparelho; ocultação só com 3 denunciantes distintos |
| Base de prestadores | Raspagem em massa | Rate limit, paginação limitada, sem telefone na listagem |
| Infraestrutura | Credenciais vazadas, dependência vulnerável | Segredos fora do código, varredura de pacotes e de código no CI |

## 2. Autenticação

- **Login social validado no servidor.** O app obtém o ID token do Google ou da Apple e envia para `POST /auth/google` ou `/auth/apple`. A API valida assinatura (JWKS do provedor, com cache), `iss`, `aud` (client ids do app), `exp`, `iat` e, na Apple, o `nonce` (hash SHA-256 do valor gerado pelo app). Nunca confie em e-mail ou nome vindos do app.
- **Token de acesso:** JWT assinado com chave assimétrica (ES256), validade de **10 minutos**, claims mínimas (`sub`, `sid`, `roles`, `iat`, `exp`, `jti`). Sem dados pessoais no token.
- **Refresh token:** valor aleatório de 256 bits, guardado só como hash (SHA-256) no banco, validade de 30 dias deslizante e limite absoluto de 90 dias. **Rotação a cada uso**; se um refresh já usado for apresentado de novo, toda a família de tokens daquela sessão é revogada e o usuário precisa entrar de novo.
- **Sessões:** cada login cria um `sid`; logout revoga o `sid`; "Sair de todos os aparelhos" revoga todos. Suspensão e exclusão de conta revogam tudo na hora (a validação do JWT consulta uma lista de `sid` revogados em cache com TTL igual à vida do token).
- **Rotação de chaves:** chaves de assinatura em cofre, com `kid`; a API publica as públicas atuais e a anterior durante a troca.
- **No app:** tokens só no `flutter_secure_storage` (Keychain/Keystore), nunca em `SharedPreferences`, logs ou URLs. Refresh concorrente é serializado no `ApiClient`.

## 3. Autorização

- **Negar por padrão.** `FallbackPolicy` exige usuário autenticado; endpoints públicos são marcados com `[AllowAnonymous]` explicitamente (busca, perfis, categorias, cidades). O teste de integração lista todas as rotas e falha se alguma não tiver marcação esperada; `get_endpoint_map` do kit confere no `/security-scan`.
- **Políticas:** `Customer`, `Provider` (tem vitrine ativa ou em rascunho), `Admin`. `Admin` só por claim emitida pela API a partir de tabela interna, nunca por request.
- **Autorização por recurso (contra BOLA):** todo handler que lê ou altera um recurso filtra pelo dono **na própria consulta** (`where o.CustomerId == currentUser.Id`), em vez de buscar e comparar depois. Recurso de outro usuário responde **404**, não 403, para não confirmar que existe.
- **Autorização por propriedade (contra BOPLA):** requests e responses são DTOs explícitos por caso de uso. Nada de bind direto em entidade. Campos como `status`, `ratingScore`, `meiVerified`, `role`, `customerId` nunca vêm do cliente.
- **Funções privilegiadas (contra BFLA):** rotas `/admin` em controller separado com `[Authorize(Policy = Admin)]` no nível da classe; ações de moderação exigem motivo e geram auditoria.
- **Endereço:** o DTO de pedido para o prestador só inclui endereço completo se `status ∈ {Scheduled, Completed, Cancelled}` e dentro da janela; cada leitura do endereço completo grava auditoria (quem, quando, pedido).

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
| `auth` | `/auth/*` | 10/min por IP, 30/h por IP |
| `search` | `GET /providers`, `/cities` | 60/min por usuário ou IP |
| `profile` | `GET /providers/{id}` e subrotas | 120/min |
| `write` | demais POST/PUT/PATCH/DELETE | 60/min por usuário |
| `orders` | `POST /orders` | 10/h e 30/dia por cliente |
| `quotes` | `POST /provider/orders/{id}/quote` | 60/h por prestador |
| `reports` | `POST /reports` | 10/dia por usuário |
| `uploads` | `upload-url` | 60/h por usuário |

Respostas 429 com `Retry-After`. Além disso: paginação máxima de 30 itens e 10 páginas na busca; telefone do prestador nunca aparece na listagem, só no perfil e só se ele ativar o WhatsApp.

## 6. Idempotência e concorrência

- `Idempotency-Key` (UUID) obrigatório em criar pedido, enviar orçamento e confirmar pagamento. A API guarda chave + hash do corpo + resposta por 24 h; repetição devolve a mesma resposta; mesma chave com corpo diferente devolve 422.
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
- Cabeçalhos: `X-Content-Type-Options: nosniff`, `Referrer-Policy: no-referrer`, `X-Frame-Options: DENY`, `Content-Security-Policy: default-src 'none'; frame-ancestors 'none'` nas respostas da API; `Cache-Control: no-store` em respostas autenticadas.
- CORS desligado para a API pública (o app não é navegador). Quando houver painel web, CORS só para o domínio do painel, com credenciais e métodos explícitos.
- OpenAPI, Scalar e painel do Hangfire **desligados em produção** ou atrás de autenticação de admin e lista de IPs.
- `ForwardedHeaders` configurado só para o proxy conhecido (sem confiar em `X-Forwarded-For` de qualquer origem), para o rate limit por IP funcionar.

## 10. Segredos e configuração

- Nada de segredo no repositório. Dev: `dotnet user-secrets`. Produção: variáveis de ambiente injetadas pelo cofre do provedor (ou Docker secrets), com rotação documentada.
- Segredos: chaves de assinatura do JWT, credenciais do banco, chaves do storage, credencial do Firebase, client ids/secret da Apple.
- Usuário do banco da API com privilégios mínimos (DML no schema da aplicação); migrações rodam com outro usuário, no pipeline.
- `gitleaks` no CI e pre-commit para barrar segredos.

## 11. Dados pessoais e LGPD

- **Minimização:** só o que a funcionalidade precisa. CPF não é coletado no MVP. Localização do aparelho não é armazenada.
- **Base legal e transparência:** termos e política de privacidade no primeiro acesso e em Conta; consentimento explícito para localização e notificações (permissões do sistema).
- **Direitos do titular:** ver, corrigir e excluir dados pelo app; exportação por solicitação no MVP.
- **Proteção em repouso:** banco e storage com criptografia do provedor; endereços completos e telefone com criptografia de coluna via ASP.NET Core Data Protection (chaves no cofre), com hash determinístico para busca de duplicidade de telefone.
- **Retenção:** pedidos e confirmações de pagamento anonimizados mantidos por 5 anos; logs de aplicação 30 dias; auditoria 1 ano.
- **Logs sem dados pessoais:** sem nome, telefone, endereço, e-mail, tokens ou coordenadas exatas. Ids e `traceId` bastam. Serilog com destructuring policy que mascara propriedades sensíveis.
- **Incidentes:** procedimento escrito (detectar, conter, avaliar, comunicar a ANPD e titulares quando houver risco relevante, corrigir).

## 12. Auditoria

Tabela `audit_events` só de inserção (o usuário da API não tem `update`/`delete` nela): leitura de endereço completo, confirmação e anulação de pagamento, contestação, ações de moderação, mudança de papel, login e revogação de sessões, exclusão de conta. Campos: quando, quem (id), ação, alvo, ip truncado, `traceId`.

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
