import 'quote_type.dart';
import 'service_time.dart';

final class OrderQuote {
  const OrderQuote({
    required this.type,
    required this.amount,
    required this.time,
    required this.materialIncluded,
    required this.sentAt,
    required this.validUntil,
    required this.version,
    this.materialDetails,
    this.message,
  });

  factory OrderQuote.fromJson(Map<String, Object?> json) => OrderQuote(
        type: QuoteType.fromJson(json['type']),
        amount: (json['amount'] as num).toDouble(),
        time: ServiceTime.fromJson(json['time'] as Map<String, Object?>),
        materialIncluded: json['materialIncluded'] == true,
        materialDetails: json['materialDetails'] as String?,
        message: json['message'] as String?,
        sentAt: DateTime.parse(json['sentAt'] as String),
        validUntil: DateTime.parse(json['validUntil'] as String),
        version: json['version'] as int,
      );

  final QuoteType type;
  final double amount;
  final ServiceTime time;
  final bool materialIncluded;
  final String? materialDetails;
  final String? message;
  final DateTime sentAt;
  final DateTime validUntil;
  final int version;
}
