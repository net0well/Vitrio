import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/design_system/tokens/vitrio_space.dart';
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

  Widget _timePicker(String value, ValueChanged<String> onChanged) => DropdownButton<String>(
        value: value,
        items: [for (final time in _times) DropdownMenuItem(value: time, child: Text(time))],
        onChanged: (selected) => onChanged(selected ?? value),
      );

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
          SwitchListTile(
            contentPadding: EdgeInsets.zero,
            title: Text(dayLabel(l10n, day)),
            value: _days[day]!.open,
            onChanged: (open) => setState(() => _days[day] = (open: open, start: _days[day]!.start, end: _days[day]!.end)),
          ),
          if (_days[day]!.open)
            Row(
              children: [
                Text(l10n.providerFrom),
                const SizedBox(width: VitrioSpace.sp8),
                _timePicker(_days[day]!.start, (start) => setState(() => _days[day] = (open: true, start: start, end: _days[day]!.end))),
                const SizedBox(width: VitrioSpace.sp8),
                Text(l10n.providerTo),
                const SizedBox(width: VitrioSpace.sp8),
                _timePicker(_days[day]!.end, (end) => setState(() => _days[day] = (open: true, start: _days[day]!.start, end: end))),
              ],
            ),
        ],
        const Divider(),
        SwitchListTile(
          contentPadding: EdgeInsets.zero,
          title: Text(l10n.providerEmergencies),
          value: _emergencies,
          onChanged: (value) => setState(() => _emergencies = value),
        ),
      ],
    );
  }
}
