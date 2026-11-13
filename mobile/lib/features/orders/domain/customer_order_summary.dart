import 'city_time.dart';
import 'order_status.dart';
import 'service_time.dart';

final class CustomerOrderSummary {
  const CustomerOrderSummary({
    required this.id,
    required this.title,
    required this.categoryId,
    required this.providerName,
    required this.status,
    required this.createdAt,
    this.cityOffset = Duration.zero,
    this.schedule,
  });

  factory CustomerOrderSummary.fromJson(Map<String, Object?> json) {
    final schedule = json['schedule'] as Map<String, Object?>?;
    return CustomerOrderSummary(
      id: json['id'] as String,
      title: json['title'] as String,
      categoryId: json['categoryId'] as int,
      providerName: json['providerName'] as String,
      status: OrderStatus.fromJson(json['status']),
      createdAt: parseCityTime(json['createdAt'] as String),
      cityOffset: parseCityOffset(json['createdAt'] as String),
      schedule: schedule == null ? null : ServiceTime.fromJson(schedule),
    );
  }

  final String id;
  final String title;
  final int categoryId;
  final String providerName;
  final OrderStatus status;
  final DateTime createdAt;
  final Duration cityOffset;
  final ServiceTime? schedule;
}
