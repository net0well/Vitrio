import 'cancellation_reason.dart';
import 'order_party.dart';

final class OrderCancellation {
  const OrderCancellation({required this.by, required this.reason, required this.at, this.details});

  factory OrderCancellation.fromJson(Map<String, Object?> json) => OrderCancellation(
        by: OrderParty.fromJson(json['by']),
        reason: CancellationReason.fromJson(json['reason']),
        details: json['details'] as String?,
        at: DateTime.parse(json['at'] as String),
      );

  final OrderParty by;
  final CancellationReason reason;
  final String? details;
  final DateTime at;
}
