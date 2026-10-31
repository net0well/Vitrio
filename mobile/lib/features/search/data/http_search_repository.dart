import '../../../core/network/api_client.dart';
import '../../../core/network/api_result.dart';
import '../../../core/network/json_body.dart';
import '../domain/home_feed.dart';
import '../domain/search_area.dart';
import '../domain/search_filters.dart';
import '../domain/search_page.dart';
import 'search_repository.dart';

final class HttpSearchRepository implements SearchRepository {
  const HttpSearchRepository(this._api);

  final ApiClient _api;

  @override
  Future<ApiResult<SearchPage>> search(
    SearchArea area,
    SearchFilters filters, {
    required int page,
  }) =>
      _api.get(
        '/api/v1/providers',
        query: {...area.toQuery(), ...filters.toQuery(), 'page': page.toString()},
        decode: (json) => SearchPage.fromJson(jsonObject(json)),
      );

  @override
  Future<ApiResult<HomeFeed>> home(SearchArea area) => _api.get(
        '/api/v1/home',
        query: area.toQuery(),
        decode: (json) => HomeFeed.fromJson(jsonObject(json)),
      );
}
