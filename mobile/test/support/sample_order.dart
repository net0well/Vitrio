import 'package:vitrio/features/account/domain/saved_address.dart';
import 'package:vitrio/features/orders/domain/customer_order.dart';
import 'package:vitrio/features/orders/domain/customer_order_summary.dart';
import 'package:vitrio/features/orders/domain/decline_reason.dart';
import 'package:vitrio/features/orders/domain/order_action.dart';
import 'package:vitrio/features/orders/domain/order_decline.dart';
import 'package:vitrio/features/orders/domain/order_provider_card.dart';
import 'package:vitrio/features/orders/domain/order_quote.dart';
import 'package:vitrio/features/orders/domain/order_status.dart';
import 'package:vitrio/features/orders/domain/preferred_period.dart';
import 'package:vitrio/features/orders/domain/quote_type.dart';
import 'package:vitrio/features/orders/domain/service_time.dart';

final sampleNow = DateTime(2026, 10, 28, 10);

const sampleOrderId = '0199a0c4-0000-7000-8000-0000000000a1';

const sampleHome = SavedAddress(
  id: 'address-home',
  label: 'Casa',
  cep: '14870-000',
  street: 'Rua Rui Barbosa',
  number: '123',
  neighborhood: 'Centro',
  city: 'Jaboticabal',
  state: 'SP',
);

const sampleOrderProvider = OrderProviderCard(
  id: '0199a0c4-0000-7000-8000-00000000c0de',
  displayName: 'Carlos Menezes',
  headline: 'Eletricista',
  initials: 'CM',
  isNew: false,
  ratingScore: 4.9,
  reviewCount: 127,
);

final sampleQuote = OrderQuote(
  type: QuoteType.fixedPrice,
  amount: 180,
  time: ServiceTime(date: DateTime(2026, 10, 29), start: '14:00', end: '15:00', durationMinutes: 60),
  materialIncluded: true,
  materialDetails: 'disjuntor 40 A',
  message: 'Troco o disjuntor e reviso a fiação.',
  sentAt: DateTime(2026, 10, 28, 9, 30),
  validUntil: DateTime(2026, 10, 29, 13),
  version: 2,
);

CustomerOrder sampleOrder({
  OrderStatus status = OrderStatus.quoted,
  List<OrderAction> actions = const [OrderAction.acceptQuote, OrderAction.declineQuote, OrderAction.cancel],
  OrderDecline? decline,
  bool withQuote = true,
  ServiceTime? schedule,
}) =>
    CustomerOrder(
      id: sampleOrderId,
      status: status,
      title: 'Chuveiro desarmando o disjuntor',
      description: 'O chuveiro desarma o disjuntor quando ligo no modo inverno.',
      serviceTitle: 'Troca de chuveiro',
      categoryId: 1,
      preferredDay: DateTime(2026, 10, 29),
      preferredPeriod: PreferredPeriod.afternoon,
      neighborhood: 'Centro',
      cityName: 'Jaboticabal',
      createdAt: DateTime(2026, 10, 28, 9, 12),
      respondBy: DateTime(2026, 10, 29, 18),
      provider: sampleOrderProvider,
      quote: withQuote ? sampleQuote : null,
      schedule: schedule,
      rescheduleCount: 0,
      decline: decline,
      photos: const [],
      photosRemoved: false,
      allowedActions: actions,
    );

const sampleDecline = OrderDecline(reason: DeclineReason.noAvailability);

CustomerOrderSummary sampleSummary({OrderStatus status = OrderStatus.quoted, String title = 'Chuveiro desarmando o disjuntor'}) =>
    CustomerOrderSummary(
      id: sampleOrderId,
      title: title,
      categoryId: 1,
      providerName: 'Carlos Menezes',
      status: status,
      createdAt: DateTime(2026, 10, 28, 9, 12),
    );
