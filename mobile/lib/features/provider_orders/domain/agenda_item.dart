import '../../orders/domain/city_time.dart';
import 'agenda_item_kind.dart';

final class AgendaItem {
  const AgendaItem({
    required this.orderId,
    required this.kind,
    required this.date,
    required this.start,
    required this.end,
    required this.title,
    required this.customerName,
    required this.neighborhood,
    this.quoteValidUntil,
  });

  factory AgendaItem.fromJson(Map<String, Object?> json) {
    final validUntil = json['quoteValidUntil'] as String?;
    return AgendaItem(
      orderId: json['orderId'] as String,
      kind: AgendaItemKind.fromJson(json['kind']),
      date: DateTime.parse(json['date'] as String),
      start: json['start'] as String,
      end: json['end'] as String,
      title: json['title'] as String,
      customerName: json['customerName'] as String,
      neighborhood: json['neighborhood'] as String,
      quoteValidUntil: validUntil == null ? null : parseCityTime(validUntil),
    );
  }

  final String orderId;
  final AgendaItemKind kind;
  final DateTime date;
  final String start;
  final String end;
  final String title;
  final String customerName;
  final String neighborhood;
  final DateTime? quoteValidUntil;
}
