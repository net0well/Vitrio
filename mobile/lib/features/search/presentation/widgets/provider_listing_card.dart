import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../app/routes.dart';
import '../../../../core/design_system/widgets/vitrio_provider_card.dart';
import '../../../../l10n/app_localizations.dart';
import '../../../provider/presentation/provider_queries.dart';
import '../../domain/provider_listing.dart';
import '../search_labels.dart';

class ProviderListingCard extends ConsumerWidget {
  const ProviderListingCard({super.key, required this.listing}) : compact = false;

  const ProviderListingCard.compact({super.key, required this.listing}) : compact = true;

  final ProviderListing listing;
  final bool compact;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final own = ref.watch(ownProviderIdProvider) == listing.id;
    void open() => PublicProviderRoute(id: listing.id).push<void>(context);
    if (compact) {
      return VitrioProviderCard.compact(
        name: listing.displayName,
        profession: listing.headline,
        seed: listing.id,
        initials: listing.initials,
        avatarUrl: listing.avatarUrl,
        verified: listing.meiVerified,
        rating: listingRating(listing),
        meta: listingMeta(l10n, listing),
        badges: listingBadges(l10n, listing, own: own),
        onTap: open,
      );
    }

    return VitrioProviderCard(
      name: listing.displayName,
      profession: listing.headline,
      seed: listing.id,
      initials: listing.initials,
      avatarUrl: listing.avatarUrl,
      verified: listing.meiVerified,
      rating: listingRating(listing),
      meta: listingMeta(l10n, listing),
      badges: listingBadges(l10n, listing, own: own),
      priceLabel: listingPriceLabel(l10n, listing),
      priceValue: listingPriceValue(l10n, listing),
      actionLabel: l10n.searchSeeProfile,
      onTap: open,
    );
  }
}
