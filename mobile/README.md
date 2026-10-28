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

Os arquivos gerados (`*.g.dart` e `lib/l10n/app_localizations*.dart`) ficam no repositório; rode o `build_runner` de novo ao mudar providers ou rotas.

Os ícones do app saem de `assets/launcher/` com `dart run flutter_launcher_icons`.

## Estrutura

Segue `docs/03-arquitetura.md` §9: `app/` (bootstrap, rotas, tema), `core/` (design system, rede, sessão, armazenamento) e `features/<nome>/{data,domain,presentation}`. Tokens e widgets seguem `docs/design-system/flutter.md`.
