import '../../../core/network/api_result.dart';
import '../domain/favorites_page.dart';

abstract interface class FavoritesRepository {
  Future<ApiResult<FavoritesPage>> list({required int page});

  Future<ApiResult<void>> save(String providerId);

  Future<ApiResult<void>> remove(String providerId);
}
