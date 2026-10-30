final class AvailabilitySlot {
  const AvailabilitySlot({required this.dayOfWeek, required this.start, required this.end});

  factory AvailabilitySlot.fromJson(Map<String, Object?> json) =>
      AvailabilitySlot(dayOfWeek: json['dayOfWeek'] as String, start: json['start'] as String, end: json['end'] as String);

  static const days = ['monday', 'tuesday', 'wednesday', 'thursday', 'friday', 'saturday', 'sunday'];

  final String dayOfWeek;
  final String start;
  final String end;

  Map<String, Object?> toJson() => {'dayOfWeek': dayOfWeek, 'start': start, 'end': end};
}
