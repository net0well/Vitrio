# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0.403-noble AS build
WORKDIR /source

COPY global.json ./
COPY backend/Directory.Build.props backend/Directory.Packages.props backend/.editorconfig backend/
COPY backend/src/Vitrio.Domain/Vitrio.Domain.csproj backend/src/Vitrio.Domain/
COPY backend/src/Vitrio.Application/Vitrio.Application.csproj backend/src/Vitrio.Application/
COPY backend/src/Vitrio.Infrastructure/Vitrio.Infrastructure.csproj backend/src/Vitrio.Infrastructure/
COPY backend/src/Vitrio.Api/Vitrio.Api.csproj backend/src/Vitrio.Api/
RUN dotnet restore backend/src/Vitrio.Api/Vitrio.Api.csproj

COPY backend/src/ backend/src/
RUN dotnet publish backend/src/Vitrio.Api/Vitrio.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app \
    -p:UseAppHost=false

# chiseled-extra: no shell, non-root by default, and it ships ICU and tzdata, which the city time zone rules need.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_gcServer=0
COPY --from=build /app ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "Vitrio.Api.dll"]
