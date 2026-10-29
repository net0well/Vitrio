# Vitrio (app)

App Flutter do Vitrio. O primeiro lançamento é só para Android; o projeto iOS existe para o lançamento seguinte e ainda não tem flavors nativos.

## Ambientes

| Flavor | Entrada | API padrão | Id do app |
| --- | --- | --- | --- |
| `dev` | `lib/main_dev.dart` | `http://10.0.2.2:5080` no emulador Android, `http://localhost:5080` nos demais | `app.vitrio.dev` |
| `staging` | `lib/main_staging.dart` | `https://api-staging.vitrio.app` | `app.vitrio.staging` |
| `prod` | `lib/main_prod.dart` | `https://api.vitrio.app` | `app.vitrio` |

Para apontar para outra API: `--dart-define=API_BASE_URL=http://192.168.0.10:5080`.

## Comandos

```bash
flutter pub get
dart run build_runner build      # providers do Riverpod e rotas tipadas (*.g.dart)
flutter gen-l10n                 # textos de lib/l10n/app_pt.arb
flutter run --flavor dev -t lib/main_dev.dart
flutter analyze
flutter test
flutter build appbundle --flavor prod -t lib/main_prod.dart --obfuscate --split-debug-info=build/symbols
```

## Teste de ponta a ponta no emulador

`integration_test/` roda o app de verdade num emulador Android contra a API local: cria a conta, confirma o e-mail com o código lido no Mailpit, edita o celular, abre os aparelhos conectados, sai, entra de novo e exclui a conta.

```bash
docker compose -f ../infra/docker-compose.yml up -d postgres redis mailpit
# na pasta backend, com as migrações aplicadas:
ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://localhost:5080 dotnet run --project src/Vitrio.Api --no-launch-profile
# na pasta mobile, com o emulador aberto:
flutter test integration_test --flavor dev -d emulator-5554
```

O emulador chega à API e ao Mailpit por `10.0.2.2`. O teste não roda no CI.

Os arquivos gerados (`*.g.dart` e `lib/l10n/app_localizations*.dart`) ficam no repositório; rode o `build_runner` de novo ao mudar providers ou rotas.

Os ícones do app saem de `assets/launcher/` com `dart run flutter_launcher_icons`.

## Estrutura

Segue `docs/03-arquitetura.md` §9: `app/` (bootstrap, rotas, tema), `core/` (design system, rede, sessão, armazenamento) e `features/<nome>/{data,domain,presentation}`. Tokens e widgets seguem `docs/design-system/flutter.md`.
