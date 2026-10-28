# Spec 002 — Localização e cidades

- **Status:** Draft
- **Data:** 2026-10-28
- **Responsável:** Wellington

## Problem

O Vitrio funciona em qualquer cidade do Brasil. Para mostrar quem atende perto, o app precisa saber onde a pessoa está (GPS) ou em que cidade ela quer procurar, sem guardar histórico de localização e sem depender do GPS para funcionar.

## Scope

### In

- Base com todos os municípios do Brasil (código IBGE, nome, UF, limite e centroide).
- Descobrir a cidade e o bairro aproximado a partir de um ponto (`locate`).
- Buscar cidades por nome (sem acento, com UF), para o seletor.
- Pré-permissão de localização no app e fluxo sem GPS.
- Estado de localização no app: "Perto de mim" (GPS) ou "Cidade escolhida", com cidades recentes salvas no aparelho.
- Consulta de CEP para endereços (usada por 001 e 003).

### Out

- Rastreamento contínuo ou em segundo plano.
- Lista oficial de bairros por cidade.
- Mapa.

## Domain Model

- `City` (referência, somente leitura): `IbgeCode` (7 dígitos, chave), `Name`, `NameNormalized`, `State` (UF), `Boundary` (`geography(MultiPolygon)` simplificada), `Centroid` (`geography(Point)`), `TimeZone` (IANA, ex.: `America/Sao_Paulo`).
- `LocationContext` (valor usado pelas consultas, não persistido): `Mode` (`Gps`, `ChosenCity`), `Point?`, `CityIbgeCode`.
- Carga: script em `infra/ibge/` lê a API de localidades e as malhas do IBGE, simplifica os polígonos e popula a tabela `cities`; idempotente; roda na criação do banco e uma vez por ano.

## API Contract

| Verbo | Rota | Request | Response | Erros |
| --- | --- | --- | --- | --- |
| GET | `/api/v1/cities/locate?lat=&lng=` | coordenadas WGS84 | `{ city: { ibgeCode, name, state }, neighborhood?, precision: "boundary" \| "nearest" }` | 400 coordenada fora do Brasil, 404 nenhuma cidade até 30 km, 429 |
| GET | `/api/v1/cities?search=&state=&limit=` | `search` ≥ 2 caracteres, `limit` ≤ 20 | `[{ ibgeCode, name, state }]` | 400, 429 |
| GET | `/api/v1/cities/{ibgeCode}` | — | `{ ibgeCode, name, state, centroid: { lat, lng }, timeZone }` | 404 |
| GET | `/api/v1/ceps/{cep}` | 8 dígitos | `{ cep, street, neighborhood, city: { ibgeCode, name, state } }` | 400 formato, 404 não encontrado, 503 serviço indisponível |

## Authorization

- `cities/*`: anônimo, política `search`.
- `ceps/*`: autenticado (evita uso da API como proxy de CEP), política `write`.
- Coordenadas recebidas não são persistidas nem logadas (log só com cidade resultante).

## Edge Cases & Failure Modes

- Ponto no mar, fronteira ou fora de qualquer limite simplificado: usa o centroide mais próximo até 30 km (`precision: "nearest"`); além disso, 404 e o app abre o seletor de cidade.
- Coordenadas fora do retângulo do Brasil (lat −34 a 6, lng −74 a −28): 400.
- Busca com acento ou sem ("sao jose", "São José"): mesma resposta; cidades homônimas aparecem com UF ("São José · SC", "São José dos Campos · SP").
- GPS negado permanentemente: o app não pede de novo; mostra "Usar minha localização" que leva às configurações do sistema.
- GPS impreciso (> 1 km): o app usa a cidade detectada, mas não exibe bairro.
- BrasilAPI de CEP fora do ar: 503; o app permite preencher endereço manualmente.
- Bairro não determinável pelo ponto: `neighborhood` ausente; o app mostra só a cidade.

## Non-Functional Requirements

- `locate` p95 ≤ 80 ms com índice GiST e cache de 10 min por célula de ~1 km.
- `cities?search` p95 ≤ 50 ms (trigram em `NameNormalized`), cache por termo.
- Nenhuma coordenada do usuário armazenada ou em log.
- Tabela `cities` ≤ 60 MB com polígonos simplificados.

## Integrations

- IBGE: API de localidades (municípios) e API de malhas (limites), só na carga.
- BrasilAPI CEP v2.
- Fuso horário por UF/município a partir de tabela estática (inclui exceções como Fernando de Noronha e Amazonas).

## Acceptance Criteria

- **AC-1** Dado o ponto (-21.2550, -48.3220), quando o app chama `locate`, então a resposta é Jaboticabal, SP, código 3524303, com `precision: "boundary"`.
- **AC-2** Dado um ponto a 2 km da costa no mar, quando chama `locate`, então a resposta traz a cidade litorânea mais próxima com `precision: "nearest"`.
- **AC-3** Dado um ponto fora do Brasil, quando chama `locate`, então a resposta é 400 com erro por campo.
- **AC-4** Dado o termo "sao jose", quando busca cidades, então a lista contém "São José (SC)" e "São José dos Campos (SP)", com no máximo 20 itens.
- **AC-5** Dado o termo com 1 caractere, quando busca cidades, então a resposta é 400.
- **AC-6** Dado um CEP válido, quando um usuário autenticado consulta, então recebe logradouro, bairro e cidade com código IBGE.
- **AC-7** Dado um usuário que negou a localização, quando abre a Início, então vê "Mostrando profissionais de <cidade escolhida>" e consegue trocar a cidade sem conceder permissão.
- **AC-8** Dado um usuário com GPS ativo que escolhe outra cidade, quando volta para a Início, então a busca usa a cidade escolhida até tocar em "Perto de mim".
- **AC-9** Dada qualquer chamada a `locate`, quando os logs são inspecionados, então não há latitude nem longitude registradas.

## Deferred Decisions

| Decisão | Fallback escolhido agora | Revisitar quando |
| --- | --- | --- |
| Bairro por ponto | Bairro do CEP mais próximo via geocodificação do provedor do aparelho; sem bairro se impreciso | Houver base de bairros confiável por cidade |
| Mapa de resultados | Não há mapa | Depois do MVP |

## Open Questions

- Fonte do bairro aproximado pelo ponto: geocodificador do aparelho (gratuito, varia por plataforma) ou serviço pago no servidor?
- Raio padrão de "Perto de mim" quando o cliente não filtra distância: 10 km ou 20 km?
