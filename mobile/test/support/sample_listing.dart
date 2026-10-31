import 'package:vitrio/features/provider/domain/price_unit.dart';
import 'package:vitrio/features/search/domain/listing_price.dart';
import 'package:vitrio/features/search/domain/provider_listing.dart';

ProviderListing sampleListing({
  String id = 'listing-1',
  String displayName = 'Carlos Menezes',
  String headline = 'Eletricista residencial',
  double? ratingScore = 4.9,
  int? reviewCount = 127,
  double? distanceKm = 1.2,
  bool meiVerified = true,
  bool availableToday = true,
  ListingPrice? price = const ListingPrice(from: 80, unit: PriceUnit.perService),
}) =>
    ProviderListing(
      id: id,
      displayName: displayName,
      headline: headline,
      initials: displayName.split(' ').take(2).map((part) => part[0]).join(),
      isNew: ratingScore == null,
      cityName: 'Jaboticabal',
      meiVerified: meiVerified,
      availableToday: availableToday,
      ratingScore: ratingScore,
      reviewCount: reviewCount,
      distanceKm: distanceKm,
      price: price,
    );
