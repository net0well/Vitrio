import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../core/design_system/tokens/vitrio_radius.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/design_system/widgets/vitrio_icon.dart';
import '../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../core/design_system/widgets/vitrio_switch_row.dart';
import '../../../core/network/api_result.dart';
import '../../../core/network/problem_details.dart';
import '../../../core/presentation/form_page.dart';
import '../../../core/presentation/problem_message.dart';
import '../../../l10n/app_localizations.dart';
import '../domain/availability_slot.dart';
import 'provider_controller.dart';
import 'provider_labels.dart';

class AvailabilityScreen extends ConsumerStatefulWidget {
  const AvailabilityScreen({super.key});

  @override
  ConsumerState<AvailabilityScreen> createState() => _AvailabilityScreenState();
}

class _AvailabilityScreenState extends ConsumerState<AvailabilityScreen> {
  static final List<String> _times = [
    for (var minute = 0; minute <= 24 * 60; minute += 30)
      '${(minute ~/ 60).toString().padLeft(2, '0')}:${(minute % 60).toString().padLeft(2, '0')}',
  ];

  final Map<String, ({bool open, String start, String end})> _days = {};
  late bool _emergencies;
  bool _busy = false;
  ProblemDetails? _problem;

  @override
  void initState() {
    super.initState();
    final profile = ref.read(providerControllerProvider).value;
    for (final day in AvailabilitySlot.days) {
      final slot = profile?.availability.where((candidate) => candidate.dayOfWeek == day).firstOrNull;
      _days[day] = (open: slot != null, start: slot?.start ?? '08:00', end: slot?.end ?? '18:00');
    }

    _emergencies = profile?.handlesEmergencies ?? false;
  }

  Future<void> _save() async {
    setState(() {
      _busy = true;
      _problem = null;
    });
    final slots = [
      for (final MapEntry(key: day, value: hours) in _days.entries)
        if (hours.open) AvailabilitySlot(dayOfWeek: day, start: hours.start, end: hours.end),
    ];
    final result = await ref
        .read(providerControllerProvider.notifier)
        .run((repository) => repository.setAvailability(slots, handlesEmergencies: _emergencies));
    if (!mounted) {
      return;
    }

    switch (result) {
      case ApiSuccess():
        Navigator.of(context).pop();
      case ApiFailure(:final problem):
        setState(() {
          _busy = false;
          _problem = problem;
        });
    }
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    return FormPage(
      title: l10n.providerAvailability,
      actionLabel: l10n.commonSave,
      busy: _busy,
      onAction: _save,
      message: _problem == null ? null : problemMessage(l10n, _problem!),
      children: [
        for (final day in AvailabilitySlot.days) ...[
          VitrioSwitchRow(
            label: dayLabel(l10n, day),
            value: _days[day]!.open,
            onChanged: (open) => setState(() => _days[day] = (open: open, start: _days[day]!.start, end: _days[day]!.end)),
          ),
          if (_days[day]!.open) ...[
            const SizedBox(height: VitrioSpace.sp8),
            Row(
              children: [
                Expanded(
                  child: _TimeField(
                    label: l10n.providerFrom,
                    value: _days[day]!.start,
                    times: _times,
                    onChanged: (start) => setState(() => _days[day] = (open: true, start: start, end: _days[day]!.end)),
                  ),
                ),
                const SizedBox(width: VitrioSpace.sp10),
                Expanded(
                  child: _TimeField(
                    label: l10n.providerTo,
                    value: _days[day]!.end,
                    times: _times,
                    onChanged: (end) => setState(() => _days[day] = (open: true, start: _days[day]!.start, end: end)),
                  ),
                ),
              ],
            ),
          ],
          const SizedBox(height: VitrioSpace.sp10),
        ],
        const SizedBox(height: VitrioSpace.sp14),
        VitrioSwitchRow(
          label: l10n.providerEmergencies,
          value: _emergencies,
          onChanged: (value) => setState(() => _emergencies = value),
        ),
      ],
    );
  }
}

class _TimeField extends StatelessWidget {
  const _TimeField({required this.label, required this.value, required this.times, required this.onChanged});

  final String label;
  final String value;
  final List<String> times;
  final ValueChanged<String> onChanged;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    return Container(
      height: VitrioSpace.tap,
      padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp12),
      decoration: BoxDecoration(
        color: c.surfaceRaised,
        borderRadius: const BorderRadius.all(VitrioRadius.lg),
        border: Border.all(color: c.lineStrong),
      ),
      child: Row(
        children: [
          Text(label, style: VitrioText.meta.copyWith(color: c.inkMuted)),
          const SizedBox(width: VitrioSpace.sp8),
          Expanded(
            child: DropdownButtonHideUnderline(
              child: DropdownButton<String>(
                value: value,
                isExpanded: true,
                borderRadius: const BorderRadius.all(VitrioRadius.lg),
                dropdownColor: c.surfaceRaised,
                icon: VitrioIcon(VitrioIconName.chevronDown, size: 16, color: c.inkMuted),
                style: VitrioText.bodyStrong.copyWith(color: c.ink),
                items: [for (final time in times) DropdownMenuItem(value: time, child: Text(time))],
                onChanged: (selected) => onChanged(selected ?? value),
              ),
            ),
          ),
        ],
      ),
    );
  }
}
