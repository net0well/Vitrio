import 'listing_price.dart';

final class ProviderListing {
  const ProviderListing({
    required this.id,
    required this.displayName,
    required this.headline,
    required this.initials,
    required this.isNew,
    required this.cityName,
    required this.meiVerified,
    required this.availableToday,
    this.avatarUrl,
    this.ratingScore,
    this.reviewCount,
    this.distanceKm,
    this.responseTime,
    this.price,
  });

  factory ProviderListing.fromJson(Map<String, Object?> json) {
    final rating = json['rating'] as Map<String, Object?>?;
    final badges = json['badges'] as Map<String, Object?>? ?? const {};
    final price = json['price'] as Map<String, Object?>?;
    return ProviderListing(
      id: json['id'] as String,
      displayName: json['displayName'] as String,
      headline: json['headline'] as String,
      initials: json['initials'] as String,
      avatarUrl: json['avatarUrl'] as String?,
      ratingScore: (rating?['score'] as num?)?.toDouble(),
      reviewCount: rating?['count'] as int?,
      isNew: json['isNew'] != false,
      cityName: json['cityName'] as String,
      distanceKm: (json['distanceKm'] as num?)?.toDouble(),
      meiVerified: badges['meiVerified'] == true,
      availableToday: badges['availableToday'] == true,
      responseTime: json['responseTime'] as String?,
      price: price == null ? null : ListingPrice.fromJson(price),
    );
  }

  final String id;
  final String displayName;
  final String headline;
  final String initials;
  final String? avatarUrl;
  final double? ratingScore;
  final int? reviewCount;
  final bool isNew;
  final String cityName;
  final double? distanceKm;
  final bool meiVerified;
  final bool availableToday;
  final String? responseTime;
  final ListingPrice? price;
}
