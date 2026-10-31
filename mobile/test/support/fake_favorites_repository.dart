import 'package:vitrio/core/network/api_result.dart';
import 'package:vitrio/features/favorites/data/favorites_repository.dart';
import 'package:vitrio/features/favorites/domain/favorites_page.dart';
import 'package:vitrio/features/search/domain/provider_listing.dart';

final class FakeFavoritesRepository implements FavoritesRepository {
  final Map<String, ProviderListing> known = {};
  final List<String> saved = [];

  @override
  Future<ApiResult<FavoritesPage>> list({required int page}) async => ApiSuccess(FavoritesPage(
        items: [for (final id in saved.reversed) ?known[id]],
        page: page,
        hasMore: false,
      ));

  @override
  Future<ApiResult<void>> save(String providerId) async {
    if (!saved.contains(providerId)) {
      saved.add(providerId);
    }

    return const ApiSuccess(null);
  }

  @override
  Future<ApiResult<void>> remove(String providerId) async {
    saved.remove(providerId);
    return const ApiSuccess(null);
  }
}
