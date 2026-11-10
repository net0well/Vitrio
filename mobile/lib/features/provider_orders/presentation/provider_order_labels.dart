import 'package:intl/intl.dart';

import '../../../core/design_system/widgets/vitrio_badge_tone.dart';
import '../../../l10n/app_localizations.dart';
import '../../orders/domain/order_party.dart';
import '../../orders/domain/order_status.dart';
import '../../orders/domain/preferred_period.dart';
import '../../orders/domain/service_time.dart';
import '../../orders/presentation/order_labels.dart';

final _monthYear = DateFormat('MMM/yyyy', 'pt_BR');
final _dayMonth = DateFormat('dd/MM', 'pt_BR');

String agoLabel(AppLocalizations l10n, DateTime cityTime, DateTime cityNow) {
  final elapsed = cityNow.difference(cityTime);
  if (elapsed.inMinutes < 60) {
    return l10n.orderAgoMinutes(elapsed.inMinutes < 1 ? 1 : elapsed.inMinutes);
  }

  if (elapsed.inHours < 24) {
    return l10n.orderAgoHours(elapsed.inHours);
  }

  return l10n.orderAgoDays(elapsed.inDays);
}

String preferenceLine(AppLocalizations l10n, DateTime day, PreferredPeriod period, DateTime today) => l10n.orderPreferenceLine(
  capitalized(relativeDay(l10n, day, today)),
  periodLabel(l10n, period).toLowerCase(),
  periodHours(l10n, period),
);

String dayOption(AppLocalizations l10n, DateTime day, DateTime today) {
  final relative = relativeDay(l10n, day, today);
  return relative == shortDate(l10n, day)
      ? '${capitalized(weekdayShort(l10n, day))}, ${_dayMonth.format(day)}'
      : capitalized('$relative, ${shortDate(l10n, day)}');
}

String durationLabel(AppLocalizations l10n, int minutes) => switch ((minutes ~/ 60, minutes % 60)) {
  (0, final rest) => l10n.durationMinutes(rest),
  (final hours, 0) => l10n.durationHours(hours),
  (final hours, final rest) => l10n.durationHoursMinutes(hours, rest),
};

String memberSinceMonth(DateTime since) => _monthYear.format(since.toLocal()).replaceAll('.', '');

(String, VitrioBadgeTone) providerStatus(
  AppLocalizations l10n, {
  required OrderStatus status,
  required DateTime cityNow,
  ServiceTime? schedule,
  DateTime? quoteValidUntil,
  OrderParty? cancelledBy,
}) => switch (status) {
  OrderStatus.quoted when quoteValidUntil != null => (
    l10n.providerStatusQuoted(l10n.orderDayAndTime(shortDate(l10n, quoteValidUntil), formatTime(quoteValidUntil))),
    VitrioBadgeTone.info,
  ),
  OrderStatus.scheduled when schedule != null => (
    l10n.providerStatusScheduled(serviceWhen(l10n, schedule, cityNow)),
    VitrioBadgeTone.brand,
  ),
  OrderStatus.declined => (l10n.providerStatusDeclined, VitrioBadgeTone.neutral),
  OrderStatus.expired => (l10n.providerStatusExpired, VitrioBadgeTone.neutral),
  OrderStatus.cancelled => (
    switch (cancelledBy) {
      OrderParty.provider => l10n.providerStatusCancelledByYou,
      OrderParty.system => l10n.providerStatusCancelledBySystem,
      _ => l10n.providerStatusCancelledByCustomer,
    },
    VitrioBadgeTone.danger,
  ),
  _ => (statusLabel(l10n, status), statusTone(status)),
};
