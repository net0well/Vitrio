import 'package:flutter/material.dart';

import '../../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../../core/design_system/tokens/vitrio_space.dart';
import '../../../../core/design_system/tokens/vitrio_text.dart';
import '../../../../core/design_system/widgets/vitrio_badge_tone.dart';
import '../../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../../core/design_system/widgets/vitrio_notice.dart';
import '../../../../core/design_system/widgets/vitrio_option.dart';
import '../../../../l10n/app_localizations.dart';
import '../../domain/agenda_item.dart';
import '../provider_order_labels.dart';
import '../schedule_slots.dart';
import 'order_select_field.dart';

class ScheduleFields extends StatelessWidget {
  const ScheduleFields({
    super.key,
    required this.dayLabel,
    required this.days,
    required this.today,
    required this.day,
    required this.starts,
    required this.start,
    required this.durations,
    required this.duration,
    required this.onDay,
    required this.onStart,
    required this.onDuration,
    this.overlap,
  });

  final String dayLabel;
  final List<DateTime> days;
  final DateTime today;
  final DateTime day;
  final List<int> starts;
  final int start;
  final List<int> durations;
  final int duration;
  final ValueChanged<DateTime> onDay;
  final ValueChanged<int> onStart;
  final ValueChanged<int> onDuration;
  final AgendaItem? overlap;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final conflict = overlap;
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        OrderSelectField<DateTime>(
          label: dayLabel,
          value: day,
          options: [for (final option in days) VitrioOption(value: option, label: dayOption(l10n, option, today))],
          onChanged: onDay,
        ),
        const SizedBox(height: VitrioSpace.sp14),
        Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Expanded(
              child: OrderSelectField<int>(
                label: l10n.respondStart,
                value: start,
                options: [for (final option in starts) VitrioOption(value: option, label: clockLabel(option))],
                onChanged: onStart,
              ),
            ),
            const SizedBox(width: VitrioSpace.sp10),
            Expanded(
              child: OrderSelectField<int>(
                label: l10n.respondDuration,
                value: duration,
                options: [for (final option in durations) VitrioOption(value: option, label: durationLabel(l10n, option))],
                onChanged: onDuration,
              ),
            ),
          ],
        ),
        const SizedBox(height: VitrioSpace.sp8),
        Text(
          l10n.respondRange(clockLabel(start), clockLabel(start + duration)),
          style: VitrioText.weighted(VitrioText.caption, 600).copyWith(color: c.inkMuted),
        ),
        if (conflict != null) ...[
          const SizedBox(height: VitrioSpace.sp10),
          VitrioNotice(
            message: l10n.respondOverlap(conflict.start, conflict.end),
            tone: VitrioBadgeTone.warning,
            icon: VitrioIconName.info,
          ),
        ],
      ],
    );
  }
}
