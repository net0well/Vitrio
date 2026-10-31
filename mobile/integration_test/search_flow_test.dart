import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';
import 'package:vitrio/app/bootstrap.dart';
import 'package:vitrio/app/flavor.dart';
import 'package:vitrio/core/design_system/widgets/vitrio_provider_card.dart';

import 'support/pump_until.dart';

const _email = 'teste@exemplo.com.br';
const _password = 'senha de teste vitrio';
const _provider = 'Carlos Pereira Lima';

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  final card = find.descendant(of: find.byType(VitrioProviderCard), matching: find.text(_provider));

  testWidgets('a visitor searches, saves a provider after signing in and finds it in favorites against the local API', (tester) async {
    await bootstrap(Flavor.dev);
    await tester.pumpAndSettle();
    if (find.text('Encontrar um profissional').evaluate().isNotEmpty) {
      await tester.tap(find.text('Encontrar um profissional'));
      await tester.pumpAndSettle();
    }

    if (find.text('Para mostrar quem atende perto de você').evaluate().isNotEmpty) {
      await tester.tap(find.widgetWithText(OutlinedButton, 'Escolher cidade'));
      await pumpUntil(tester, find.byType(TextField));
      await tester.enterText(find.byType(TextField), 'sao paulo');
      await pumpUntil(tester, find.text('São Paulo · SP'));
      await tester.tap(find.text('São Paulo · SP'));
      await pumpUntil(tester, find.text('Mostrando profissionais de São Paulo, SP.'));
    }

    await tester.tap(find.text('Conta').last);
    await pumpUntil(tester, find.byType(ListView));
    if (find.text('Sair').evaluate().isNotEmpty) {
      await tester.tap(find.text('Sair'));
      await pumpUntil(tester, find.widgetWithText(OutlinedButton, 'Criar conta'));
    }

    await tester.tap(find.text('Início').last);
    await pumpUntil(tester, find.text('Do que você precisa hoje?'));
    await tester.enterText(find.byType(TextField).first, _provider);
    await tester.testTextInput.receiveAction(TextInputAction.search);
    await pumpUntil(tester, card);
    await tester.tap(card.first);
    await pumpUntil(tester, find.byTooltip('Salvar nos favoritos'));

    await tester.tap(find.byTooltip('Salvar nos favoritos'));
    await pumpUntil(tester, find.widgetWithText(FilledButton, 'Entrar'));
    await tester.enterText(find.byType(TextField).at(0), _email);
    await tester.enterText(find.byType(TextField).at(1), _password);
    await tester.tap(find.widgetWithText(FilledButton, 'Entrar'));
    await pumpUntil(tester, find.byTooltip('Remover dos favoritos'));
    expect(find.text(_provider), findsOneWidget);

    await tester.tap(find.byTooltip('Voltar'));
    await pumpUntil(tester, find.text('Do que você precisa hoje?'));
    await tester.tap(find.text('Conta').last);
    await pumpUntil(tester, find.text('Favoritos'));
    await tester.tap(find.text('Favoritos'));
    await pumpUntil(tester, card);

    await tester.tap(card.first);
    await pumpUntil(tester, find.byTooltip('Remover dos favoritos'));
    await tester.tap(find.byTooltip('Remover dos favoritos'));
    await pumpUntil(tester, find.byTooltip('Salvar nos favoritos'));
    await tester.tap(find.byTooltip('Voltar'));
    await pumpUntil(tester, card, absent: true);
  });
}
