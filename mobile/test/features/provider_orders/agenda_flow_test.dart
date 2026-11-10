import 'package:flutter_test/flutter_test.dart';
import 'package:vitrio/features/orders/domain/order_action.dart';
import 'package:vitrio/features/orders/domain/order_status.dart';
import 'package:vitrio/features/provider_orders/domain/agenda_item.dart';
import 'package:vitrio/features/provider_orders/domain/agenda_item_kind.dart';

import '../../support/fake_provider_orders_repository.dart';
import '../../support/pump_provider_mode.dart';
import '../../support/pump_vitrio.dart';
import '../../support/sample_order.dart';
import '../../support/sample_provider_order.dart';

void main() {
  AgendaItem item(
    String orderId,
    AgendaItemKind kind,
    DateTime date,
    String start,
    String end,
    String title, {
    DateTime? quoteValidUntil,
  }) => AgendaItem(
    orderId: orderId,
    kind: kind,
    date: date,
    start: start,
    end: end,
    title: title,
    customerName: 'Rafaela S.',
    neighborhood: 'Centro',
    quoteValidUntil: quoteValidUntil,
  );

  FakeProviderOrdersRepository agenda() => FakeProviderOrdersRepository()
    ..agendaItems.addAll([
      item(sampleProviderOrderId, AgendaItemKind.scheduled, DateTime(2026, 10, 28), '16:00', '17:00', 'Troca de tomadas da cozinha'),
      item('done', AgendaItemKind.done, DateTime(2026, 10, 6), '09:00', '10:00', 'Troca de disjuntor'),
      item(
        'quoted',
        AgendaItemKind.quoted,
        DateTime(2026, 10, 30),
        '09:00',
        '11:00',
        'Revisão elétrica do apartamento',
        quoteValidUntil: DateTime(2026, 10, 29, 18),
      ),
    ]);

  Future<VitrioHarness> openAgenda(WidgetTester tester, FakeProviderOrdersRepository orders) async {
    final harness = await pumpProviderMode(tester, orders: orders);
    harness.go('/provider-agenda');
    await tester.pumpAndSettle();
    return harness;
  }

  Future<void> tapDay(WidgetTester tester, int day) async {
    await tester.tap(find.text('$day').first);
    await tester.pumpAndSettle();
  }

  testWidgets('the agenda opens on today and lists the scheduled services of the day', (tester) async {
    final orders = agenda();
    await openAgenda(tester, orders);

    expect(find.text('Outubro 2026'), findsOneWidget);
    expect(find.text('Seus serviços e os orçamentos que aguardam resposta.'), findsOneWidget);
    expect(find.text('Qua, 28 de outubro'), findsOneWidget);
    expect(find.text('AGENDADOS'), findsOneWidget);
    expect(find.text('Troca de tomadas da cozinha'), findsOneWidget);
    expect(find.text('até 17:00'), findsOneWidget);
    expect(find.text('Rafaela S. · Centro'), findsOneWidget);
    expect(orders.calls, contains('agenda:2026-10-01:2026-10-31'));
  });

  testWidgets('tapping a day shows its quotes waiting for the customer and its finished services', (tester) async {
    await openAgenda(tester, agenda());

    await tapDay(tester, 30);
    expect(find.text('Sex, 30 de outubro'), findsOneWidget);
    expect(find.text('AGUARDANDO O CLIENTE'), findsOneWidget);
    expect(find.text('Válido até qui 29/10, 18:00'), findsOneWidget);

    await tapDay(tester, 6);
    expect(find.text('FEITOS'), findsOneWidget);
    expect(find.text('Concluído'), findsOneWidget);

    await tapDay(tester, 27);
    expect(find.text('Nenhum serviço neste dia.'), findsOneWidget);
  });

  testWidgets('the arrows move between months and load the new month', (tester) async {
    final orders = agenda();
    await openAgenda(tester, orders);

    await tester.tap(find.byTooltip('Próximo mês'));
    await tester.pumpAndSettle();

    expect(find.text('Novembro 2026'), findsOneWidget);
    expect(find.text('Dom, 1 de novembro'), findsOneWidget);
    expect(orders.calls, contains('agenda:2026-11-01:2026-11-30'));

    await tester.tap(find.byTooltip('Mês anterior'));
    await tester.pumpAndSettle();
    expect(find.text('Qua, 28 de outubro'), findsOneWidget);
  });

  testWidgets('tapping an item opens the order', (tester) async {
    final orders = agenda()
      ..orders[sampleProviderOrderId] = sampleProviderOrder(
        status: OrderStatus.scheduled,
        actions: const [OrderAction.reschedule, OrderAction.cancel],
        schedule: sampleSchedule,
        quote: sampleQuote,
        released: true,
      );
    await openAgenda(tester, orders);

    await tester.tap(find.text('Troca de tomadas da cozinha'));
    await tester.pumpAndSettle();

    expect(find.text('Serviço agendado'), findsOneWidget);
    expect(find.text('Pedido'), findsWidgets);
  });
}
