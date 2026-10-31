import 'package:vitrio/core/network/api_result.dart';
import 'package:vitrio/core/network/problem_details.dart';
import 'package:vitrio/features/provider/domain/service_category.dart';
import 'package:vitrio/features/search/data/search_repository.dart';
import 'package:vitrio/features/search/domain/home_feed.dart';
import 'package:vitrio/features/search/domain/provider_listing.dart';
import 'package:vitrio/features/search/domain/search_area.dart';
import 'package:vitrio/features/search/domain/search_filters.dart';
import 'package:vitrio/features/search/domain/search_mode.dart';
import 'package:vitrio/features/search/domain/search_page.dart';

import 'fake_cities_repository.dart';

final class FakeSearchRepository implements SearchRepository {
  List<ProviderListing> listings = [];
  List<ProviderListing> topRated = [];
  List<ServiceCategory> categories = [];
  bool hasProviders = true;
  bool hasMore = false;
  ProblemDetails? failure;
  final List<(SearchArea, SearchFilters, int)> searches = [];

  @override
  Future<ApiResult<SearchPage>> search(SearchArea area, SearchFilters filters, {required int page}) async {
    searches.add((area, filters, page));
    if (failure case final problem?) {
      return ApiFailure(problem);
    }

    return ApiSuccess(SearchPage(
      items: page == 1 ? listings : const [],
      page: page,
      hasMore: page == 1 && hasMore,
      mode: area.hasPoint ? SearchMode.nearMe : SearchMode.city,
      city: jaboticabal,
    ));
  }

  @override
  Future<ApiResult<HomeFeed>> home(SearchArea area) async => failure != null
      ? ApiFailure(failure!)
      : ApiSuccess(HomeFeed(city: jaboticabal, categories: categories, topRated: topRated, hasProviders: hasProviders));
}
