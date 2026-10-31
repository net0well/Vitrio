import '../../../core/network/api_result.dart';
import '../domain/home_feed.dart';
import '../domain/search_area.dart';
import '../domain/search_filters.dart';
import '../domain/search_page.dart';

abstract interface class SearchRepository {
  Future<ApiResult<SearchPage>> search(
    SearchArea area,
    SearchFilters filters, {
    required int page,
  });

  Future<ApiResult<HomeFeed>> home(SearchArea area);
}
