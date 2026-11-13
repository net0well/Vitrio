import '../../orders/domain/city_time.dart';
import '../../orders/domain/order_party.dart';
import '../../orders/domain/order_status.dart';
import '../../orders/domain/preferred_period.dart';
import '../../orders/domain/service_time.dart';

final class ProviderOrderSummary {
  const ProviderOrderSummary({
    required this.id,
    required this.status,
    required this.title,
    required this.serviceTitle,
    required this.description,
    required this.customerName,
    required this.neighborhood,
    required this.preferredDay,
    required this.preferredPeriod,
    required this.createdAt,
    required this.respondBy,
    required this.photoCount,
    this.cityOffset = Duration.zero,
    this.distanceKm,
    this.schedule,
    this.quoteValidUntil,
    this.cancelledBy,
  });

  factory ProviderOrderSummary.fromJson(Map<String, Object?> json) {
    final schedule = json['schedule'] as Map<String, Object?>?;
    final quoteValidUntil = json['quoteValidUntil'] as String?;
    return ProviderOrderSummary(
      id: json['id'] as String,
      status: OrderStatus.fromJson(json['status']),
      title: json['title'] as String,
      serviceTitle: json['serviceTitle'] as String,
      description: json['description'] as String,
      customerName: json['customerName'] as String,
      neighborhood: json['neighborhood'] as String,
      distanceKm: (json['distanceKm'] as num?)?.toDouble(),
      preferredDay: DateTime.parse(json['preferredDay'] as String),
      preferredPeriod: PreferredPeriod.fromJson(json['preferredPeriod']),
      createdAt: parseCityTime(json['createdAt'] as String),
      respondBy: parseCityTime(json['respondBy'] as String),
      cityOffset: parseCityOffset(json['createdAt'] as String),
      schedule: schedule == null ? null : ServiceTime.fromJson(schedule),
      quoteValidUntil: quoteValidUntil == null ? null : parseCityTime(quoteValidUntil),
      cancelledBy: json['cancelledBy'] == null ? null : OrderParty.fromJson(json['cancelledBy']),
      photoCount: json['photoCount'] as int? ?? 0,
    );
  }

  final String id;
  final OrderStatus status;
  final String title;
  final String serviceTitle;
  final String description;
  final String customerName;
  final String neighborhood;
  final double? distanceKm;
  final DateTime preferredDay;
  final PreferredPeriod preferredPeriod;
  final DateTime createdAt;
  final DateTime respondBy;
  final Duration cityOffset;
  final ServiceTime? schedule;
  final DateTime? quoteValidUntil;
  final OrderParty? cancelledBy;
  final int photoCount;
}
