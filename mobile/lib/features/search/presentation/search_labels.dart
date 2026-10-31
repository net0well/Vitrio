import '../../../core/design_system/widgets/vitrio_badge.dart';
import '../../../core/design_system/widgets/vitrio_badge_tone.dart';
import '../../../core/presentation/brazilian_format.dart';
import '../../../l10n/app_localizations.dart';
import '../../location/domain/city_summary.dart';
import '../../provider/domain/price_unit.dart';
import '../../provider/presentation/provider_labels.dart';
import '../domain/provider_listing.dart';
import '../domain/search_filters.dart';
import '../domain/search_mode.dart';
import '../domain/search_sort.dart';

const _maxBadges = 3;

String cityLabel(CitySummary city) => '${city.name}, ${city.state}';

String sortLabel(AppLocalizations l10n, SearchSort sort) => switch (sort) {
      SearchSort.relevance => l10n.searchSortRelevance,
      SearchSort.distance => l10n.searchSortDistance,
      SearchSort.rating => l10n.searchSortRating,
      SearchSort.price => l10n.searchSortPrice,
    };

String resultsHeadline(
  AppLocalizations l10n,
  SearchFilters filters,
  SearchMode mode,
  CitySummary? city,
) {
  final category = filters.category?.name;
  final inCity = mode == SearchMode.city && city != null;
  return switch ((category, inCity)) {
    (final String name, true) => l10n.searchCategoryInCity(
        name,
        cityLabel(city!),
      ),
    (final String name, false) => l10n.searchCategoryNearMe(name),
    (null, true) => l10n.searchResultsInCity(cityLabel(city!)),
    (null, false) => l10n.searchResultsNearMe,
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
