import 'package:intl/intl.dart';

import '../../../core/design_system/widgets/vitrio_calendar_labels.dart';
import '../../../l10n/app_localizations.dart';
import '../../orders/presentation/order_labels.dart';

final _monthName = DateFormat('MMMM', 'pt_BR');
final _monthAndYear = DateFormat('MMMM yyyy', 'pt_BR');

String agendaDayTitle(AppLocalizations l10n, DateTime day) =>
    l10n.agendaDayTitle(capitalized(weekdayShort(l10n, day)), day.day, _monthName.format(day));

VitrioCalendarLabels agendaCalendarLabels(AppLocalizations l10n, DateTime month) => VitrioCalendarLabels(
  month: capitalized(_monthAndYear.format(month)),
  weekdays: [
    l10n.weekdaySunShort,
    l10n.weekdayMonShort,
    l10n.weekdayTueShort,
    l10n.weekdayWedShort,
    l10n.weekdayThuShort,
    l10n.weekdayFriShort,
    l10n.weekdaySatShort,
  ],
  previousMonth: l10n.agendaPreviousMonth,
  nextMonth: l10n.agendaNextMonth,
  scheduled: l10n.agendaScheduled,
  quoted: l10n.agendaQuoted,
  done: l10n.agendaDone,
  day: (day, items) => l10n.agendaDayLabel(capitalized(weekdayShort(l10n, day)), day.day, _monthName.format(day), items),
);
