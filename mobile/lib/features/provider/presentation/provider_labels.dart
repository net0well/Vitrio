import 'package:intl/intl.dart';

import '../../../l10n/app_localizations.dart';
import '../domain/price_unit.dart';
import '../domain/provider_service.dart';
import '../domain/provider_status.dart';

final _currency = NumberFormat.currency(locale: 'pt_BR', symbol: r'R$');

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
