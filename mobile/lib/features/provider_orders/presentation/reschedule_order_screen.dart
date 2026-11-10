import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/routes.dart';
import '../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../core/design_system/tokens/vitrio_radius.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/design_system/widgets/vitrio_action_bar.dart';
import '../../../core/design_system/widgets/vitrio_button.dart';
import '../../../core/design_system/widgets/vitrio_empty_state.dart';
import '../../../core/design_system/widgets/vitrio_icon.dart';
import '../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../core/design_system/widgets/vitrio_notice.dart';
import '../../../core/design_system/widgets/vitrio_top_bar.dart';
import '../../../core/network/api_result.dart';
import '../../../core/network/problem_details.dart';
import '../../../core/presentation/clock.dart';
import '../../../core/presentation/problem_message.dart';
import '../../../l10n/app_localizations.dart';
import '../../orders/domain/city_time.dart';
import '../../orders/domain/order_action.dart';
import '../../orders/presentation/order_labels.dart';
import '../domain/provider_order.dart';
import 'provider_agenda_queries.dart';
import 'provider_order_controller.dart';
import 'schedule_slots.dart';
import 'widgets/order_info_tile.dart';
import 'widgets/schedule_fields.dart';

class RescheduleOrderScreen extends ConsumerWidget {
  const RescheduleOrderScreen({super.key, required this.orderId});

  final String orderId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    return switch (ref.watch(providerOrderControllerProvider(orderId))) {
      AsyncData(:final value) when value.schedule != null => _RescheduleForm(order: value),
      AsyncLoading() => Scaffold(
        appBar: VitrioTopBar(title: l10n.rescheduleTitle),
        body: const Center(child: CircularProgressIndicator()),
      ),
      _ => Scaffold(
        appBar: VitrioTopBar(title: l10n.rescheduleTitle),
        body: Center(
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp20),
            child: VitrioEmptyState(
              icon: VitrioIconName.orders,
              message: l10n.orderLoadError,
              actionLabel: l10n.commonRetry,
              onAction: () => ref.invalidate(providerOrderControllerProvider(orderId)),
            ),
          ),
        ),
      ),
    };
  }
}

class _RescheduleForm extends ConsumerStatefulWidget {
  const _RescheduleForm({required this.order});

  final ProviderOrder order;

  @override
  ConsumerState<_RescheduleForm> createState() => _RescheduleFormState();
}

class _RescheduleFormState extends ConsumerState<_RescheduleForm> {
  static const _maxReschedules = 2;

  late DateTime _day;
  late int _start;
  late int _duration;
  bool _busy = false;
  ProblemDetails? _problem;

  DateTime get _now => cityNow(ref.read(clockProvider)(), widget.order.cityOffset);

  @override
  void initState() {
    super.initState();
    final schedule = widget.order.schedule!;
    final days = scheduleDays(_now);
    _day = days.firstWhere((day) => !day.isBefore(dayOf(schedule.date)), orElse: () => days.first);
    _start = closestSlot(startSlots(_day, _now), parseClock(schedule.start));
    _duration = closestSlot(durationSlots(_start), schedule.durationMinutes);
  }

  void _selectDay(DateTime day) => setState(() {
    _day = day;
    _start = closestSlot(startSlots(day, _now), _start);
    _duration = closestSlot(durationSlots(_start), _duration);
  });

  void _selectStart(int start) => setState(() {
    _start = start;
    _duration = closestSlot(durationSlots(start), _duration);
  });

  Future<void> _save() async {
    final l10n = AppLocalizations.of(context);
    setState(() {
      _busy = true;
      _problem = null;
    });
    final result = await ref
        .read(providerOrderControllerProvider(widget.order.id).notifier)
        .reschedule(date: _day, start: clockLabel(_start), durationMinutes: _duration);
    if (!mounted) {
      return;
    }

    switch (result) {
      case ApiSuccess():
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(l10n.providerRescheduled(widget.order.customer.firstName))));
        _leave();
      case ApiFailure(:final problem):
        setState(() {
          _busy = false;
          _problem = problem;
        });
    }
  }

  void _leave() {
    final navigator = Navigator.of(context);
    if (navigator.canPop()) {
      navigator.pop();
    } else {
      ProviderOrderRoute(id: widget.order.id).go(context);
    }
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final order = widget.order;
    final schedule = order.schedule!;
    final now = cityNow(ref.watch(clockProvider)(), order.cityOffset);
    final agenda = ref.watch(providerAgendaProvider(_day, _day)).value ?? const [];
    final conflict = overlapping(agenda, day: _day, start: _start, end: _start + _duration, orderId: order.id);
    final remaining = _maxReschedules - order.rescheduleCount;
    final allowed = order.allows(OrderAction.reschedule);
    final serverMessage = _problem == null ? null : problemMessage(l10n, _problem!);

    return Scaffold(
      appBar: VitrioTopBar(title: l10n.rescheduleTitle),
      body: ListView(
        padding: const EdgeInsets.fromLTRB(VitrioSpace.sp20, VitrioSpace.sp16, VitrioSpace.sp20, VitrioSpace.sp24),
        children: [
          Container(
            padding: const EdgeInsets.all(VitrioSpace.sp16),
            decoration: BoxDecoration(
              color: c.surfaceRaised,
              borderRadius: const BorderRadius.all(VitrioRadius.card),
              border: Border.all(color: c.lineSoft),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Text(order.title, style: VitrioText.bodyStrong.copyWith(color: c.ink)),
                const SizedBox(height: 2),
                Text('${order.customer.name} · ${order.neighborhood}', style: VitrioText.meta.copyWith(color: c.inkMuted)),
                const SizedBox(height: VitrioSpace.sp12),
                OrderInfoTile(
                  label: l10n.rescheduleCurrent,
                  value: l10n.orderDayAndTime(shortDate(l10n, schedule.date), serviceRange(schedule)),
                ),
              ],
            ),
          ),
          const SizedBox(height: VitrioSpace.sp24),
          Semantics(
            header: true,
            child: Text(l10n.rescheduleNew, style: VitrioText.section.copyWith(color: c.ink)),
          ),
          const SizedBox(height: VitrioSpace.sp12),
          ScheduleFields(
            dayLabel: l10n.rescheduleDay,
            days: scheduleDays(now),
            today: now,
            day: _day,
            starts: startSlots(_day, now),
            start: _start,
            durations: durationSlots(_start),
            duration: _duration,
            onDay: _selectDay,
            onStart: _selectStart,
            onDuration: (duration) => setState(() => _duration = duration),
            overlap: conflict,
          ),
          const SizedBox(height: VitrioSpace.sp14),
          Row(
            children: [
              VitrioIcon(VitrioIconName.clock, size: 16, color: c.inkMuted),
              const SizedBox(width: VitrioSpace.sp6),
              Expanded(
                child: Text(
                  allowed ? l10n.rescheduleRemaining(remaining) : l10n.rescheduleNone,
                  style: VitrioText.meta.copyWith(color: c.inkMuted),
                ),
              ),
            ],
          ),
        ],
      ),
      bottomNavigationBar: VitrioActionBar(
        note: l10n.rescheduleNote(order.customer.firstName),
        top: serverMessage == null ? null : VitrioNotice(message: serverMessage),
        children: [VitrioButton(label: l10n.rescheduleSave, busy: _busy, onPressed: allowed ? _save : null)],
      ),
    );
  }
}
