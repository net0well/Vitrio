import 'dart:math';

import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';
import 'package:vitrio/app/bootstrap.dart';
import 'package:vitrio/app/flavor.dart';
import 'package:vitrio/core/design_system/widgets/vitrio_switch_row.dart';
import 'package:vitrio/core/design_system/widgets/vitrio_text_field.dart';
import 'package:vitrio/features/provider/data/provider_providers.dart';

import 'support/generated_photo_picker.dart';
import 'support/mailpit.dart';
import 'support/pump_until.dart';

const _password = 'vidro azul na janela';
const _name = 'Carlos Pereira Lima';

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  Future<void> fill(WidgetTester tester, String label, String text) async {
    final field = find.widgetWithText(VitrioTextField, label);
    await tester.scrollUntilVisible(field, 200, scrollable: find.byType(Scrollable).first);
    await tester.enterText(field, text);
    await tester.pump();
  }

  testWidgets('a new provider builds the showcase with a photo, publishes it and sees the public profile against the local API', (tester) async {
    final email = 'vitrine-${Random().nextInt(1 << 30)}@exemplo.com.br';
    await bootstrap(Flavor.dev, overrides: [photoPickerProvider.overrideWithValue(GeneratedPhotoPicker())]);
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
    await pumpUntil(tester, find.text('Quero oferecer meus serviços'));

    await tester.tap(find.text('Quero oferecer meus serviços'));
    await pumpUntil(tester, find.text('Passo 1 de 4'));
    await fill(tester, 'Profissão', 'Eletricista');
    await fill(tester, 'Sobre você e o seu trabalho', 'Eletricista há 12 anos, faço instalações e reparos residenciais com garantia.');
    await tester.tap(find.text('Continuar'));
    await pumpUntil(tester, find.text('Passo 2 de 4'));

    await pumpUntil(tester, find.text('Elétrica'));
    await tester.tap(find.text('Elétrica'));
    await fill(tester, 'CEP do endereço base', '01001-000');
    await fill(tester, 'Número', '100');
    await fill(tester, '(16) 9 0000-0000', '(16) 99123-4567');
    final whatsApp = find.byType(VitrioSwitchRow);
    await tester.scrollUntilVisible(whatsApp, 200, scrollable: find.byType(Scrollable).first);
    await tester.ensureVisible(whatsApp);
    await tester.pumpAndSettle();
    await tester.tap(whatsApp);
    await tester.pumpAndSettle();
    expect(tester.widget<VitrioSwitchRow>(whatsApp).value, isTrue);
    await tester.tap(find.text('Continuar'));
    await pumpUntil(tester, find.text('Passo 3 de 4'));

    await tester.tap(find.bySemanticsLabel('Adicionar foto'));
    await pumpUntil(tester, find.text('Processando'));
    await pumpUntil(tester, find.text('Processando'), absent: true);
    final addService = find.text('Adicionar serviço');
    await tester.scrollUntilVisible(addService, 200, scrollable: find.byType(Scrollable).first);
    await tester.tap(addService);
    await pumpUntil(tester, find.widgetWithText(VitrioTextField, 'Nome do serviço'));
    await tester.enterText(find.widgetWithText(VitrioTextField, 'Nome do serviço'), 'Troca de tomada');
    await tester.enterText(find.widgetWithText(VitrioTextField, 'A partir de'), '50');
    await tester.tap(find.widgetWithText(FilledButton, 'Salvar'));
    await pumpUntil(tester, find.text('Troca de tomada'));
    await tester.tap(find.text('Continuar'));
    await pumpUntil(tester, find.text('Tudo pronto para publicar.'));

    await tester.tap(find.widgetWithText(FilledButton, 'Publicar'));
    await pumpUntil(tester, find.text('Novos pedidos'));
    await tester.tap(find.text('Meu perfil'));
    await pumpUntil(tester, find.text('Ver como os clientes veem'));
    await tester.tap(find.text('Ver como os clientes veem'));
    await pumpUntil(tester, find.text('Trabalhos'));
    expect(find.text(_name), findsOneWidget);
    expect(find.text('Novo no Vitrio'), findsWidgets);
    expect(find.text('WhatsApp'), findsOneWidget);
  });
}
