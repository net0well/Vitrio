import 'dart:convert';
import 'dart:io';
import 'dart:math';

import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';
import 'package:vitrio/app/bootstrap.dart';
import 'package:vitrio/app/flavor.dart';

const _mailpit = String.fromEnvironment('MAILPIT_URL', defaultValue: 'http://10.0.2.2:8025');
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

    await tester.tap(find.text('Conta').last);
    await _pumpUntil(tester, find.byType(ListView));
    if (find.text('Sair').evaluate().isNotEmpty) {
      await tester.tap(find.text('Sair'));
      await _pumpUntil(tester, find.widgetWithText(OutlinedButton, 'Criar conta'));
    }

    await tester.tap(find.widgetWithText(OutlinedButton, 'Criar conta'));
    await _pumpUntil(tester, find.byType(CheckboxListTile));
    await tester.enterText(find.byType(TextField).at(0), _name);
    await tester.enterText(find.byType(TextField).at(1), email);
    await tester.enterText(find.byType(TextField).at(2), _password);
    await tester.tap(find.byType(CheckboxListTile));
    await tester.tap(find.widgetWithText(FilledButton, 'Criar conta'));
    await _pumpUntil(tester, find.textContaining('Enviamos um código de 6 números'));

    await tester.enterText(find.byType(TextField), await _codeFor(email));
    await tester.tap(find.widgetWithText(FilledButton, 'Confirmar'));
    await _pumpUntil(tester, find.text(_name));
    expect(find.text('MF'), findsOneWidget);

    await tester.tap(find.text('Editar dados'));
    await _pumpUntil(tester, find.widgetWithText(FilledButton, 'Salvar'));
    await tester.enterText(find.byType(TextField).at(1), '(16) 99123-4567');
    await tester.tap(find.widgetWithText(FilledButton, 'Salvar'));
    await _pumpUntil(tester, find.textContaining('+5516991234567'));

    await tester.tap(find.text('Aparelhos conectados'));
    await _pumpUntil(tester, find.textContaining('Este aparelho'));
    await tester.binding.handlePopRoute();
    await _pumpUntil(tester, find.text('Sair'));

    await tester.tap(find.text('Sair'));
    await _pumpUntil(tester, find.widgetWithText(FilledButton, 'Entrar'));
    await tester.tap(find.widgetWithText(FilledButton, 'Entrar'));
    await _pumpUntil(tester, find.text('Esqueci a senha'));
    await tester.enterText(find.byType(TextField).at(0), email);
    await tester.enterText(find.byType(TextField).at(1), _password);
    await tester.tap(find.widgetWithText(FilledButton, 'Entrar'));
    await _pumpUntil(tester, find.text(_name));

    await tester.tap(find.text('Excluir conta'));
    await _pumpUntil(tester, find.text('Excluir sua conta?'));
    await tester.enterText(find.byType(TextField), 'EXCLUIR');
    await tester.pump();
    await tester.tap(find.widgetWithText(FilledButton, 'Excluir'));
    await _pumpUntil(tester, find.widgetWithText(OutlinedButton, 'Criar conta'));
    expect(find.text(_name), findsNothing);
  });
}

Future<void> _pumpUntil(WidgetTester tester, Finder finder, {Duration timeout = const Duration(seconds: 30)}) async {
  final deadline = DateTime.now().add(timeout);
  while (DateTime.now().isBefore(deadline)) {
    await tester.pump(const Duration(milliseconds: 200));
    if (finder.evaluate().isNotEmpty) {
      return;
    }
  }

  throw TestFailure('Timed out waiting for $finder');
}

Future<String> _codeFor(String email) async {
  final client = HttpClient();
  try {
    for (var attempt = 0; attempt < 30; attempt++) {
      final request = await client.getUrl(Uri.parse('$_mailpit/api/v1/search?query=${Uri.encodeQueryComponent('to:$email')}'));
      final response = await request.close();
      final body = jsonDecode(await response.transform(utf8.decoder).join()) as Map<String, Object?>;
      for (final message in (body['messages'] as List<Object?>? ?? const []).cast<Map<String, Object?>>()) {
        final code = RegExp(r'^(\d{6})').firstMatch(message['Subject']! as String)?.group(1);
        if (code != null) {
          return code;
        }
      }

      await Future<void>.delayed(const Duration(milliseconds: 500));
    }
  } finally {
    client.close();
  }

  throw StateError('No confirmation code arrived for $email.');
}
