import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:vitrio/app/app.dart';
import 'package:vitrio/app/app_config.dart';
import 'package:vitrio/app/flavor.dart';
import 'package:vitrio/app/router.dart';
import 'package:vitrio/core/network/network_providers.dart';
import 'package:vitrio/core/storage/preferences.dart';
import 'package:vitrio/features/auth/data/auth_providers.dart';

import 'fake_account_repository.dart';
import 'fake_auth_repository.dart';
import 'memory_token_store.dart';

final class VitrioHarness {
  VitrioHarness({required this.container, required this.tokens, required this.auth, required this.account});

  final ProviderContainer container;
  final MemoryTokenStore tokens;
  final FakeAuthRepository auth;
  final FakeAccountRepository account;

  void go(String location) => container.read(routerProvider).go(location);
}

Future<VitrioHarness> pumpVitrio(
  WidgetTester tester, {
  Map<String, Object> preferences = const {'welcome_seen': true},
  FakeAuthRepository? auth,
  FakeAccountRepository? account,
  MemoryTokenStore? tokens,
}) async {
  SharedPreferences.setMockInitialValues(preferences);
  final instance = await SharedPreferences.getInstance();
  final harnessTokens = tokens ?? MemoryTokenStore();
  final harnessAuth = auth ?? FakeAuthRepository();
  final harnessAccount = account ?? FakeAccountRepository();
  final container = ProviderContainer(
    overrides: [
      appConfigProvider.overrideWithValue(AppConfig.forFlavor(Flavor.dev)),
      preferencesProvider.overrideWithValue(instance),
      tokenStoreProvider.overrideWithValue(harnessTokens),
      authRepositoryProvider.overrideWithValue(harnessAuth),
      accountRepositoryProvider.overrideWithValue(harnessAccount),
    ],
  );
  addTearDown(container.dispose);

  await tester.pumpWidget(UncontrolledProviderScope(container: container, child: const VitrioApp()));
  await tester.pumpAndSettle();
  return VitrioHarness(container: container, tokens: harnessTokens, auth: harnessAuth, account: harnessAccount);
}
