import 'package:flutter/material.dart';

import '../tokens/vitrio_colors.dart';
import '../tokens/vitrio_radius.dart';
import '../tokens/vitrio_space.dart';
import '../tokens/vitrio_text.dart';
import 'vitrio_calendar_labels.dart';
import 'vitrio_calendar_mark.dart';
import 'vitrio_icon_button.dart';
import 'vitrio_icon_name.dart';

class VitrioCalendar extends StatelessWidget {
  const VitrioCalendar({
    super.key,
    required this.month,
    required this.today,
    required this.selected,
    required this.marks,
    required this.labels,
    required this.onSelect,
    required this.onPrevious,
    required this.onNext,
  });

  static const _daysInWeek = 7;

  final DateTime month;
  final DateTime today;
  final DateTime? selected;
  final Map<DateTime, List<VitrioCalendarMark>> marks;
  final VitrioCalendarLabels labels;
  final ValueChanged<DateTime> onSelect;
  final VoidCallback? onPrevious;
  final VoidCallback? onNext;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    final first = DateTime(month.year, month.month);
    final days = DateUtils.getDaysInMonth(month.year, month.month);
    final leading = first.weekday % _daysInWeek;
    final cells = <DateTime?>[
      for (var blank = 0; blank < leading; blank++) null,
      for (var day = 1; day <= days; day++) DateTime(month.year, month.month, day),
    ];
    final weeks = [
      for (var start = 0; start < cells.length; start += _daysInWeek)
        cells.sublist(start, (start + _daysInWeek).clamp(0, cells.length)),
    ];
    return Container(
      padding: const EdgeInsets.symmetric(vertical: VitrioSpace.sp16, horizontal: VitrioSpace.sp12),
      decoration: BoxDecoration(
        color: c.surfaceRaised,
        borderRadius: const BorderRadius.all(VitrioRadius.card),
        border: Border.all(color: c.lineSoft),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp4),
            child: Row(
              children: [
                VitrioIconButton(icon: VitrioIconName.chevronLeft, label: labels.previousMonth, onPressed: onPrevious),
                Expanded(
                  child: Text(
                    labels.month,
                    textAlign: TextAlign.center,
                    style: VitrioText.section.copyWith(color: c.ink),
                  ),
                ),
                VitrioIconButton(icon: VitrioIconName.chevronRight, label: labels.nextMonth, onPressed: onNext),
              ],
            ),
          ),
          const SizedBox(height: VitrioSpace.sp10),
          ExcludeSemantics(
            child: Row(
              children: [
                for (final weekday in labels.weekdays)
                  Expanded(
                    child: Text(weekday, textAlign: TextAlign.center, style: VitrioText.micro.copyWith(color: c.inkMuted)),
                  ),
              ],
            ),
          ),
          const SizedBox(height: VitrioSpace.sp4),
          for (final (index, week) in weeks.indexed) ...[
            if (index > 0) const SizedBox(height: 2),
            Row(
              children: [
                for (var column = 0; column < _daysInWeek; column++)
                  Expanded(
                    child: column < week.length && week[column] != null
                        ? _DayCell(
                            day: week[column]!,
                            marks: marks[week[column]!] ?? const [],
                            selected: selected != null && DateUtils.isSameDay(selected, week[column]),
                            today: DateUtils.isSameDay(today, week[column]),
                            label: labels.day(week[column]!, (marks[week[column]!] ?? const []).length),
                            onTap: () => onSelect(week[column]!),
                          )
                        : const SizedBox(height: _DayCell.height),
                  ),
              ],
            ),
          ],
          Container(
            margin: const EdgeInsets.fromLTRB(VitrioSpace.sp4, VitrioSpace.sp10, VitrioSpace.sp4, 0),
            padding: const EdgeInsets.only(top: VitrioSpace.sp12),
            decoration: BoxDecoration(border: Border(top: BorderSide(color: c.lineSoft))),
            child: Wrap(
              spacing: VitrioSpace.sp16,
              runSpacing: VitrioSpace.sp8,
              children: [
                _LegendItem(mark: VitrioCalendarMark.scheduled, label: labels.scheduled),
                _LegendItem(mark: VitrioCalendarMark.quoted, label: labels.quoted),
                _LegendItem(mark: VitrioCalendarMark.done, label: labels.done),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _DayCell extends StatelessWidget {
  const _DayCell({
    required this.day,
    required this.marks,
    required this.selected,
    required this.today,
    required this.label,
    required this.onTap,
  });

  static const height = 50.0;
  static const _circle = 34.0;
  static const _markGap = 3.0;

  final DateTime day;
  final List<VitrioCalendarMark> marks;
  final bool selected;
  final bool today;
  final String label;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    final kinds = {...marks};
    final (Color? fill, Border? border, Color text, int weight) = switch ((selected, today)) {
      (true, _) => (c.plum, null, c.onPlum, 800),
      (false, true) => (null, Border.all(color: c.plum, width: 1.5), c.plum, 800),
      _ => (null, null, c.ink, 700),
    };
    return Semantics(
      button: true,
      selected: selected,
      label: label,
      excludeSemantics: true,
      child: InkWell(
        customBorder: const CircleBorder(),
        onTap: onTap,
        child: SizedBox(
          height: height,
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Container(
                width: _circle,
                height: _circle,
                alignment: Alignment.center,
                decoration: BoxDecoration(color: fill, shape: BoxShape.circle, border: border),
                child: Text('${day.day}', style: VitrioText.weighted(VitrioText.label, weight).copyWith(color: text)),
              ),
              const SizedBox(height: _markGap),
              SizedBox(
                height: _CalendarMarkDot.size,
                child: Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    for (final (index, mark) in VitrioCalendarMark.values.where(kinds.contains).indexed) ...[
                      if (index > 0) const SizedBox(width: _markGap),
                      _CalendarMarkDot(mark: mark),
                    ],
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _CalendarMarkDot extends StatelessWidget {
  const _CalendarMarkDot({required this.mark, this.diameter = size});

  static const size = 7.0;

  final VitrioCalendarMark mark;
  final double diameter;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    final (Color fill, Border? border) = switch (mark) {
      VitrioCalendarMark.scheduled => (c.plum, null),
      VitrioCalendarMark.quoted => (c.surfaceRaised, Border.all(color: c.plum, width: 1.5)),
      VitrioCalendarMark.done => (c.lineStrong, null),
    };
    return Container(
      width: diameter,
      height: diameter,
      decoration: BoxDecoration(color: fill, shape: BoxShape.circle, border: border),
    );
  }
}

class _LegendItem extends StatelessWidget {
  const _LegendItem({required this.mark, required this.label});

  static const _legendDot = 8.0;

  final VitrioCalendarMark mark;
  final String label;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        _CalendarMarkDot(mark: mark, diameter: _legendDot),
        const SizedBox(width: VitrioSpace.sp6),
        Text(label, style: VitrioText.weighted(VitrioText.caption, 600).copyWith(color: c.ink2)),
      ],
    );
  }
}
