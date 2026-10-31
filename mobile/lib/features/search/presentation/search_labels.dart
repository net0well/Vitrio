import '../../../core/design_system/widgets/vitrio_badge.dart';
import '../../../core/design_system/widgets/vitrio_badge_tone.dart';
import '../../../core/presentation/brazilian_format.dart';
import '../../../l10n/app_localizations.dart';
import '../../location/domain/city_summary.dart';
import '../../provider/domain/price_unit.dart';
import '../../provider/presentation/provider_labels.dart';
import '../domain/professional_name.dart';
import '../domain/provider_listing.dart';
import '../domain/search_mode.dart';
import '../domain/search_sort.dart';
import 'search_results.dart';

const _maxBadges = 3;

String cityLabel(CitySummary city) => '${city.name}, ${city.state}';

String sortLabel(AppLocalizations l10n, SearchSort sort) => switch (sort) {
      SearchSort.relevance => l10n.searchSortRelevance,
      SearchSort.distance => l10n.searchSortDistance,
      SearchSort.rating => l10n.searchSortRating,
      SearchSort.price => l10n.searchSortPrice,
    };

String resultsHeadline(AppLocalizations l10n, SearchResults results) {
  final one = results.count == 1 && !results.moreThanCount;
  final professionals = switch ((results.professional, one)) {
    (final ProfessionalName name, true) => name.singular,
    (final ProfessionalName name, false) => name.plural,
    (null, true) => l10n.searchProfessionalOne,
    (null, false) => l10n.searchProfessionalMany,
  };
  final count = results.count.toString();
  final city = results.mode == SearchMode.city ? results.city : null;
  return switch ((results.moreThanCount, city)) {
    (false, null) => l10n.searchCountNearMe(count, professionals),
    (false, final CitySummary city) => l10n.searchCountInCity(count, professionals, cityLabel(city)),
    (true, null) => l10n.searchMoreThanNearMe(count, professionals),
    (true, final CitySummary city) => l10n.searchMoreThanInCity(count, professionals, cityLabel(city)),
  };
}

String? listingRating(ProviderListing listing) => listing.isNew || listing.ratingScore == null ? null : formatRating(listing.ratingScore!);

String listingMeta(AppLocalizations l10n, ProviderListing listing) => [
      if (listingRating(listing) != null && listing.reviewCount != null) l10n.searchReviewCount(listing.reviewCount.toString()),
      if (listing.distanceKm case final km?) l10n.providerDistance(formatDistance(km)),
      listing.cityName,
    ].join(' · ');

List<VitrioBadge> listingBadges(
  AppLocalizations l10n,
  ProviderListing listing,
) =>
    [
      if (listing.meiVerified) VitrioBadge(label: l10n.providerBadgeMei, tone: VitrioBadgeTone.verified),
      if (listing.availableToday) VitrioBadge(label: l10n.providerBadgeToday),
      if (listing.isNew) VitrioBadge(label: l10n.providerNew, tone: VitrioBadgeTone.warning),
    ].take(_maxBadges).toList(growable: false);

String listingPriceLabel(AppLocalizations l10n, ProviderListing listing) =>
    listing.price == null ? l10n.providerPriceLabel : l10n.searchPriceFrom;

String listingPriceValue(AppLocalizations l10n, ProviderListing listing) {
  final price = listing.price;
  if (price == null) {
    return l10n.providerServiceOnQuote;
  }

  final amount = formatMoney(price.from);
  return price.unit == PriceUnit.perService ? amount : '$amount/${unitLabel(l10n, price.unit)}';
}
