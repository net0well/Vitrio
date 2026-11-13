import 'package:vitrio/features/orders/domain/order_action.dart';
import 'package:vitrio/features/orders/domain/order_party.dart';
import 'package:vitrio/features/orders/domain/order_quote.dart';
import 'package:vitrio/features/orders/domain/order_status.dart';
import 'package:vitrio/features/orders/domain/preferred_period.dart';
import 'package:vitrio/features/orders/domain/service_time.dart';
import 'package:vitrio/features/provider_orders/domain/order_address.dart';
import 'package:vitrio/features/provider_orders/domain/order_customer.dart';
import 'package:vitrio/features/provider_orders/domain/provider_order.dart';
import 'package:vitrio/features/provider_orders/domain/provider_order_summary.dart';

const sampleProviderOrderId = '0199a0c4-0000-7000-8000-0000000000b2';

final sampleSchedule = ServiceTime(date: DateTime(2026, 10, 29), start: '14:00', end: '15:00', durationMinutes: 60);

ProviderOrderSummary sampleProviderOrderSummary({
  String id = sampleProviderOrderId,
  OrderStatus status = OrderStatus.sent,
  String title = 'Chuveiro desarmando o disjuntor',
  ServiceTime? schedule,
  DateTime? quoteValidUntil,
  OrderParty? cancelledBy,
}) =>
    ProviderOrderSummary(
      id: id,
      status: status,
      title: title,
      serviceTitle: 'Chuveiro',
      description: 'O chuveiro desarma o disjuntor quando ligo no modo inverno. Já troquei a resistência e continua igual.',
      customerName: 'Mariana A.',
      neighborhood: 'Centro',
      distanceKm: 1.8,
      preferredDay: DateTime(2026, 10, 29),
      preferredPeriod: PreferredPeriod.afternoon,
      createdAt: DateTime(2026, 10, 28, 9, 55),
      respondBy: DateTime(2026, 10, 29, 18),
      photoCount: 2,
      schedule: schedule,
      quoteValidUntil: quoteValidUntil,
      cancelledBy: cancelledBy,
    );

ProviderOrder sampleProviderOrder({
  OrderStatus status = OrderStatus.sent,
  List<OrderAction> actions = const [OrderAction.sendQuote, OrderAction.decline],
  ServiceTime? schedule,
  OrderQuote? quote,
  int rescheduleCount = 0,
  bool released = false,
}) =>
    ProviderOrder(
      id: sampleProviderOrderId,
      status: status,
      title: 'Chuveiro desarmando o disjuntor',
      description: 'O chuveiro desarma o disjuntor quando ligo no modo inverno. Já troquei a resistência e continua igual.',
      serviceTitle: 'Chuveiro',
      preferredDay: DateTime(2026, 10, 29),
      preferredPeriod: PreferredPeriod.afternoon,
      createdAt: DateTime(2026, 10, 28, 9, 55),
      respondBy: DateTime(2026, 10, 29, 18),
      customer: OrderCustomer(
        displayName: 'Mariana A.',
        fullName: released ? 'Mariana Alves' : null,
        phone: released ? '(16) 99123-4567' : null,
        memberSince: DateTime(2026, 9, 3),
        orderCount: 3,
      ),
      neighborhood: 'Centro',
      cityName: 'Jaboticabal',
      distanceKm: 1.8,
      address: released
          ? const OrderAddress(
              street: 'Rua Rui Barbosa',
              number: '123',
              complement: 'apto 12',
              neighborhood: 'Centro',
              cityName: 'Jaboticabal',
              state: 'SP',
              cep: '14870-000',
            )
          : null,
      contactVisibleUntil: released ? DateTime(2026, 11, 5, 15) : null,
      quote: quote,
      schedule: schedule,
      rescheduleCount: rescheduleCount,
      photos: const [],
      photosRemoved: false,
      allowedActions: actions,
    );
