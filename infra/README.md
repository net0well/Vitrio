# Infra local

Sobe PostgreSQL com PostGIS, Redis (cache, sem persistência), Mailpit, SeaweedFS (storage S3 das fotos) e o Seq para desenvolvimento.

```bash
cp infra/.env.example infra/.env        # escolha uma senha local e as chaves do S3
docker compose -f infra/docker-compose.yml up -d
```

A API lê a conexão do user-secrets:

```bash
cd backend
dotnet user-secrets --project src/Vitrio.Api set "ConnectionStrings:Database" "Host=localhost;Database=vitrio;Username=vitrio;Password=<a mesma senha do .env>"
dotnet user-secrets --project src/Vitrio.Api set "Storage:AccessKey" "<S3_ACCESS_KEY do .env>"
dotnet user-secrets --project src/Vitrio.Api set "Storage:SecretKey" "<S3_SECRET_KEY do .env>"
dotnet ef database update -p src/Vitrio.Infrastructure -s src/Vitrio.Infrastructure --connection "Host=localhost;Database=vitrio;Username=vitrio;Password=<a mesma senha do .env>"
dotnet run --project src/Vitrio.Api
```

- API: http://localhost:5080 (Swagger em `/swagger`, saúde em `/health/ready`)
- Redis: `localhost:6379` (já configurado em `appsettings.Development.json`; sem Redis a API usa só o cache em memória)
- Mailpit (caixa de e-mail de teste; todo e-mail da API em desenvolvimento cai aqui): http://localhost:8025, SMTP em `localhost:1025`
- SeaweedFS (S3): http://localhost:8333. A API cria os buckets `vitrio-uploads` (privado, recebe o original) e `vitrio-public` (leitura anônima, só fotos processadas) ao subir. As URLs de envio e das fotos usam `10.0.2.2:8333`, o endereço do computador visto pelo emulador Android.
- Hangfire: roda dentro da API (schema `hangfire` no mesmo banco), com a limpeza de envios abandonados a cada hora.
- Seq: http://localhost:5341

Para rodar a API em contêiner, com a mesma imagem de produção (aplique as migrações antes, como acima):

```bash
docker compose -f infra/docker-compose.yml --profile api up -d --build
```

## Carga para medir a busca

Os scripts de `search-load/` medem a busca de prestadores com 50 mil vitrines publicadas espalhadas pelos municípios de SP, com peso pela população. Rode só no banco local, com as migrações aplicadas e as cidades carregadas:

```bash
docker cp infra/search-load/seed.sql vitrio-postgres-1:/tmp/seed.sql
docker exec vitrio-postgres-1 psql -U vitrio -d vitrio -q -f /tmp/seed.sql
pwsh infra/search-load/measure.ps1        # com a API rodando em http://localhost:5080
docker cp infra/search-load/cleanup.sql vitrio-postgres-1:/tmp/cleanup.sql
docker exec vitrio-postgres-1 psql -U vitrio -d vitrio -q -f /tmp/cleanup.sql
```

- As contas da carga usam e-mails `@carga.vitrio.local`, e a limpeza apaga só elas (a conta fixa de teste fica).
- O `measure.ps1` faz 55 requisições por cenário e espera a janela do rate limit (60 por minuto) entre um cenário e outro; a medição completa leva cerca de 7 minutos.
- Ele chama `127.0.0.1`, não `localhost`: no Windows, `localhost` tenta IPv6 antes e soma uns 200 ms a cada requisição.

## Logo e ícones

Os SVGs de `docs/design-system/assets/logos/` e `docs/design-system/assets/store/` são a fonte da marca. O script de `brand/` gera todos os PNGs a partir deles (design system, ícone do app, abertura antiga do Android, e-mail e Google Play) e copia os SVGs para `mobile/assets/brand/` e o símbolo e a assinatura clara para a página de compartilhar da API (`backend/src/Vitrio.Api/Share/Templates/`):

```bash
cd infra/brand
npm ci
node render.mjs
cd ../../mobile && dart run flutter_launcher_icons   # recria os ícones do Android a partir de assets/launcher/
```

- Nenhum PNG é editado à mão: mude o SVG e rode o script.
- O texto da imagem de destaque da Google Play vira contorno no script, com o peso pedido, a partir das fontes variáveis do app.

O deploy de produção está em [production/README.md](production/README.md).
