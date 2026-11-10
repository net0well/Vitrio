import '../../../core/network/json_body.dart';
import '../../orders/domain/city_time.dart';
import '../../orders/domain/order_action.dart';
import '../../orders/domain/order_cancellation.dart';
import '../../orders/domain/order_decline.dart';
import '../../orders/domain/order_party.dart';
import '../../orders/domain/order_photo.dart';
import '../../orders/domain/order_quote.dart';
import '../../orders/domain/order_status.dart';
import '../../orders/domain/preferred_period.dart';
import '../../orders/domain/service_time.dart';
import 'order_address.dart';
import 'order_customer.dart';

final class ProviderOrder {
  const ProviderOrder({
    required this.id,
    required this.status,
    required this.title,
    required this.description,
    required this.serviceTitle,
    required this.preferredDay,
    required this.preferredPeriod,
    required this.createdAt,
    required this.respondBy,
    required this.customer,
    required this.neighborhood,
    required this.cityName,
    required this.rescheduleCount,
    required this.photos,
    required this.photosRemoved,
    required this.allowedActions,
    this.cityOffset = Duration.zero,
    this.distanceKm,
    this.address,
    this.contactVisibleUntil,
    this.quote,
    this.schedule,
    this.completedAt,
    this.completedBy,
    this.decline,
    this.cancellation,
  });

  factory ProviderOrder.fromJson(Map<String, Object?> json) {
    final address = json['address'] as Map<String, Object?>?;
    final quote = json['quote'] as Map<String, Object?>?;
    final schedule = json['schedule'] as Map<String, Object?>?;
    final decline = json['decline'] as Map<String, Object?>?;
    final cancellation = json['cancellation'] as Map<String, Object?>?;
    final contactVisibleUntil = json['contactVisibleUntil'] as String?;
    final completedAt = json['completedAt'] as String?;
    return ProviderOrder(
      id: json['id'] as String,
      status: OrderStatus.fromJson(json['status']),
      title: json['title'] as String,
      description: json['description'] as String,
      serviceTitle: json['serviceTitle'] as String,
      preferredDay: DateTime.parse(json['preferredDay'] as String),
      preferredPeriod: PreferredPeriod.fromJson(json['preferredPeriod']),
      createdAt: parseCityTime(json['createdAt'] as String),
      respondBy: parseCityTime(json['respondBy'] as String),
      cityOffset: parseCityOffset(json['createdAt'] as String),
      customer: OrderCustomer.fromJson(jsonObject(json['customer'])),
      neighborhood: json['neighborhood'] as String,
      cityName: json['cityName'] as String,
      distanceKm: (json['distanceKm'] as num?)?.toDouble(),
      address: address == null ? null : OrderAddress.fromJson(address),
      contactVisibleUntil: contactVisibleUntil == null ? null : parseCityTime(contactVisibleUntil),
      quote: quote == null ? null : OrderQuote.fromJson(quote),
      schedule: schedule == null ? null : ServiceTime.fromJson(schedule),
      rescheduleCount: json['rescheduleCount'] as int? ?? 0,
      completedAt: completedAt == null ? null : parseCityTime(completedAt),
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
  final DateTime preferredDay;
  final PreferredPeriod preferredPeriod;
  final DateTime createdAt;
  final DateTime respondBy;
  final Duration cityOffset;
  final OrderCustomer customer;
  final String neighborhood;
  final String cityName;
  final double? distanceKm;
  final OrderAddress? address;
  final DateTime? contactVisibleUntil;
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
