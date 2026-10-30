import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:vitrio/core/design_system/widgets/vitrio_text_field.dart';
import 'package:vitrio/core/network/api_result.dart';
import 'package:vitrio/features/auth/domain/me.dart';
import 'package:vitrio/features/provider/domain/provider_status.dart';

import '../../support/fake_account_repository.dart';
import '../../support/fake_provider_repository.dart';
import '../../support/memory_token_store.dart';
import '../../support/pump_vitrio.dart';
import '../../support/sample_account.dart';
import '../../support/sample_provider.dart';

void main() {
  const signedInClient = {'welcome_seen': true, 'location_mode': 'nearMe'};

  Future<void> fill(WidgetTester tester, String label, String text) async {
    final field = find.widgetWithText(VitrioTextField, label);
    await tester.scrollUntilVisible(field, 200, scrollable: find.byType(Scrollable).first);
    await tester.enterText(field, text);
  }

  Future<void> tapVisible(WidgetTester tester, Finder finder) async {
    await tester.scrollUntilVisible(finder, 200, scrollable: find.byType(Scrollable).first);
    await tester.tap(finder);
    await tester.pumpAndSettle();
  }

  testWidgets('offering services walks through the four steps and publishes the showcase', (tester) async {
    tester.view.physicalSize = const Size(1080, 2400);
    tester.view.devicePixelRatio = 2.6;
    addTearDown(tester.view.reset);
    final account = FakeAccountRepository()..meResult = const ApiSuccess(sampleMe);
    final provider = FakeProviderRepository();
    provider.onCreated = () => account.meResult = const ApiSuccess(Me(
          id: '0199a0c4-3f2e-7c1a-9d7e-2a4b6c8d0e1f',
          displayName: 'Mariana Souza',
          initials: 'MS',
          email: 'mariana@exemplo.com.br',
          phoneNumber: null,
          roles: ['customer', 'provider'],
          hasProviderProfile: true,
        ));
    final harness = await pumpVitrio(
      tester,
      preferences: signedInClient,
      account: account,
      provider: provider,
      tokens: MemoryTokenStore()..tokens = sampleSession.tokens,
    );
    harness.go('/account');
    await tester.pumpAndSettle();

    await tester.tap(find.text('Quero oferecer meus serviços'));
    await tester.pumpAndSettle();
    expect(find.text('Passo 1 de 4'), findsOneWidget);
    await fill(tester, 'Profissão', 'Diarista');
    await fill(tester, 'Sobre você e o seu trabalho', 'Faço faxina residencial há oito anos, com produtos próprios e muito capricho.');
    await tester.tap(find.text('Continuar'));
    await tester.pumpAndSettle();

    expect(provider.calls, containsAllInOrder(['create', 'update']));
    expect(find.text('Passo 2 de 4'), findsOneWidget);
    await tester.tap(find.text('Elétrica'));
    await tester.pumpAndSettle();
    await fill(tester, 'CEP do endereço base', '14870-000');
    await fill(tester, 'Número', '120');
    await tester.tap(find.text('Continuar'));
    await tester.pumpAndSettle();

    expect(provider.categoryIds, [1]);
    expect(provider.area?.number, '120');
    expect(find.text('Passo 3 de 4'), findsOneWidget);
    await tapVisible(tester, find.text('Adicionar serviço'));
    await tester.enterText(find.widgetWithText(VitrioTextField, 'Nome do serviço'), 'Faxina completa');
    await tester.enterText(find.widgetWithText(VitrioTextField, 'A partir de'), '150');
    await tester.tap(find.widgetWithText(FilledButton, 'Salvar'));
    await tester.pumpAndSettle();
    expect(find.text('R\$ 150'), findsOneWidget);
    await tester.tap(find.text('Continuar'));
    await tester.pumpAndSettle();

    expect(find.text('Passo 4 de 4'), findsOneWidget);
    expect(find.text('Tudo pronto para publicar.'), findsOneWidget);
    await tester.tap(find.widgetWithText(FilledButton, 'Publicar'));
    await tester.pumpAndSettle();

    expect(provider.calls.last, 'publish');
    expect(find.text('Painel'), findsWidgets);
    expect(find.text('Novos pedidos'), findsOneWidget);
  });

  testWidgets('review lists what is missing and keeps publish disabled', (tester) async {
    final provider = FakeProviderRepository()
      ..seedComplete()
      ..about = null
      ..services = [];
    final harness = await pumpVitrio(
      tester,
      preferences: signedInClient,
      account: FakeAccountRepository()..meResult = const ApiSuccess(sampleProviderMe),
      provider: provider,
      tokens: MemoryTokenStore()..tokens = sampleSession.tokens,
    );

    harness.go('/provider-edit/review');
    await tester.pumpAndSettle();

    expect(find.text('Para publicar, falta:'), findsOneWidget);
    expect(find.text('• o texto sobre você'), findsOneWidget);
    expect(find.text('• um serviço'), findsOneWidget);
    expect(tester.widget<FilledButton>(find.widgetWithText(FilledButton, 'Publicar')).onPressed, isNull);
  });

  testWidgets('a provider switches between the provider and the client modes from the account', (tester) async {
    final provider = FakeProviderRepository()
      ..seedComplete()
      ..status = ProviderStatus.published;
    final harness = await pumpVitrio(
      tester,
      preferences: signedInClient,
      account: FakeAccountRepository()..meResult = const ApiSuccess(sampleProviderMe),
      provider: provider,
      tokens: MemoryTokenStore()..tokens = sampleSession.tokens,
    );
    harness.go('/account');
    await tester.pumpAndSettle();

    await tester.tap(find.text('Entrar como prestador'));
    await tester.pumpAndSettle();
    expect(find.text('83%'), findsOneWidget);
    expect(find.text('Agenda'), findsOneWidget);

    await tester.tap(find.text('Meu perfil'));
    await tester.pumpAndSettle();
    await tester.scrollUntilVisible(find.text('Conta'), 200, scrollable: find.byType(Scrollable).first);
    await tester.tap(find.text('Conta'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('Voltar para cliente'));
    await tester.pumpAndSettle();
    expect(find.text('Início'), findsOneWidget);
  });

  testWidgets('public profile shows the badges and the WhatsApp button without any address', (tester) async {
    final provider = FakeProviderRepository()..published = samplePublicProvider;
    final harness = await pumpVitrio(tester, preferences: signedInClient, provider: provider);

    harness.go('/providers/${samplePublicProvider.id}');
    await tester.pumpAndSettle();

    expect(find.text('Carlos Eletricista'), findsOneWidget);
    expect(find.text('MEI verificado'), findsOneWidget);
    expect(find.text('Atende hoje'), findsOneWidget);
    expect(find.text('Novo no Vitrio'), findsWidgets);
    expect(find.text('Até 15 km de Jaboticabal, SP'), findsOneWidget);
    expect(find.text('R\$ 80'), findsOneWidget);
    expect(find.text('WhatsApp'), findsOneWidget);
  });

  testWidgets('a shared link opens the public profile', (tester) async {
    final provider = FakeProviderRepository()..published = samplePublicProvider;
    final harness = await pumpVitrio(tester, preferences: signedInClient, provider: provider);

    harness.go('/p/${samplePublicProvider.id}');
    await tester.pumpAndSettle();

    expect(find.text('Carlos Eletricista'), findsOneWidget);
  });

  testWidgets('public profile of a showcase that is not published says it is not available', (tester) async {
    final harness = await pumpVitrio(tester, preferences: signedInClient);

    harness.go('/providers/unknown');
    await tester.pumpAndSettle();

    expect(find.text('Este perfil não está disponível.'), findsOneWidget);
  });
}
