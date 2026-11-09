import 'decline_reason.dart';

final class OrderDecline {
  const OrderDecline({required this.reason, this.details});

  factory OrderDecline.fromJson(Map<String, Object?> json) =>
      OrderDecline(reason: DeclineReason.fromJson(json['reason']), details: json['details'] as String?);

  final DeclineReason reason;
  final String? details;
}
