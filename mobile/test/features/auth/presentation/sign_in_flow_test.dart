import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:vitrio/core/network/api_result.dart';
import 'package:vitrio/core/network/problem_details.dart';
import 'package:vitrio/features/auth/domain/auth_session.dart';

import '../../../support/fake_auth_repository.dart';
import '../../../support/pump_vitrio.dart';
import '../../../support/sample_account.dart';

void main() {
  Future<VitrioHarness> openSignIn(WidgetTester tester, FakeAuthRepository auth) async {
    final harness = await pumpVitrio(tester, auth: auth);
    harness.go('/sign-in');
    await tester.pumpAndSettle();
    await tester.enterText(find.byType(TextField).at(0), 'mariana@exemplo.com.br');
    await tester.enterText(find.byType(TextField).at(1), 'vidro azul na janela');
    return harness;
  }

  testWidgets('signing in opens the account with the person name', (tester) async {
    final auth = FakeAuthRepository()..loginResult = const ApiSuccess(sampleSession);
    final harness = await openSignIn(tester, auth);

    await tester.tap(find.widgetWithText(FilledButton, 'Entrar'));
    await tester.pumpAndSettle();

    expect(find.text('Mariana Souza'), findsOneWidget);
    expect(harness.tokens.tokens?.refreshToken, 'refresh-1');
  });

  testWidgets('wrong credentials show the message from the API', (tester) async {
    final auth = FakeAuthRepository()
      ..loginResult = const ApiFailure<AuthSession>(
        ProblemDetails(status: 401, code: 'auth.invalid_credentials', detail: 'E-mail ou senha incorretos.'),
      );
    await openSignIn(tester, auth);

    await tester.tap(find.widgetWithText(FilledButton, 'Entrar'));
    await tester.pumpAndSettle();

    expect(find.text('E-mail ou senha incorretos.'), findsOneWidget);
  });

  testWidgets('an unconfirmed e-mail sends a new code and opens the code screen', (tester) async {
    final auth = FakeAuthRepository()
      ..loginResult = const ApiFailure<AuthSession>(ProblemDetails(status: 409, code: 'account.email_not_verified'))
      ..verifyResult = const ApiSuccess(sampleSession);
    final harness = await openSignIn(tester, auth);

    await tester.tap(find.widgetWithText(FilledButton, 'Entrar'));
    await tester.pumpAndSettle();

    expect(auth.calls, contains('resend:mariana@exemplo.com.br:verifyEmail'));
    expect(find.textContaining('Enviamos um código de 6 números para mariana@exemplo.com.br'), findsOneWidget);

    await tester.enterText(find.byType(TextField), '482917');
    await tester.tap(find.widgetWithText(FilledButton, 'Confirmar'));
    await tester.pumpAndSettle();

    expect(auth.lastVerifiedPassword, 'vidro azul na janela');
    expect(find.text('Mariana Souza'), findsOneWidget);
    expect(harness.tokens.tokens?.accessToken, 'access-1');
  });

  testWidgets('protected account screens send signed-out people to sign in', (tester) async {
    final harness = await pumpVitrio(tester);

    harness.go('/connected-devices');
    await tester.pumpAndSettle();

    expect(find.widgetWithText(FilledButton, 'Entrar'), findsOneWidget);
  });
}
