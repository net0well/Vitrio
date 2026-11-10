import '../domain/agenda_item.dart';
import '../domain/agenda_item_kind.dart';

const slotMinutes = 30;
const maxDurationMinutes = 8 * 60;
const maxDaysAhead = 30;
const minimumNotice = Duration(hours: 1);
const _minutesPerDay = 24 * 60;

String clockLabel(int minutes) => '${(minutes ~/ 60).toString().padLeft(2, '0')}:${(minutes % 60).toString().padLeft(2, '0')}';

int parseClock(String value) {
  final parts = value.split(':');
  return int.parse(parts[0]) * 60 + int.parse(parts[1]);
}

DateTime dayOf(DateTime moment) => DateTime(moment.year, moment.month, moment.day);

List<DateTime> scheduleDays(DateTime cityNow) {
  final today = dayOf(cityNow);
  return [
    for (var offset = 0; offset <= maxDaysAhead; offset++)
      if (startSlots(DateTime(today.year, today.month, today.day + offset), cityNow).isNotEmpty)
        DateTime(today.year, today.month, today.day + offset),
  ];
}

List<int> startSlots(DateTime day, DateTime cityNow) {
  final earliest = cityNow.add(minimumNotice);
  return [
    for (var minutes = 0; minutes < _minutesPerDay; minutes += slotMinutes)
      if (DateTime(day.year, day.month, day.day, minutes ~/ 60, minutes % 60).isAfter(earliest)) minutes,
  ];
}

List<int> durationSlots(int start) => [
  for (var minutes = slotMinutes; minutes <= maxDurationMinutes && start + minutes <= _minutesPerDay; minutes += slotMinutes) minutes,
];

int closestSlot(List<int> slots, int wanted) =>
    slots.contains(wanted) ? wanted : slots.firstWhere((slot) => slot >= wanted, orElse: () => slots.last);

AgendaItem? overlapping(List<AgendaItem> agenda, {required DateTime day, required int start, required int end, required String orderId}) =>
    agenda
        .where(
          (item) =>
              item.kind == AgendaItemKind.scheduled &&
              item.orderId != orderId &&
              dayOf(item.date) == dayOf(day) &&
              parseClock(item.start) < end &&
              parseClock(item.end) > start,
        )
        .firstOrNull;
