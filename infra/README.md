# Infra local

Sobe PostgreSQL com PostGIS e o Seq para desenvolvimento.

```bash
cp infra/.env.example infra/.env        # escolha uma senha local
docker compose -f infra/docker-compose.yml up -d
```

A API lê a conexão do user-secrets:

```bash
cd backend
dotnet user-secrets --project src/Vitrio.Api set "ConnectionStrings:Database" "Host=localhost;Database=vitrio;Username=vitrio;Password=<a mesma senha do .env>"
dotnet ef database update -p src/Vitrio.Infrastructure -s src/Vitrio.Infrastructure --connection "Host=localhost;Database=vitrio;Username=vitrio;Password=<a mesma senha do .env>"
dotnet run --project src/Vitrio.Api
```

- API: http://localhost:5080 (Scalar em `/scalar`, saúde em `/health/ready`)
- Seq: http://localhost:5341
