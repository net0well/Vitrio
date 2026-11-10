import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:vitrio/core/design_system/widgets/vitrio_text_field.dart';
import 'package:vitrio/core/network/api_result.dart';
import 'package:vitrio/features/auth/domain/me.dart';
import 'package:vitrio/features/location/domain/cep_address.dart';
import 'package:vitrio/features/location/domain/device_permission.dart';
import 'package:vitrio/features/orders/domain/order_action.dart';
import 'package:vitrio/features/orders/domain/order_status.dart';
import 'package:vitrio/features/orders/domain/preferred_period.dart';
import 'package:vitrio/features/orders/domain/service_time.dart';

import '../../support/fake_account_repository.dart';
import '../../support/fake_address_repository.dart';
import '../../support/fake_cities_repository.dart';
import '../../support/fake_device_location.dart';
import '../../support/fake_orders_repository.dart';
import '../../support/fake_provider_repository.dart';
import '../../support/memory_token_store.dart';
import '../../support/pump_vitrio.dart';
import '../../support/sample_account.dart';
import '../../support/sample_listing.dart';
import '../../support/sample_order.dart';
import '../../support/sample_provider.dart';

void main() {
  const withPhone = Me(
    id: '0199a0c4-3f2e-7c1a-9d7e-2a4b6c8d0e1f',
    displayName: 'Mariana Souza',
    initials: 'MS',
    email: 'mariana@exemplo.com.br',
    phoneNumber: '+5516988887777',
    roles: ['customer'],
  );
  final provider = FakeProviderRepository()..published = samplePublicProviderWithServiceIds;

  void phone(WidgetTester tester) {
    tester.view.physicalSize = const Size(1080, 2400);
    tester.view.devicePixelRatio = 2.6;
    addTearDown(tester.view.reset);
  }

  Future<VitrioHarness> signedIn(
    WidgetTester tester, {
    Me me = withPhone,
    FakeOrdersRepository? orders,
    FakeAddressRepository? addresses,
    DateTime? now,
    FakeAccountRepository? account,
    Map<String, Object> preferences = const {'welcome_seen': true},
    FakeDeviceLocation? device,
    FakeCitiesRepository? cities,
  }) {
    phone(tester);
    return pumpVitrio(
      tester,
      preferences: preferences,
      device: device,
      cities: cities,
      account: (account ?? FakeAccountRepository())..meResult = ApiSuccess(me),
      tokens: MemoryTokenStore()..tokens = sampleSession.tokens,
      provider: provider,
      orders: orders,
      addresses: addresses ?? (FakeAddressRepository()..addresses.add(sampleHome)),
      now: now ?? sampleNow,
    );
  }

  Future<void> open(VitrioHarness harness, WidgetTester tester, String location) async {
    harness.go(location);
    await tester.pumpAndSettle();
  }

  Future<void> reveal(WidgetTester tester, String text) async {
    for (final step in [200.0, -200.0]) {
      for (var attempt = 0; attempt < 15 && find.text(text).evaluate().isEmpty; attempt++) {
        await tester.drag(find.byType(Scrollable).first, Offset(0, step));
        await tester.pumpAndSettle();
      }
    }

    await tester.ensureVisible(find.text(text).last);
    await tester.pumpAndSettle();
  }

  Future<void> tapText(WidgetTester tester, String text) async {
    await reveal(tester, text);
    await tester.tap(find.text(text).last);
    await tester.pumpAndSettle();
  }

  Future<void> fill(WidgetTester tester, String nearText, String value) async {
    await reveal(tester, nearText);
    final field = find.descendant(
      of: find.ancestor(of: find.text(nearText).first, matching: find.byType(VitrioTextField)),
      matching: find.byType(TextField),
    );
    await tester.enterText(field, value);
    await tester.pumpAndSettle();
  }

  Future<void> typeCep(WidgetTester tester, String cep) async {
    await tester.enterText(find.widgetWithText(VitrioTextField, 'CEP'), cep);
    await tester.pumpAndSettle();
  }

  String fieldText(WidgetTester tester, String label) =>
      tester.widget<VitrioTextField>(find.widgetWithText(VitrioTextField, label)).controller.text;

  testWidgets('a new address fills the street, the neighborhood and the city from the CEP', (tester) async {
    final addresses = FakeAddressRepository();
    final cities = FakeCitiesRepository()
      ..cepAddress = const CepAddress(cep: '14870180', street: 'Avenida Marechal Deodoro', neighborhood: 'Centro', city: jaboticabalCity);
    final harness = await signedIn(tester, addresses: addresses, cities: cities);
    await open(harness, tester, '/request-quote/${samplePublicProviderWithServiceIds.id}');

    await tapText(tester, 'Adicionar');
    expect(find.text('Digite o CEP para preencher o endereço.'), findsOneWidget);
    await typeCep(tester, '14870180');

    expect(fieldText(tester, 'CEP'), '14870-180');
    expect(fieldText(tester, 'Rua'), 'Avenida Marechal Deodoro');
    expect(fieldText(tester, 'Bairro'), 'Centro');
    expect(fieldText(tester, 'Cidade'), 'Jaboticabal, SP');
    expect(tester.widget<VitrioTextField>(find.widgetWithText(VitrioTextField, 'Rua')).enabled, isFalse);

    await tester.enterText(find.widgetWithText(VitrioTextField, 'Número'), '45');
    await tapText(tester, 'Salvar endereço');

    final added = addresses.added.single;
    expect(added.cep, '14870180');
    expect(added.street, 'Avenida Marechal Deodoro');
    expect(added.neighborhood, 'Centro');
    expect(added.number, '45');
    expect(added.state, 'SP');
    expect(added.cityIbgeCode, jaboticabalCity.ibgeCode);
  });

  testWidgets('a CEP of the whole city asks for the street and the neighborhood', (tester) async {
    final addresses = FakeAddressRepository();
    final cities = FakeCitiesRepository()..cepAddress = const CepAddress(cep: '14870000', city: jaboticabalCity);
    final harness = await signedIn(tester, addresses: addresses, cities: cities);
    await open(harness, tester, '/request-quote/${samplePublicProviderWithServiceIds.id}');

    await tapText(tester, 'Adicionar');
    await typeCep(tester, '14870000');
    expect(tester.widget<VitrioTextField>(find.widgetWithText(VitrioTextField, 'Rua')).enabled, isTrue);
    await tester.enterText(find.widgetWithText(VitrioTextField, 'Número'), '45');
    await tapText(tester, 'Salvar endereço');

    expect(find.text('Informe a rua.'), findsOneWidget);
    expect(find.text('Informe o bairro.'), findsOneWidget);
    expect(addresses.added, isEmpty);

    await tester.enterText(find.widgetWithText(VitrioTextField, 'Rua'), 'Rua Rui Barbosa');
    await tester.enterText(find.widgetWithText(VitrioTextField, 'Bairro'), 'Centro');
    await tapText(tester, 'Salvar endereço');

    expect(addresses.added.single.street, 'Rua Rui Barbosa');
    expect(addresses.added.single.neighborhood, 'Centro');
  });

  testWidgets('a CEP that does not exist says so and keeps the form closed', (tester) async {
    final harness = await signedIn(tester, addresses: FakeAddressRepository(), cities: FakeCitiesRepository()..cepAddress = null);
    await open(harness, tester, '/request-quote/${samplePublicProviderWithServiceIds.id}');

    await tapText(tester, 'Adicionar');
    await typeCep(tester, '99999999');

    expect(find.text('Não encontramos este CEP. Confira os 8 números.'), findsOneWidget);
    expect(find.widgetWithText(VitrioTextField, 'Rua'), findsNothing);
  });

  testWidgets('request quote sends the chosen service, day, period and saved address', (tester) async {
    final orders = FakeOrdersRepository()..createdResult = sampleOrder(status: OrderStatus.sent, withQuote: false, actions: const [OrderAction.cancel]);
    final harness = await signedIn(tester, orders: orders);
    await open(harness, tester, '/request-quote/${samplePublicProviderWithServiceIds.id}');

    expect(find.text('Pedir orçamento'), findsOneWidget);
    await reveal(tester, 'Casa · Centro');
    expect(find.text('Casa · Centro'), findsOneWidget);
    await tapText(tester, 'Troca de chuveiro');
    await fill(tester, 'Ex.: Chuveiro desarmando o disjuntor', 'Chuveiro desarmando o disjuntor');
    await fill(tester, 'Ex.: a tomada da cozinha parou de funcionar depois de uma queda de energia.', 'O chuveiro desarma o disjuntor quando ligo no modo inverno.');
    await tapText(tester, 'Tarde');
    await tapText(tester, 'Enviar pedido');

    final created = orders.created.single;
    expect(created.serviceId, 'service-1');
    expect(created.title, 'Chuveiro desarmando o disjuntor');
    expect(created.preferredDay, DateTime(2026, 10, 29));
    expect(created.preferredPeriod, PreferredPeriod.afternoon);
    expect(created.addressId, sampleHome.id);
    expect(orders.idempotencyKeys.single, isNotEmpty);
    expect(find.text('Seu pedido'), findsOneWidget);
  });

  testWidgets('request quote shows what is missing before sending', (tester) async {
    final orders = FakeOrdersRepository();
    final harness = await signedIn(tester, orders: orders);
    await open(harness, tester, '/request-quote/${samplePublicProviderWithServiceIds.id}');

    await tapText(tester, 'Enviar pedido');

    expect(orders.created, isEmpty);
    await reveal(tester, 'Escolha o serviço.');
    expect(find.text('Escolha o serviço.'), findsOneWidget);
    await reveal(tester, 'Resuma em 5 a 80 caracteres.');
    expect(find.text('Resuma em 5 a 80 caracteres.'), findsOneWidget);
    await reveal(tester, 'Escolha o período.');
    expect(find.text('Escolha o período.'), findsOneWidget);
  });

  testWidgets('request quote asks for the phone when the account has none and saves it first', (tester) async {
    final account = FakeAccountRepository();
    final orders = FakeOrdersRepository()..createdResult = sampleOrder(status: OrderStatus.sent, withQuote: false);
    final harness = await signedIn(tester, me: sampleMe, orders: orders, account: account);
    await open(harness, tester, '/request-quote/${samplePublicProviderWithServiceIds.id}');

    await reveal(tester, 'Seu celular');
    expect(find.text('Seu celular'), findsOneWidget);
    await tapText(tester, 'Outro');
    await fill(tester, 'Ex.: Chuveiro desarmando o disjuntor', 'Chuveiro desarmando o disjuntor');
    await fill(tester, 'Ex.: a tomada da cozinha parou de funcionar depois de uma queda de energia.', 'O chuveiro desarma o disjuntor quando ligo no modo inverno.');
    await tapText(tester, 'Manhã');
    await fill(tester, 'O profissional usa para falar com você depois que você aceitar o orçamento.', '16 99123-4567');
    await tapText(tester, 'Enviar pedido');

    expect(account.updatedPhones, ['(16) 99123-4567']);
    expect(orders.created.single.serviceId, isNull);
  });

  testWidgets('periods of today that already ended cannot be chosen', (tester) async {
    final harness = await signedIn(tester, now: DateTime.utc(2026, 10, 28, 19));
    await open(harness, tester, '/request-quote/${samplePublicProviderWithServiceIds.id}');

    await tapText(tester, 'Hoje');
    await tapText(tester, 'Tarde');

    expect(tester.getSemantics(find.text('Tarde')), isSemantics(isSelected: false, isEnabled: false));
    await tapText(tester, 'Noite');
    expect(tester.getSemantics(find.text('Noite')), isSemantics(isSelected: true));
  });

  testWidgets('a quoted order shows the quote and accepting sends its version', (tester) async {
    final orders = FakeOrdersRepository()..orders[sampleOrderId] = sampleOrder();
    final harness = await signedIn(tester, orders: orders);
    await open(harness, tester, '/orders/$sampleOrderId');

    expect(find.text('Orçamento de Carlos'), findsOneWidget);
    expect(find.textContaining('180,00'), findsOneWidget);
    expect(find.text('Válido até qui 29/10, 13:00'), findsOneWidget);
    expect(find.text('Material incluso (disjuntor 40 A)'), findsOneWidget);
    expect(find.text('Aguardando sua resposta'), findsOneWidget);
    await tapText(tester, 'Aceitar orçamento');

    expect(orders.calls, ['accept:2']);
  });

  testWidgets('a declined order shows the reason and providers to ask instead', (tester) async {
    final orders = FakeOrdersRepository()
      ..orders[sampleOrderId] = sampleOrder(status: OrderStatus.declined, decline: sampleDecline, withQuote: false, actions: const [])
      ..suggested[sampleOrderId] = [sampleListing(id: 'listing-2', displayName: 'Rogério Alves', headline: 'Eletricista')];
    final harness = await signedIn(tester, orders: orders);
    await open(harness, tester, '/orders/$sampleOrderId');

    expect(find.text('Carlos não pode atender agora'), findsOneWidget);
    expect(find.text('Motivo: Sem horário disponível.'), findsOneWidget);
    expect(find.text('Outros eletricistas perto de você'), findsOneWidget);
    expect(find.text('Rogério Alves'), findsOneWidget);
    expect(find.text('Pedir para este'), findsOneWidget);
  });

  testWidgets('cancelling a scheduled order needs a reason and sends it', (tester) async {
    final orders = FakeOrdersRepository()
      ..orders[sampleOrderId] = sampleOrder(
        status: OrderStatus.scheduled,
        actions: const [OrderAction.cancel],
        schedule: ServiceTime(date: DateTime(2026, 10, 29), start: '14:00', end: '15:00', durationMinutes: 60),
      );
    final harness = await signedIn(tester, orders: orders);
    await open(harness, tester, '/orders/$sampleOrderId');

    expect(find.text('Serviço agendado'), findsWidgets);
    expect(find.text('Você poderá marcar como concluído a partir das 14:00.'), findsOneWidget);
    await tapText(tester, 'Cancelar');
    await tapText(tester, 'Cancelar pedido');
    expect(find.text('Escolha um motivo.'), findsOneWidget);
    await tapText(tester, 'Resolvi de outro jeito');
    await tapText(tester, 'Cancelar pedido');

    expect(orders.calls, ['cancel:resolvedAnotherWay:']);
  });

  testWidgets('orders list splits open and closed orders', (tester) async {
    final orders = FakeOrdersRepository()
      ..open.add(sampleSummary())
      ..closed.add(sampleSummary(status: OrderStatus.declined, title: 'Trocar a fiação da garagem'));
    final harness = await signedIn(tester, orders: orders);
    await open(harness, tester, '/orders');

    expect(find.text('Chuveiro desarmando o disjuntor'), findsOneWidget);
    expect(find.text('Orçamento recebido'), findsOneWidget);
    await tapText(tester, 'Finalizados');

    expect(find.text('Trocar a fiação da garagem'), findsOneWidget);
    expect(find.text('Recusado'), findsOneWidget);
  });

  testWidgets('home shows the latest open order', (tester) async {
    final orders = FakeOrdersRepository()..open.add(sampleSummary(status: OrderStatus.scheduled));
    final harness = await signedIn(
      tester,
      orders: orders,
      preferences: const {'welcome_seen': true, 'location_mode': 'nearMe'},
      device: FakeDeviceLocation()..current = DevicePermission.granted,
    );
    await open(harness, tester, '/home');

    final title = find.text('Seu último pedido');
    await tester.scrollUntilVisible(title, 200, scrollable: find.byType(Scrollable).first);
    expect(title, findsOneWidget);
    expect(find.text('Agendado'), findsOneWidget);
  });

  testWidgets('an account without orders is invited to search', (tester) async {
    final harness = await signedIn(tester);
    await open(harness, tester, '/orders');

    expect(find.text('Você ainda não fez nenhum pedido. Que tal começar pela busca?'), findsOneWidget);
    await tapText(tester, 'Buscar profissionais');

    expect(find.text('O que você procura?'), findsOneWidget);
  });
}
