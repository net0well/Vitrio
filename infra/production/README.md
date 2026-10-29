# Produção

Um servidor Linux com Docker Compose: a API, o Redis (cache, sem persistência e com senha) e o Caddy (TLS automático, HSTS e proxy). O PostgreSQL é gerenciado, fora deste servidor.

## Primeira instalação

1. Crie o DNS de `API_DOMAIN` apontando para o servidor e libere as portas 80 e 443.
2. Copie `docker-compose.yml`, `Caddyfile` e `.env.example` para o servidor.
3. `cp .env.example .env` e preencha. Depois, `chmod 600 .env`.
   - `REDIS_PASSWORD`: `openssl rand -hex 32`.
   - `AUTH_CODE_KEY` (chave dos códigos de e-mail): `openssl rand -base64 32`.
   - `JWT_SIGNING_KEY` (chave ES256 dos tokens), numa linha só entre aspas duplas: `openssl genpkey -algorithm EC -pkeyopt ec_paramgen_curve:P-256 | awk '{printf "%s\\n", $0}'`.
   - `SMTP_*`: servidor do provedor de e-mail, com STARTTLS na porta 587.
   - `DATA_PROTECTION_CERTIFICATE` e `DATA_PROTECTION_CERTIFICATE_PASSWORD`: certificado que protege as chaves da criptografia de celular e endereço (as chaves ficam no banco, cifradas por ele). Gere uma vez, guarde no cofre com cópia de segurança e não troque sem migrar os dados:
     ```bash
     openssl req -x509 -newkey rsa:3072 -nodes -days 3650 -subj "/CN=vitrio-data-protection" -keyout dp.key -out dp.crt
     openssl pkcs12 -export -inkey dp.key -in dp.crt -out dp.pfx -passout pass:"$DATA_PROTECTION_CERTIFICATE_PASSWORD"
     base64 -w0 dp.pfx   # valor de DATA_PROTECTION_CERTIFICATE
     ```
4. No banco, a API usa um usuário só com permissão de leitura e escrita nas tabelas do schema da aplicação. As migrações rodam com outro usuário, que pode alterar o schema.

A API se recusa a subir em produção sem `AllowedHosts` com o domínio, sem o proxy conhecido (`ForwardedHeaders`), sem senha no Redis, sem as chaves de token e de código, sem o servidor SMTP ou sem o certificado da Data Protection. Perder o certificado torna ilegíveis os celulares e endereços já salvos. Trocar `JWT_SIGNING_KEY` desconecta todo mundo; trocar `AUTH_CODE_KEY` invalida só os códigos ainda não usados. Sem o proxy conhecido, todos os clientes cairiam no mesmo balde de rate limit.

## Deploy de uma versão

```bash
# na máquina de build, a partir da raiz do repositório
tag=$(git rev-parse --short HEAD)
docker build -f backend/Dockerfile -t ghcr.io/net0well/vitrio-api:$tag .
docker push ghcr.io/net0well/vitrio-api:$tag

# migrações, com o usuário de migração, antes da nova imagem
dotnet ef migrations script --idempotent -p backend/src/Vitrio.Infrastructure -s backend/src/Vitrio.Infrastructure -o migrations.sql
psql "<conexão do usuário de migração>" -v ON_ERROR_STOP=1 -f migrations.sql

# no servidor
sed -i "s|^API_IMAGE=.*|API_IMAGE=ghcr.io/net0well/vitrio-api:$tag|" .env
docker compose up -d
curl -fsS http://127.0.0.1:8081/health/ready
```

- **Rollback:** volte o `API_IMAGE` para a tag anterior e rode `docker compose up -d` de novo. As migrações precisam ser compatíveis com a versão anterior (adicionar antes, remover numa versão seguinte).
- O Caddy só expõe `/api/*`. `/health/live` e `/health/ready` ficam em `127.0.0.1:8081`, para o monitoramento no próprio servidor.
- Durante um restart, o Caddy segura a requisição por até 15 s esperando a API ficar saudável, em vez de devolver 503.
- A API roda sem root, com sistema de arquivos só de leitura e sem capabilities do Linux.
