import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';
import 'package:vitrio/app/bootstrap.dart';
import 'package:vitrio/app/flavor.dart';

import 'support/pump_until.dart';

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  testWidgets('finds the city by GPS, switches to a chosen city and back to near me against the local API', (tester) async {
    await bootstrap(Flavor.dev);
    await tester.pumpAndSettle();
    if (find.text('Encontrar um profissional').evaluate().isNotEmpty) {
      await tester.tap(find.text('Encontrar um profissional'));
      await tester.pumpAndSettle();
    }

    await pumpUntil(tester, find.text('Para mostrar quem atende perto de você'));
    await tester.tap(find.widgetWithText(FilledButton, 'Usar minha localização'));
    await pumpUntil(tester, find.textContaining('Jaboticabal'));
    expect(find.textContaining('Mostrando profissionais'), findsNothing);

    await tester.tap(find.textContaining('Jaboticabal'));
    final cityField = find.descendant(of: find.byType(BottomSheet), matching: find.byType(TextField));
    await pumpUntil(tester, cityField);
    await tester.enterText(cityField, 'sao paulo');
    await pumpUntil(tester, find.text('São Paulo · SP'));
    await tester.tap(find.text('São Paulo · SP'));
    await pumpUntil(tester, find.text('Mostrando profissionais de São Paulo, SP.'));

    await tester.tap(find.text('São Paulo'));
    await pumpUntil(tester, find.text('Usar minha localização'));
    await tester.tap(find.text('Usar minha localização'));
    await pumpUntil(tester, find.textContaining('Jaboticabal'));
    expect(find.textContaining('Mostrando profissionais'), findsNothing);
  });
}
