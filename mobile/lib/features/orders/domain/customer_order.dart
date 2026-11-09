import '../../../core/network/json_body.dart';
import 'order_action.dart';
import 'order_cancellation.dart';
import 'order_decline.dart';
import 'order_party.dart';
import 'order_photo.dart';
import 'order_provider_card.dart';
import 'order_quote.dart';
import 'order_status.dart';
import 'preferred_period.dart';
import 'service_time.dart';

final class CustomerOrder {
  const CustomerOrder({
    required this.id,
    required this.status,
    required this.title,
    required this.description,
    required this.serviceTitle,
    required this.categoryId,
    required this.preferredDay,
    required this.preferredPeriod,
    required this.neighborhood,
    required this.cityName,
    required this.createdAt,
    required this.respondBy,
    required this.provider,
    required this.rescheduleCount,
    required this.photos,
    required this.photosRemoved,
    required this.allowedActions,
    this.quote,
    this.schedule,
    this.completedAt,
    this.completedBy,
    this.decline,
    this.cancellation,
  });

  factory CustomerOrder.fromJson(Map<String, Object?> json) {
    final quote = json['quote'] as Map<String, Object?>?;
    final schedule = json['schedule'] as Map<String, Object?>?;
    final decline = json['decline'] as Map<String, Object?>?;
    final cancellation = json['cancellation'] as Map<String, Object?>?;
    final completedAt = json['completedAt'] as String?;
    return CustomerOrder(
      id: json['id'] as String,
      status: OrderStatus.fromJson(json['status']),
      title: json['title'] as String,
      description: json['description'] as String,
      serviceTitle: json['serviceTitle'] as String,
      categoryId: json['categoryId'] as int,
      preferredDay: DateTime.parse(json['preferredDay'] as String),
      preferredPeriod: PreferredPeriod.fromJson(json['preferredPeriod']),
      neighborhood: json['neighborhood'] as String,
      cityName: json['cityName'] as String,
      createdAt: DateTime.parse(json['createdAt'] as String),
      respondBy: DateTime.parse(json['respondBy'] as String),
      provider: OrderProviderCard.fromJson(jsonObject(json['provider'])),
      quote: quote == null ? null : OrderQuote.fromJson(quote),
      schedule: schedule == null ? null : ServiceTime.fromJson(schedule),
      rescheduleCount: json['rescheduleCount'] as int? ?? 0,
      completedAt: completedAt == null ? null : DateTime.parse(completedAt),
      completedBy: json['completedBy'] == null ? null : OrderParty.fromJson(json['completedBy']),
      decline: decline == null ? null : OrderDecline.fromJson(decline),
      cancellation: cancellation == null ? null : OrderCancellation.fromJson(cancellation),
      photos: jsonObjects(json['photos'] ?? const <Object?>[]).map(OrderPhoto.fromJson).toList(growable: false),
      photosRemoved: json['photosRemoved'] == true,
      allowedActions: (json['allowedActions'] as List<Object?>? ?? const []).map(OrderAction.fromJson).toList(growable: false),
    );
  }

  final String id;
  final OrderStatus status;
  final String title;
  final String description;
  final String serviceTitle;
  final int categoryId;
  final DateTime preferredDay;
  final PreferredPeriod preferredPeriod;
  final String neighborhood;
  final String cityName;
  final DateTime createdAt;
  final DateTime respondBy;
  final OrderProviderCard provider;
  final OrderQuote? quote;
  final ServiceTime? schedule;
  final int rescheduleCount;
  final DateTime? completedAt;
  final OrderParty? completedBy;
  final OrderDecline? decline;
  final OrderCancellation? cancellation;
  final List<OrderPhoto> photos;
  final bool photosRemoved;
  final List<OrderAction> allowedActions;

  bool allows(OrderAction action) => allowedActions.contains(action);
}
