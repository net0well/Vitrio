import '../../../core/network/json_body.dart';
import '../../location/domain/city_summary.dart';
import '../../provider/domain/service_category.dart';
import 'provider_listing.dart';

final class HomeFeed {
  const HomeFeed({
    required this.categories,
    required this.topRated,
    required this.hasProviders,
    this.city,
  });

  factory HomeFeed.fromJson(Map<String, Object?> json) {
    final city = json['city'] as Map<String, Object?>?;
    return HomeFeed(
      city: city == null ? null : CitySummary.fromJson(city),
      categories: jsonObjects(json['categories']).map(ServiceCategory.fromJson).toList(growable: false),
      topRated: jsonObjects(json['topRated']).map(ProviderListing.fromJson).toList(growable: false),
      hasProviders: json['hasProviders'] == true,
    );
  }

  final CitySummary? city;
  final List<ServiceCategory> categories;
  final List<ProviderListing> topRated;
  final bool hasProviders;
}
