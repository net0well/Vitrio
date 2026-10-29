import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:vitrio/core/network/api_result.dart';
import 'package:vitrio/core/network/network_providers.dart';
import 'package:vitrio/core/session/session_expiry.dart';
import 'package:vitrio/features/auth/data/auth_providers.dart';
import 'package:vitrio/features/auth/presentation/session_controller.dart';

import '../../../support/fake_account_repository.dart';
import '../../../support/fake_auth_repository.dart';
import '../../../support/memory_token_store.dart';
import '../../../support/sample_account.dart';

void main() {
  late MemoryTokenStore tokens;
  late FakeAccountRepository account;
  late FakeAuthRepository auth;

  ProviderContainer container() {
    final created = ProviderContainer(overrides: [
      tokenStoreProvider.overrideWithValue(tokens),
      accountRepositoryProvider.overrideWithValue(account),
      authRepositoryProvider.overrideWithValue(auth),
    ]);
    addTearDown(created.dispose);
    return created;
  }

  setUp(() {
    tokens = MemoryTokenStore();
    account = FakeAccountRepository();
    auth = FakeAuthRepository();
  });

  test('without stored tokens the person is signed out', () async {
    expect(await container().read(sessionControllerProvider.future), isNull);
  });

  test('with stored tokens the account is loaded', () async {
    tokens.tokens = sampleSession.tokens;
    account.meResult = const ApiSuccess(sampleMe);

    expect((await container().read(sessionControllerProvider.future))?.displayName, 'Mariana Souza');
  });

  test('a rejected session forgets the tokens', () async {
    tokens.tokens = sampleSession.tokens;

    expect(await container().read(sessionControllerProvider.future), isNull);
    expect(tokens.tokens, isNull);
  });

  test('starting a session stores the tokens and an expired session signs out', () async {
    final scope = container();
    await scope.read(sessionControllerProvider.future);

    await scope.read(sessionControllerProvider.notifier).start(sampleSession);
    expect(tokens.tokens?.accessToken, 'access-1');
    expect(scope.read(sessionControllerProvider).value, isNotNull);

    scope.read(sessionExpiryProvider.notifier).notify();
    expect(scope.read(sessionControllerProvider).value, isNull);
  });

  test('signing out tells the API and forgets the tokens', () async {
    final scope = container();
    await scope.read(sessionControllerProvider.future);
    await scope.read(sessionControllerProvider.notifier).start(sampleSession);

    await scope.read(sessionControllerProvider.notifier).signOut(allDevices: true);

    expect(auth.calls, contains('logout:true'));
    expect(tokens.tokens, isNull);
    expect(scope.read(sessionControllerProvider).value, isNull);
  });
}
