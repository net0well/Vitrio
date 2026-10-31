import '../../../core/network/json_body.dart';
import '../../search/domain/provider_listing.dart';

final class FavoritesPage {
  const FavoritesPage({
    required this.items,
    required this.page,
    required this.hasMore,
  });

  factory FavoritesPage.fromJson(Map<String, Object?> json) => FavoritesPage(
        items: jsonObjects(json['items']).map(ProviderListing.fromJson).toList(growable: false),
        page: json['page'] as int,
        hasMore: json['hasMore'] == true,
      );

  final List<ProviderListing> items;
  final int page;
  final bool hasMore;
}
