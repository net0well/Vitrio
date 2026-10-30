import 'dart:math';

import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';
import 'package:vitrio/app/bootstrap.dart';
import 'package:vitrio/app/flavor.dart';

import 'support/mailpit.dart';
import 'support/pump_until.dart';

const _password = 'vidro azul na janela';
const _name = 'Mariana Ferreira Souza';

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  testWidgets('creates, confirms, edits, signs out, signs in and deletes an account against the local API', (tester) async {
    final email = 'app-${Random().nextInt(1 << 30)}@exemplo.com.br';
    await bootstrap(Flavor.dev);
    await tester.pumpAndSettle();

    if (find.text('Encontrar um profissional').evaluate().isNotEmpty) {
      await tester.tap(find.text('Encontrar um profissional'));
      await tester.pumpAndSettle();
    }

    if (find.text('Para mostrar quem atende perto de você').evaluate().isNotEmpty) {
      await tester.tap(find.widgetWithText(OutlinedButton, 'Escolher cidade'));
      await pumpUntil(tester, find.byType(TextField));
      await tester.enterText(find.byType(TextField), 'jaboticabal');
      await pumpUntil(tester, find.text('Jaboticabal · SP'));
      await tester.tap(find.text('Jaboticabal · SP'));
      await pumpUntil(tester, find.text('Mostrando profissionais de Jaboticabal, SP.'));
    }

    await tester.tap(find.text('Conta').last);
    await pumpUntil(tester, find.byType(ListView));
    if (find.text('Sair').evaluate().isNotEmpty) {
      await tester.tap(find.text('Sair'));
      await pumpUntil(tester, find.widgetWithText(OutlinedButton, 'Criar conta'));
    }

    await tester.tap(find.widgetWithText(OutlinedButton, 'Criar conta'));
    await pumpUntil(tester, find.byType(CheckboxListTile));
    await tester.enterText(find.byType(TextField).at(0), _name);
    await tester.enterText(find.byType(TextField).at(1), email);
    await tester.enterText(find.byType(TextField).at(2), _password);
    await tester.tap(find.byType(CheckboxListTile));
    await tester.tap(find.widgetWithText(FilledButton, 'Criar conta'));
    await pumpUntil(tester, find.textContaining('Enviamos um código de 6 números'));

    await tester.enterText(find.byType(TextField), await confirmationCodeFor(email));
    await tester.tap(find.widgetWithText(FilledButton, 'Confirmar'));
    await pumpUntil(tester, find.text(_name));
    expect(find.text('MF'), findsOneWidget);

    await tester.tap(find.text('Editar dados'));
    await pumpUntil(tester, find.widgetWithText(FilledButton, 'Salvar'));
    await tester.enterText(find.byType(TextField).at(1), '(16) 99123-4567');
    await tester.tap(find.widgetWithText(FilledButton, 'Salvar'));
    await pumpUntil(tester, find.textContaining('+5516991234567'));

    await tester.tap(find.text('Aparelhos conectados'));
    await pumpUntil(tester, find.textContaining('Este aparelho'));
    await tester.binding.handlePopRoute();
    await pumpUntil(tester, find.text('Sair'));

    await tester.tap(find.text('Sair'));
    await pumpUntil(tester, find.widgetWithText(FilledButton, 'Entrar'));
    await tester.tap(find.widgetWithText(FilledButton, 'Entrar'));
    await pumpUntil(tester, find.text('Esqueci a senha'));
    await tester.enterText(find.byType(TextField).at(0), email);
    await tester.enterText(find.byType(TextField).at(1), _password);
    await tester.tap(find.widgetWithText(FilledButton, 'Entrar'));
    await pumpUntil(tester, find.text(_name));

    await tester.tap(find.text('Excluir conta'));
    await pumpUntil(tester, find.text('Excluir sua conta?'));
    await tester.enterText(find.byType(TextField), 'EXCLUIR');
    await tester.pump();
    await tester.tap(find.widgetWithText(FilledButton, 'Excluir'));
    await pumpUntil(tester, find.widgetWithText(OutlinedButton, 'Criar conta'));
    expect(find.text(_name), findsNothing);
  });
}
