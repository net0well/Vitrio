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

O deploy de produção está em [production/README.md](production/README.md).
