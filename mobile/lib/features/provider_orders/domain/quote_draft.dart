import '../../orders/domain/quote_type.dart';

final class QuoteDraft {
  const QuoteDraft({
    required this.type,
    required this.amount,
    required this.date,
    required this.start,
    required this.durationMinutes,
    required this.materialIncluded,
    this.materialDetails,
    this.message,
  });

  final QuoteType type;
  final double amount;
  final DateTime date;
  final String start;
  final int durationMinutes;
  final bool materialIncluded;
  final String? materialDetails;
  final String? message;

  Map<String, Object?> toJson() => {
    'type': type.name,
    'amount': amount,
    'date': isoDate(date),
    'start': start,
    'durationMinutes': durationMinutes,
    'materialIncluded': materialIncluded,
    'materialDetails': materialDetails,
    'message': message,
  };

  static String isoDate(DateTime day) =>
      '${day.year.toString().padLeft(4, '0')}-${day.month.toString().padLeft(2, '0')}-${day.day.toString().padLeft(2, '0')}';
}
