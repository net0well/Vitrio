# Plano: Localização e cidades

**Status:** Em andamento
**Spec:** [002-localizacao-e-cidades](../specs/002-localizacao-e-cidades.md) (Approved)
**Arquitetura:** Clean Architecture com Controllers ([03-arquitetura.md](../03-arquitetura.md))
**Camadas afetadas:** Domain (`Locations`), Application (`Locations`, endereços de `Accounts`), Infrastructure (tabela `cities`, carga do IBGE, cache), Api (`CitiesController`, `CepsController`, comando `load-cities`), app Flutter.

## Recortes

Todos na branch `feat/localizacao-e-cidades`, em vários commits. O PR só abre quando os dois estiverem prontos e testados de ponta a ponta.

| # | Recorte | Critérios |
| --- | --- | --- |
| 1 | Cidades, `locate`, busca, CEP e carga do IBGE na API | AC-1 a AC-6, AC-9, AC-10, AC-11 |
| 2 | Localização no app: pré-permissão, "Perto de mim", cidade escolhida, recentes e pílula da Início | AC-7, AC-8 |

## Decisões técnicas

| Tema | Decisão |
| --- | --- |
| Cidade | Dado de referência sem regra de negócio: não é agregado. `CityRecord` só na Infrastructure (geografia do NetTopologySuite fica fora do Domain). A Application lê por `ICitySearchableRepository` (localizar, buscar, por código, por nome na UF) e grava pela carga com `ICityWritableRepository`. |
| Texto de busca | `SearchText.Normalize` no Domain (sem acento, minúsculas, espaços únicos), usado na carga, na busca e na comparação de nomes do CEP. Índice trigram GIN em `name_normalized`. |
| Fuso | `BrazilTimeZones.For(uf, nome)` no Domain: fuso por UF, `America/Noronha` para Fernando de Noronha e `America/Eirunepe` para os 13 municípios do oeste do Amazonas. |
| Carga | `LoadCitiesCommand` com `IIbgeCitySource` (localidades e malhas por UF; o GeoJSON é lido à mão para Polygon e MultiPolygon) e `ICityWritableRepository.UpsertAsync`. Centroide pelo ponto interno do polígono (cai sempre dentro da cidade). Ao final, `ICacheTags.RemoveAsync("cities")`. Comando de linha `load-cities` na Api, como o `grant-admin`. |
| `locate` | Ponto dentro do limite (`ST_Covers`), senão o centroide mais perto até 30 km; fora do retângulo do Brasil, 400 por campo. Cache de 10 min por célula de ~1 km (coordenadas arredondadas em 2 casas). Coordenadas nunca em log. |
| Busca e cidade | `cities?search=` com o termo normalizado (`LIKE` com trigram), começo do nome primeiro, até 20 itens; cache de 24 h por termo com a tag `cities` e `Cache-Control: public, max-age=86400`. |
| CEP | `IAddressLookupService.LookupAsync` devolve rua, bairro e cidade (com código IBGE da tabela) ou nada; `GET /ceps/{cep}` usa ele; o cadastro de endereço também. `ICityDirectory` e o cliente do IBGE por requisição saem: a cidade passa a vir da tabela. |
| Testes | A fonte do IBGE é trocada por uma falsa com algumas cidades (polígonos de teste) e a carga roda no início dos testes de integração, o que também cobre o AC-11. AC-9 com um `ILogEventSink` de teste. |

## Recorte 1: API

**Situação:** pronto. Carga real do IBGE: 5.570 municípios em cerca de 20 s, tabela de 5,4 MB; a busca põe as cidades maiores primeiro pela estimativa de população do IBGE; municípios ainda sem limite na malha do IBGE são pulados com aviso.

1. Domain: `SearchText`, `BrazilTimeZones`, erros de cidade.
2. Application: `Locations` (`LocateCity`, `SearchCities`, `GetCity`, `LoadCities`, `LookupCep`), repositories e `IIbgeCitySource`; `AddressLookupService` passa a usar a tabela.
3. Infrastructure: `CityRecord`, configuração (geografia, GIST, trigram), migração `AddCities`, repositories, `IbgeCitySource` com resiliência, `HybridCacheTags`.
4. Api: `CitiesController` (anônimo, política `search`, cache público), `CepsController` (autenticado, política `search`), comando `load-cities`.
5. Testes de integração e de arquitetura; teste manual com a carga real do IBGE.

## Recorte 2: app

**Situação:** pronto. Testado com widgets e no emulador contra a API local com as 5.570 cidades: o GPS acha Jaboticabal, com o bairro dado pelo geocodificador do Android ("Centro, Jaboticabal"); a troca para São Paulo mostra "Mostrando profissionais de São Paulo"; "Usar minha localização" na folha volta para "Perto de mim". A folha mostra carregando e diferencia falta de conexão de busca sem resultado.

1. Pacotes: `geolocator` (permissão e posição) e `geocoding` (geocodificador do Android para o bairro). Permissões `ACCESS_COARSE_LOCATION` e `ACCESS_FINE_LOCATION` no Android.
2. `features/location`:
   - `domain`: `CitySummary`, `LocationMode` (`nearMe`, `chosenCity`) e `LocationState` (modo, cidade, bairro opcional).
   - `data`: `CitiesRepository` (`locate`, `search`) sobre o `ApiClient`; `DeviceLocation` (permissão, posição com precisão, bairro pelo geocodificador); `LocationPreferences` (modo, cidade escolhida e até 5 recentes no aparelho, nunca coordenadas).
   - `presentation`: `LocationController` resolve a cidade ao abrir o app (GPS → `locate`; sem GPS → cidade escolhida); tela de pré-permissão ("Usar minha localização" e "Escolher cidade", com o mesmo peso); folha "Escolher cidade" (busca, recentes, "Usar minha localização"); pílula da Início ("Bairro, Cidade" com GPS preciso, só a cidade se a precisão passar de 1 km, ou a cidade escolhida) e "Mostrando profissionais de <cidade>".
3. Permissão negada para sempre: "Usar minha localização" abre as configurações do sistema.
4. Testes de widget com dublês da localização e da API (AC-7, AC-8, recentes, bairro só com precisão boa) e teste no emulador com a posição simulada.
