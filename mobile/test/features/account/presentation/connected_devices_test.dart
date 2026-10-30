import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:vitrio/core/network/api_result.dart';
import 'package:vitrio/features/auth/domain/connected_session.dart';

import '../../../support/fake_account_repository.dart';
import '../../../support/memory_token_store.dart';
import '../../../support/pump_vitrio.dart';
import '../../../support/sample_account.dart';

void main() {
  testWidgets('connected devices lists the sessions and disconnects another one', (tester) async {
    final account = FakeAccountRepository()
      ..meResult = const ApiSuccess(sampleMe)
      ..connected = [
        ConnectedSession(id: 'this', deviceName: 'Android', lastSeenAt: DateTime.utc(2026, 10, 29, 17, 5), current: true),
        ConnectedSession(id: 'tablet', deviceName: 'Tablet', lastSeenAt: DateTime.utc(2026, 10, 20, 9), current: false),
      ];
    final tokens = MemoryTokenStore()..tokens = sampleSession.tokens;
    final harness = await pumpVitrio(tester, account: account, tokens: tokens);

    harness.go('/connected-devices');
    await tester.pumpAndSettle();

    expect(find.text('Android · Este aparelho'), findsOneWidget);
    expect(find.textContaining('Último acesso em'), findsNWidgets(2));

    await tester.tap(find.widgetWithText(OutlinedButton, 'Desconectar'));
    await tester.pumpAndSettle();

    expect(account.revoked, ['tablet']);
    expect(find.text('Tablet'), findsNothing);
  });

  testWidgets('signed-in account shows the person and the account actions', (tester) async {
    final account = FakeAccountRepository()..meResult = const ApiSuccess(sampleMe);
    final tokens = MemoryTokenStore()..tokens = sampleSession.tokens;
    final harness = await pumpVitrio(tester, account: account, tokens: tokens);

    harness.go('/account');
    await tester.pumpAndSettle();

    expect(find.text('Mariana Souza'), findsOneWidget);
    expect(find.text('MS'), findsOneWidget);
    expect(find.text('Aparelhos conectados'), findsOneWidget);
    expect(find.text('Excluir conta'), findsOneWidget);
  });
}
