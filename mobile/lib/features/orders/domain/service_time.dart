final class ServiceTime {
  const ServiceTime({required this.date, required this.start, required this.end, required this.durationMinutes});

  factory ServiceTime.fromJson(Map<String, Object?> json) => ServiceTime(
        date: DateTime.parse(json['date'] as String),
        start: json['start'] as String,
        end: json['end'] as String,
        durationMinutes: json['durationMinutes'] as int,
      );

  final DateTime date;
  final String start;
  final String end;
  final int durationMinutes;
}
