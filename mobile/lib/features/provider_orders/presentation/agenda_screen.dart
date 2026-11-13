import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/routes.dart';
import '../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/design_system/widgets/vitrio_button.dart';
import '../../../core/design_system/widgets/vitrio_button_variant.dart';
import '../../../core/design_system/widgets/vitrio_calendar.dart';
import '../../../core/design_system/widgets/vitrio_calendar_mark.dart';
import '../../../core/design_system/widgets/vitrio_notice.dart';
import '../../../core/presentation/clock.dart';
import '../../../l10n/app_localizations.dart';
import '../domain/agenda_item.dart';
import '../domain/agenda_item_kind.dart';
import 'agenda_labels.dart';
import 'provider_agenda_queries.dart';
import 'schedule_slots.dart';
import 'widgets/agenda_item_card.dart';
import 'widgets/order_section_label.dart';

class AgendaScreen extends ConsumerStatefulWidget {
  const AgendaScreen({super.key});

  @override
  ConsumerState<AgendaScreen> createState() => _AgendaScreenState();
}

class _AgendaScreenState extends ConsumerState<AgendaScreen> {
  late DateTime _selected = dayOf(ref.read(clockProvider)());
  late DateTime _month = DateTime(_selected.year, _selected.month);

  DateTime get _lastDay => DateTime(_month.year, _month.month + 1, 0);

  void _moveMonth(int delta) {
    final today = dayOf(ref.read(clockProvider)());
    final month = DateTime(_month.year, _month.month + delta);
    setState(() {
      _month = month;
      _selected = month.year == today.year && month.month == today.month ? today : month;
    });
  }

  static VitrioCalendarMark _mark(AgendaItemKind kind) => switch (kind) {
    AgendaItemKind.scheduled => VitrioCalendarMark.scheduled,
    AgendaItemKind.quoted => VitrioCalendarMark.quoted,
    AgendaItemKind.done => VitrioCalendarMark.done,
  };

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final agenda = ref.watch(providerAgendaProvider(_month, _lastDay));
    final items = agenda.value ?? const <AgendaItem>[];
    final marks = <DateTime, List<VitrioCalendarMark>>{};
    for (final item in items) {
      marks.putIfAbsent(dayOf(item.date), () => []).add(_mark(item.kind));
    }

    final ofDay = items.where((item) => dayOf(item.date) == _selected).toList()..sort((a, b) => a.start.compareTo(b.start));
    final sections = [
      (AgendaItemKind.scheduled, l10n.agendaScheduledSection),
      (AgendaItemKind.quoted, l10n.agendaQuotedSection),
      (AgendaItemKind.done, l10n.agendaDoneSection),
    ];

    return AnnotatedRegion<SystemUiOverlayStyle>(
      value: Theme.of(context).brightness == Brightness.dark ? SystemUiOverlayStyle.light : SystemUiOverlayStyle.dark,
      child: Scaffold(
        body: SafeArea(
          bottom: false,
          child: RefreshIndicator(
            onRefresh: () => ref.refresh(providerAgendaProvider(_month, _lastDay).future),
            child: ListView(
              padding: const EdgeInsets.fromLTRB(VitrioSpace.sp20, VitrioSpace.sp20, VitrioSpace.sp20, VitrioSpace.sp24),
              children: [
                Semantics(
                  header: true,
                  child: Text(l10n.tabAgenda, style: VitrioText.title.copyWith(color: c.ink)),
                ),
                const SizedBox(height: VitrioSpace.sp4),
                Text(l10n.agendaSubtitle, style: VitrioText.meta.copyWith(color: c.inkMuted)),
                const SizedBox(height: VitrioSpace.sp16),
                VitrioCalendar(
                  month: _month,
                  today: dayOf(ref.watch(clockProvider)()),
                  selected: _selected,
                  marks: marks,
                  labels: agendaCalendarLabels(l10n, _month),
                  onSelect: (day) => setState(() => _selected = day),
                  onPrevious: () => _moveMonth(-1),
                  onNext: () => _moveMonth(1),
                ),
                if (agenda.hasError) ...[
                  const SizedBox(height: VitrioSpace.sp16),
                  VitrioNotice(message: l10n.agendaLoadError),
                  VitrioButton(
                    label: l10n.commonRetry,
                    variant: VitrioButtonVariant.ghost,
                    onPressed: () => ref.invalidate(providerAgendaProvider(_month, _lastDay)),
                  ),
                ],
                const SizedBox(height: VitrioSpace.sp24),
                Semantics(
                  header: true,
                  child: Text(agendaDayTitle(l10n, _selected), style: VitrioText.section.copyWith(color: c.ink)),
                ),
                if (ofDay.isEmpty && agenda.hasValue) ...[
                  const SizedBox(height: VitrioSpace.sp12),
                  Text(l10n.agendaEmptyDay, style: VitrioText.body.copyWith(height: 1.5, color: c.inkMuted)),
                ],
                for (final (index, (kind, title)) in sections.where((section) => ofDay.any((item) => item.kind == section.$1)).indexed) ...[
                  SizedBox(height: index == 0 ? VitrioSpace.sp14 : 18),
                  OrderSectionLabel(title),
                  for (final item in ofDay.where((item) => item.kind == kind)) ...[
                    const SizedBox(height: VitrioSpace.sp10),
                    AgendaItemCard(
                      item: item,
                      onTap: () => ProviderOrderRoute(id: item.orderId).go(context),
                    ),
                  ],
                ],
              ],
            ),
          ),
        ),
      ),
    );
  }
}
