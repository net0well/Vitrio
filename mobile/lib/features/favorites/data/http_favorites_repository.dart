import '../../../core/network/api_client.dart';
import '../../../core/network/api_result.dart';
import '../../../core/network/json_body.dart';
import '../domain/favorites_page.dart';
import 'favorites_repository.dart';

final class HttpFavoritesRepository implements FavoritesRepository {
  const HttpFavoritesRepository(this._api);

  static const _favorites = '/api/v1/me/favorites';

  final ApiClient _api;

  @override
  Future<ApiResult<FavoritesPage>> list({required int page}) => _api.get(
        _favorites,
        query: {'page': page.toString()},
        decode: (json) => FavoritesPage.fromJson(jsonObject(json)),
      );

  @override
  Future<ApiResult<void>> save(String providerId) => _api.put('$_favorites/$providerId', decode: _ignore);

  @override
  Future<ApiResult<void>> remove(String providerId) => _api.delete('$_favorites/$providerId', decode: _ignore);

  static void _ignore(Object? json) {}
}
