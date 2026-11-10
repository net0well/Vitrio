import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:vitrio/core/design_system/widgets/vitrio_button.dart';
import 'package:vitrio/core/design_system/widgets/vitrio_text_field.dart';
import 'package:vitrio/core/network/api_result.dart';
import 'package:vitrio/features/orders/domain/order_action.dart';
import 'package:vitrio/features/orders/domain/order_party.dart';
import 'package:vitrio/features/orders/domain/order_status.dart';
import 'package:vitrio/features/orders/domain/quote_type.dart';
import 'package:vitrio/features/provider/domain/provider_status.dart';
import 'package:vitrio/features/provider_orders/domain/agenda_item.dart';
import 'package:vitrio/features/provider_orders/domain/agenda_item_kind.dart';
import 'package:vitrio/features/provider_orders/domain/provider_order_tab.dart';

import '../../support/fake_account_repository.dart';
import '../../support/fake_provider_orders_repository.dart';
import '../../support/fake_provider_repository.dart';
import '../../support/memory_token_store.dart';
import '../../support/pump_vitrio.dart';
import '../../support/sample_account.dart';
import '../../support/sample_order.dart';
import '../../support/sample_provider.dart';
import '../../support/sample_provider_order.dart';

void main() {
  FakeProviderRepository publishedProvider() => FakeProviderRepository()
    ..seedComplete()
    ..status = ProviderStatus.published;

  Future<VitrioHarness> asProvider(WidgetTester tester, {FakeProviderOrdersRepository? orders, FakeProviderRepository? provider}) {
    tester.view.physicalSize = const Size(1080, 2400);
    tester.view.devicePixelRatio = 2.6;
    addTearDown(tester.view.reset);
    return pumpVitrio(
      tester,
      preferences: const {'welcome_seen': true, 'location_mode': 'nearMe', 'app_mode': 'provider'},
      account: FakeAccountRepository()..meResult = const ApiSuccess(sampleProviderMe),
      provider: provider ?? publishedProvider(),
      tokens: MemoryTokenStore()..tokens = sampleSession.tokens,
      providerOrders: orders,
      now: sampleNow,
    );
  }

  Future<void> open(VitrioHarness harness, WidgetTester tester, String location) async {
    harness.go(location);
    await tester.pumpAndSettle();
  }

  Future<void> reveal(WidgetTester tester, String text) async {
    for (final step in [-200.0, 200.0]) {
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

  testWidgets('dashboard shows the new order count, the services this month and the latest new orders', (tester) async {
    final provider = publishedProvider()
      ..newOrders = 2
      ..servicesThisMonth = 7
      ..latestNewOrders = [
        sampleProviderOrderSummary(),
        sampleProviderOrderSummary(id: 'second', title: 'Instalar 4 luminárias na sala'),
      ];
    await asProvider(tester, provider: provider);

    expect(find.text('7'), findsOneWidget);
    expect(find.text('Novos pedidos'), findsOneWidget);
    expect(find.text('Ver todos'), findsOneWidget);
    await reveal(tester, 'Novo · Chuveiro');
    expect(find.text('há 5 min'), findsWidgets);
    expect(find.text('Mariana · Centro · 1,8 km'), findsWidgets);
    expect(find.text('Amanhã · tarde (12h–18h)'), findsWidgets);
    expect(find.text('2 fotos anexadas'), findsOneWidget);
    await reveal(tester, 'Instalar 4 luminárias na sala');
    expect(find.text('Chuveiro'), findsOneWidget);
  });

  testWidgets('sending a quote posts the type, amount, day, start, duration, material and message', (tester) async {
    final orders = FakeProviderOrdersRepository()..orders[sampleProviderOrderId] = sampleProviderOrder();
    final harness = await asProvider(tester, orders: orders);
    await open(harness, tester, '/provider-respond/$sampleProviderOrderId');

    expect(find.text('Responder pedido'), findsOneWidget);
    expect(find.text('Mariana A.'), findsOneWidget);
    expect(find.text('No Vitrio desde set/2026 · 3 pedidos'), findsOneWidget);
    expect(find.text('Amanhã · tarde'), findsOneWidget);
    expect(find.text('Centro · 1,8 km'), findsOneWidget);
    expect(find.text('Endereço completo liberado quando Mariana aceitar.'), findsOneWidget);
    expect(find.text('Mariana recebe uma notificação e aceita pelo app.'), findsOneWidget);

    await fill(tester, 'Valor do serviço', '180,00');
    await reveal(tester, 'Das 14:00 às 15:00');
    expect(find.text('Das 14:00 às 15:00'), findsOneWidget);
    await fill(tester, 'Ex.: disjuntor 40 A e conectores', 'Disjuntor 40 A');
    await fill(tester, 'Explique o que está incluso e o que você vai fazer.', 'Troco o disjuntor e reviso a fiação.');
    await tester.tap(find.widgetWithText(VitrioButton, 'Enviar orçamento'));
    await tester.pumpAndSettle();

    final quote = orders.quotes.single;
    expect(quote.type, QuoteType.fixedPrice);
    expect(quote.amount, 180);
    expect(quote.date, DateTime(2026, 10, 29));
    expect(quote.start, '14:00');
    expect(quote.durationMinutes, 60);
    expect(quote.materialIncluded, isTrue);
    expect(quote.materialDetails, 'Disjuntor 40 A');
    expect(quote.message, 'Troco o disjuntor e reviso a fiação.');
    expect(orders.idempotencyKeys.single, isNotEmpty);
  });

  testWidgets('sending a quote without a valid amount shows the amount error and sends nothing', (tester) async {
    final orders = FakeProviderOrdersRepository()..orders[sampleProviderOrderId] = sampleProviderOrder();
    final harness = await asProvider(tester, orders: orders);
    await open(harness, tester, '/provider-respond/$sampleProviderOrderId');

    await tester.tap(find.widgetWithText(VitrioButton, 'Enviar orçamento'));
    await tester.pumpAndSettle();

    await reveal(tester, 'Informe um valor entre R\$ 1,00 e R\$ 100.000,00.');
    expect(find.text('Informe um valor entre R\$ 1,00 e R\$ 100.000,00.'), findsOneWidget);
    expect(orders.quotes, isEmpty);
  });

  testWidgets('choosing a time that crosses a scheduled service warns about the overlap', (tester) async {
    final orders = FakeProviderOrdersRepository()
      ..orders[sampleProviderOrderId] = sampleProviderOrder()
      ..agendaItems.add(
        AgendaItem(
          orderId: 'another',
          kind: AgendaItemKind.scheduled,
          date: DateTime(2026, 10, 29),
          start: '14:30',
          end: '15:30',
          title: 'Troca de tomadas',
          customerName: 'Rafaela S.',
          neighborhood: 'Centro',
        ),
      );
    final harness = await asProvider(tester, orders: orders);
    await open(harness, tester, '/provider-respond/$sampleProviderOrderId');

    await reveal(tester, 'Você já tem um serviço das 14:30 às 15:30 nesse dia.');
    expect(find.text('Você já tem um serviço das 14:30 às 15:30 nesse dia.'), findsOneWidget);
  });

  testWidgets('declining a new order with "Outro" asks for the reason and sends it', (tester) async {
    final orders = FakeProviderOrdersRepository()
      ..orders[sampleProviderOrderId] = sampleProviderOrder(status: OrderStatus.declined, actions: const [])
      ..lists[ProviderOrderTab.fresh]!.add(sampleProviderOrderSummary());
    final harness = await asProvider(tester, orders: orders, provider: publishedProvider()..newOrders = 1);
    await open(harness, tester, '/provider-orders');

    expect(find.text('Novos · 1'), findsOneWidget);
    expect(find.text('Responda até amanhã, 18:00'), findsOneWidget);
    await tester.tap(find.widgetWithText(VitrioButton, 'Recusar'));
    await tester.pumpAndSettle();

    expect(find.text('Recusar o pedido?'), findsOneWidget);
    expect(
      find.text('Mariana vê o motivo junto de “Carlos não pode atender agora” e recebe sugestões de outros profissionais.'),
      findsOneWidget,
    );
    await tapText(tester, 'Outro');
    await tapText(tester, 'Recusar pedido');
    expect(find.text('Conte o motivo em poucas palavras.'), findsOneWidget);
    expect(orders.calls.where((call) => call.startsWith('decline')), isEmpty);

    await fill(tester, 'Mariana vai ler este texto.', 'Esse modelo pede um técnico do fabricante.');
    await tapText(tester, 'Recusar pedido');

    expect(orders.calls, contains('decline:other:Esse modelo pede um técnico do fabricante.'));
    expect(find.text('Você recusou o pedido'), findsOneWidget);
  });

  testWidgets('a scheduled order shows the time, the contact, the address and blocks completion before the start', (tester) async {
    final orders = FakeProviderOrdersRepository()
      ..orders[sampleProviderOrderId] = sampleProviderOrder(
        status: OrderStatus.scheduled,
        actions: const [OrderAction.reschedule, OrderAction.cancel],
        schedule: sampleSchedule,
        quote: sampleQuote,
        released: true,
      );
    final harness = await asProvider(tester, orders: orders);
    await open(harness, tester, '/provider-orders/$sampleProviderOrderId');

    expect(find.text('Serviço agendado'), findsOneWidget);
    expect(find.text('Amanhã, qui 29/10\n14:00–15:00'), findsOneWidget);
    expect(find.textContaining(RegExp(r'^Valor combinado: R\$\s180,00\. Material incluso \(disjuntor 40 A\)\.$')), findsOneWidget);
    expect(find.text('Você pode marcar como concluído a partir das 14:00 de qui 29/10.'), findsOneWidget);
    final complete = tester.widget<VitrioButton>(find.widgetWithText(VitrioButton, 'Serviço concluído'));
    expect(complete.onPressed, isNull);
    await reveal(tester, 'Mariana Alves');
    expect(find.text('(16) 99123-4567'), findsOneWidget);
    expect(find.text('Ligar'), findsOneWidget);
    await reveal(tester, 'Rua Rui Barbosa, 123, apto 12');
    expect(find.text('Centro, Jaboticabal, SP · 14870-000'), findsOneWidget);
    expect(find.text('Endereço e contato visíveis até qui 05/11.'), findsOneWidget);
    await reveal(tester, 'Mariana pediu: amanhã · tarde (12h–18h)');
    expect(find.text('Mariana pediu: amanhã · tarde (12h–18h)'), findsOneWidget);
  });

  testWidgets('cancelling a scheduled order offers the provider reasons', (tester) async {
    final orders = FakeProviderOrdersRepository()
      ..orders[sampleProviderOrderId] = sampleProviderOrder(
        status: OrderStatus.scheduled,
        actions: const [OrderAction.reschedule, OrderAction.cancel],
        schedule: sampleSchedule,
        quote: sampleQuote,
        released: true,
      );
    final harness = await asProvider(tester, orders: orders);
    await open(harness, tester, '/provider-orders/$sampleProviderOrderId');

    await tapText(tester, 'Cancelar pedido');
    expect(find.text('Cancelar o pedido?'), findsOneWidget);
    expect(find.text('Não consigo chegar no horário'), findsOneWidget);
    expect(find.text('O cliente pediu para cancelar'), findsOneWidget);
    expect(find.text('Resolvi de outro jeito'), findsNothing);
    await tapText(tester, 'Imprevisto');
    await tapText(tester, 'Cancelar pedido');

    expect(orders.calls, contains('cancel:unforeseen:'));
  });

  testWidgets('adjusting the time saves the chosen day, start and duration', (tester) async {
    final orders = FakeProviderOrdersRepository()
      ..orders[sampleProviderOrderId] = sampleProviderOrder(
        status: OrderStatus.scheduled,
        actions: const [OrderAction.reschedule, OrderAction.cancel],
        schedule: sampleSchedule,
        quote: sampleQuote,
        released: true,
      );
    final harness = await asProvider(tester, orders: orders);
    await open(harness, tester, '/provider-orders/$sampleProviderOrderId');

    await tapText(tester, 'Ajustar horário');
    expect(find.text('Horário atual'), findsOneWidget);
    expect(find.text('qui 29/10, 14:00–15:00'), findsOneWidget);
    expect(find.text('Você ainda pode ajustar este pedido mais 2 vezes.'), findsOneWidget);
    expect(find.text('Mariana recebe um aviso com o novo horário e pode cancelar se não servir.'), findsOneWidget);
    await tester.tap(find.widgetWithText(VitrioButton, 'Salvar novo horário'));
    await tester.pumpAndSettle();

    expect(orders.calls, contains('reschedule:2026-10-29:14:00:60'));
  });

  testWidgets('the ongoing and closed tabs show the provider statuses', (tester) async {
    final orders = FakeProviderOrdersRepository();
    orders.lists[ProviderOrderTab.ongoing]!.addAll([
      sampleProviderOrderSummary(id: 'scheduled', status: OrderStatus.scheduled, schedule: sampleSchedule),
      sampleProviderOrderSummary(id: 'quoted', status: OrderStatus.quoted, quoteValidUntil: DateTime(2026, 10, 29, 18)),
    ]);
    orders.lists[ProviderOrderTab.closed]!.addAll([
      sampleProviderOrderSummary(id: 'cancelled', status: OrderStatus.cancelled, cancelledBy: OrderParty.customer),
      sampleProviderOrderSummary(id: 'declined', status: OrderStatus.declined),
    ]);
    final harness = await asProvider(tester, orders: orders);
    await open(harness, tester, '/provider-orders');

    expect(find.text('Nenhum pedido novo por enquanto.'), findsOneWidget);
    await tester.tap(find.text('Em andamento'));
    await tester.pumpAndSettle();
    expect(find.text('Agendado · amanhã, 14:00–15:00'), findsOneWidget);
    expect(find.text('Aguardando · válido até qui 29/10, 18:00'), findsOneWidget);

    await tester.tap(find.text('Finalizados'));
    await tester.pumpAndSettle();
    expect(find.text('Cancelado pelo cliente'), findsOneWidget);
    expect(find.text('Recusado por você'), findsOneWidget);
  });
}
