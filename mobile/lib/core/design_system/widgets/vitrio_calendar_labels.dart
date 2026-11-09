class VitrioCalendarLabels {
  const VitrioCalendarLabels({
    required this.month,
    required this.weekdays,
    required this.previousMonth,
    required this.nextMonth,
    required this.scheduled,
    required this.quoted,
    required this.done,
    required this.day,
  });

  final String month;
  final List<String> weekdays;
  final String previousMonth;
  final String nextMonth;
  final String scheduled;
  final String quoted;
  final String done;
  final String Function(DateTime day, int items) day;
}
