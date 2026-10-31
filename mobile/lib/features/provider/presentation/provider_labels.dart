import 'package:intl/intl.dart';

import '../../../l10n/app_localizations.dart';
import '../domain/availability_slot.dart';
import '../domain/price_unit.dart';
import '../domain/provider_service.dart';
import '../domain/provider_status.dart';

final _currency = NumberFormat.currency(locale: 'pt_BR', symbol: r'R$');
final _wholeCurrency = NumberFormat.currency(locale: 'pt_BR', symbol: r'R$', decimalDigits: 0);

String unitLabel(AppLocalizations l10n, PriceUnit unit) => switch (unit) {
      PriceUnit.perService => l10n.providerUnitPerService,
      PriceUnit.perHour => l10n.providerUnitPerHour,
      PriceUnit.perDay => l10n.providerUnitPerDay,
      PriceUnit.perSquareMeter => l10n.providerUnitPerSquareMeter,
      PriceUnit.perUnit => l10n.providerUnitPerUnit,
    };

String priceText(AppLocalizations l10n, ProviderService service) => service.onQuote || service.priceFrom == null
    ? l10n.providerServiceOnQuote
    : l10n.providerPriceFrom(_currency.format(service.priceFrom), unitLabel(l10n, service.unit));

String? priceValue(AppLocalizations l10n, ProviderService service) {
  final price = service.priceFrom;
  if (service.onQuote || price == null) {
    return null;
  }

  final amount = price == price.roundToDouble() ? _wholeCurrency.format(price) : _currency.format(price);
  return service.unit == PriceUnit.perService ? amount : '$amount/${unitLabel(l10n, service.unit)}';
}

String dayLabel(AppLocalizations l10n, String day) => switch (day) {
      'monday' => l10n.providerDayMonday,
      'tuesday' => l10n.providerDayTuesday,
      'wednesday' => l10n.providerDayWednesday,
      'thursday' => l10n.providerDayThursday,
      'friday' => l10n.providerDayFriday,
      'saturday' => l10n.providerDaySaturday,
      _ => l10n.providerDaySunday,
    };

String statusLabel(AppLocalizations l10n, ProviderStatus status) => switch (status) {
      ProviderStatus.published => l10n.providerStatusPublished,
      ProviderStatus.paused => l10n.providerStatusPaused,
      ProviderStatus.suspended || ProviderStatus.removed => l10n.providerStatusSuspended,
      ProviderStatus.draft => l10n.providerStatusDraft,
    };

String missingLabel(AppLocalizations l10n, String code) => switch (code) {
      'about' => l10n.providerMissingAbout,
      'categories' => l10n.providerMissingCategories,
      'serviceArea' => l10n.providerMissingServiceArea,
      'services' => l10n.providerMissingServices,
      _ => l10n.providerMissingContactPhone,
    };

String phoneText(String? phone) {
  if (phone == null || !phone.startsWith('+55') || phone.length != 14) {
    return phone ?? '';
  }

  final digits = phone.substring(3);
  return '(${digits.substring(0, 2)}) ${digits.substring(2, 7)}-${digits.substring(7)}';
}

String cnpjText(String? cnpj) {
  final digits = (cnpj ?? '').replaceAll(RegExp(r'\D'), '');
  final text = StringBuffer();
  for (var index = 0; index < digits.length && index < 14; index++) {
    text
      ..write(switch (index) { 2 || 5 => '.', 8 => '/', 12 => '-', _ => '' })
      ..write(digits[index]);
  }

  return text.toString();
}

String _dayShort(AppLocalizations l10n, String day) => switch (day) {
      'monday' => l10n.providerDayShortMonday,
      'tuesday' => l10n.providerDayShortTuesday,
      'wednesday' => l10n.providerDayShortWednesday,
      'thursday' => l10n.providerDayShortThursday,
      'friday' => l10n.providerDayShortFriday,
      'saturday' => l10n.providerDayShortSaturday,
      _ => l10n.providerDayShortSunday,
    };

String _hour(String time) {
  final [hours, minutes] = time.split(':');
  final hour = int.parse(hours);
  return minutes == '00' ? '${hour}h' : '${hour}h$minutes';
}

String? availabilitySummary(AppLocalizations l10n, List<AvailabilitySlot> slots, {required bool emergencies}) {
  final byDay = {for (final slot in slots) slot.dayOfWeek: slot};
  final groups = <({List<String> days, String start, String end})>[];
  for (final day in AvailabilitySlot.days) {
    final slot = byDay[day];
    if (slot == null) {
      continue;
    }

    final previous = groups.lastOrNull;
    final previousIndex = previous == null ? -2 : AvailabilitySlot.days.indexOf(previous.days.last);
    if (previous != null && previous.start == slot.start && previous.end == slot.end && previousIndex == AvailabilitySlot.days.indexOf(day) - 1) {
      previous.days.add(day);
    } else {
      groups.add((days: [day], start: slot.start, end: slot.end));
    }
  }

  final parts = [
    for (final group in groups)
      '${group.days.length == 1 ? _dayShort(l10n, group.days.first) : l10n.providerDaysRange(_dayShort(l10n, group.days.first), _dayShort(l10n, group.days.last))}, '
          '${l10n.providerHoursRange(_hour(group.start), _hour(group.end))}',
    if (emergencies) l10n.providerEmergenciesShort,
  ];
  if (parts.isEmpty) {
    return null;
  }

  final text = parts.join(' · ');
  return text[0].toUpperCase() + text.substring(1);
}
