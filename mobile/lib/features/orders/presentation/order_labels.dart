import 'package:intl/intl.dart';

import '../../../core/design_system/widgets/vitrio_badge_tone.dart';
import '../../../l10n/app_localizations.dart';
import '../domain/cancellation_reason.dart';
import '../domain/decline_reason.dart';
import '../domain/order_status.dart';
import '../domain/preferred_period.dart';
import '../domain/service_time.dart';

final _money = NumberFormat.currency(locale: 'pt_BR', symbol: r'R$');
final _dayMonth = DateFormat('dd/MM', 'pt_BR');
final _time = DateFormat('HH:mm', 'pt_BR');

String formatQuoteAmount(double amount) => _money.format(amount);

String formatTime(DateTime instant) => _time.format(instant.toLocal());

String weekdayShort(AppLocalizations l10n, DateTime day) => switch (day.weekday) {
      DateTime.monday => l10n.weekdayMonShort,
      DateTime.tuesday => l10n.weekdayTueShort,
      DateTime.wednesday => l10n.weekdayWedShort,
      DateTime.thursday => l10n.weekdayThuShort,
      DateTime.friday => l10n.weekdayFriShort,
      DateTime.saturday => l10n.weekdaySatShort,
      _ => l10n.weekdaySunShort,
    };

String shortDate(AppLocalizations l10n, DateTime day) => '${weekdayShort(l10n, day)} ${_dayMonth.format(day)}';

String relativeDay(AppLocalizations l10n, DateTime day, DateTime today) {
  final date = DateTime(day.year, day.month, day.day);
  final base = DateTime(today.year, today.month, today.day);
  return switch (date.difference(base).inDays) {
    0 => l10n.orderToday,
    1 => l10n.orderTomorrow,
    -1 => l10n.orderYesterday,
    _ => shortDate(l10n, date),
  };
}

String relativeMoment(AppLocalizations l10n, DateTime instant, DateTime now) {
  final local = instant.toLocal();
  return l10n.orderDayAndTime(relativeDay(l10n, local, now), _time.format(local));
}

String capitalized(String text) => text.isEmpty ? text : '${text[0].toUpperCase()}${text.substring(1)}';

String serviceRange(ServiceTime time) => '${time.start}–${time.end}';

String serviceWhen(AppLocalizations l10n, ServiceTime time, DateTime today) =>
    l10n.orderDayAndTime(relativeDay(l10n, time.date, today), serviceRange(time));

String periodLabel(AppLocalizations l10n, PreferredPeriod period) => switch (period) {
      PreferredPeriod.morning => l10n.orderPeriodMorning,
      PreferredPeriod.afternoon => l10n.orderPeriodAfternoon,
      PreferredPeriod.evening => l10n.orderPeriodEvening,
    };

String periodHours(AppLocalizations l10n, PreferredPeriod period) => switch (period) {
      PreferredPeriod.morning => l10n.orderPeriodMorningHours,
      PreferredPeriod.afternoon => l10n.orderPeriodAfternoonHours,
      PreferredPeriod.evening => l10n.orderPeriodEveningHours,
    };

String preferenceLabel(AppLocalizations l10n, DateTime day, PreferredPeriod period, DateTime today) {
  final when = capitalized(relativeDay(l10n, day, today));
  return switch (period) {
    PreferredPeriod.morning => l10n.orderPeriodMorningPhrase(when),
    PreferredPeriod.afternoon => l10n.orderPeriodAfternoonPhrase(when),
    PreferredPeriod.evening => l10n.orderPeriodEveningPhrase(when),
  };
}

String statusLabel(AppLocalizations l10n, OrderStatus status) => switch (status) {
      OrderStatus.sent => l10n.orderStatusSent,
      OrderStatus.quoted => l10n.orderStatusQuoted,
      OrderStatus.scheduled => l10n.orderStatusScheduled,
      OrderStatus.declined => l10n.orderStatusDeclined,
      OrderStatus.expired => l10n.orderStatusExpired,
      OrderStatus.quoteDeclined => l10n.orderStatusQuoteDeclined,
      OrderStatus.quoteExpired => l10n.orderStatusQuoteExpired,
      OrderStatus.cancelled => l10n.orderStatusCancelled,
      OrderStatus.completed => l10n.orderStatusCompleted,
      OrderStatus.paymentConfirmed => l10n.orderStatusPaymentConfirmed,
      OrderStatus.paymentDisputed => l10n.orderStatusPaymentDisputed,
      OrderStatus.paymentVoided => l10n.orderStatusPaymentVoided,
    };

VitrioBadgeTone statusTone(OrderStatus status) => switch (status) {
      OrderStatus.sent => VitrioBadgeTone.info,
      OrderStatus.quoted || OrderStatus.completed || OrderStatus.paymentConfirmed => VitrioBadgeTone.verified,
      OrderStatus.scheduled => VitrioBadgeTone.brand,
      OrderStatus.cancelled || OrderStatus.paymentDisputed => VitrioBadgeTone.danger,
      _ => VitrioBadgeTone.neutral,
    };

String declineReasonLabel(AppLocalizations l10n, DeclineReason reason) => switch (reason) {
      DeclineReason.noAvailability => l10n.orderDeclineNoAvailability,
      DeclineReason.outsideArea => l10n.orderDeclineOutsideArea,
      DeclineReason.notMyService => l10n.orderDeclineNotMyService,
      DeclineReason.other => l10n.orderReasonOther,
    };

String cancellationReasonLabel(AppLocalizations l10n, CancellationReason reason) => switch (reason) {
      CancellationReason.resolvedAnotherWay => l10n.orderCancelResolvedAnotherWay,
      CancellationReason.timeNoLongerWorks => l10n.orderCancelTimeNoLongerWorks,
      CancellationReason.newTimeDoesNotWork => l10n.orderCancelNewTimeDoesNotWork,
      CancellationReason.foundAnotherProfessional => l10n.orderCancelFoundAnotherProfessional,
      CancellationReason.unforeseen => l10n.orderCancelUnforeseen,
      CancellationReason.cannotMakeIt => l10n.orderCancelCannotMakeIt,
      CancellationReason.customerAskedToCancel => l10n.orderCancelCustomerAskedToCancel,
      CancellationReason.other => l10n.orderReasonOther,
      CancellationReason.accountDeleted => l10n.orderCancelAccountDeleted,
      CancellationReason.accountSuspended => l10n.orderCancelAccountSuspended,
    };

String? responseTimeLabel(AppLocalizations l10n, String? bucket) => switch (bucket) {
      '15min' => l10n.responseTime15min,
      '30min' => l10n.responseTime30min,
      '1h' => l10n.responseTime1h,
      '3h' => l10n.responseTime3h,
      '1d' => l10n.responseTime1d,
      _ => null,
    };
