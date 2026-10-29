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

`integration_test/` roda o app de verdade num emulador Android contra a API local, com as cidades carregadas (`load-cities`):

- `account_flow_test.dart`: escolhe a cidade, cria a conta, confirma o e-mail com o código lido no Mailpit, edita o celular, abre os aparelhos conectados, sai, entra de novo e exclui a conta.
- `location_flow_test.dart`: acha a cidade pelo GPS, troca para uma cidade escolhida e volta para "Perto de mim".

```bash
docker compose -f ../infra/docker-compose.yml up -d postgres redis mailpit
# na pasta backend, com as migrações aplicadas:
ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://localhost:5080 dotnet run --project src/Vitrio.Api --no-launch-profile
# na pasta mobile, com o emulador aberto:
flutter test integration_test --flavor dev -d emulator-5554
```

O emulador chega à API e ao Mailpit por `10.0.2.2`. O teste não roda no CI.

O teste de localização precisa da permissão concedida e de uma posição em Jaboticabal. O `flutter test` reinstala o app a cada arquivo, e o Google Play Services só aceita posição nova; por isso a permissão e a posição vão num laço, em outro terminal, enquanto o teste roda:

```bash
adb shell appops set com.android.shell android:mock_location allow
for p in gps network; do
  adb shell cmd location providers add-test-provider $p
  adb shell cmd location providers set-test-provider-enabled $p true
done
while true; do
  adb shell "pm grant app.vitrio.dev android.permission.ACCESS_FINE_LOCATION; cmd location providers set-test-provider-location gps --location -21.2546,-48.3225 --accuracy 20; cmd location providers set-test-provider-location network --location -21.2546,-48.3225 --accuracy 20" >/dev/null 2>&1
  sleep 0.5
done
```

Depois, `adb shell cmd location providers remove-test-provider gps` (e `network`) devolve a localização normal do emulador.

Os arquivos gerados (`*.g.dart` e `lib/l10n/app_localizations*.dart`) ficam no repositório; rode o `build_runner` de novo ao mudar providers ou rotas.

Os ícones do app saem de `assets/launcher/` com `dart run flutter_launcher_icons`.

## Estrutura

Segue `docs/03-arquitetura.md` §9: `app/` (bootstrap, rotas, tema), `core/` (design system, rede, sessão, armazenamento) e `features/<nome>/{data,domain,presentation}`. Tokens e widgets seguem `docs/design-system/flutter.md`.
