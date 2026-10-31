import '../../location/domain/city_summary.dart';
import '../domain/professional_name.dart';
import '../domain/provider_listing.dart';
import '../domain/search_mode.dart';
import '../domain/search_page.dart';

final class SearchResults {
  const SearchResults({
    required this.items,
    required this.page,
    required this.hasMore,
    required this.mode,
    required this.count,
    this.moreThanCount = false,
    this.city,
    this.professional,
    this.loadingMore = false,
    this.loadMoreFailed = false,
  });

  factory SearchResults.first(SearchPage page) => SearchResults(
        items: page.items,
        page: page.page,
        hasMore: page.hasMore,
        mode: page.mode,
        count: page.count,
        moreThanCount: page.moreThanCount,
        city: page.city,
        professional: page.professional,
      );

  final List<ProviderListing> items;
  final int page;
  final bool hasMore;
  final SearchMode mode;
  final int count;
  final bool moreThanCount;
  final CitySummary? city;
  final ProfessionalName? professional;
  final bool loadingMore;
  final bool loadMoreFailed;

  SearchResults append(SearchPage next) => _with(items: [...items, ...next.items], page: next.page, hasMore: next.hasMore);

  SearchResults loading() => _with(loadingMore: true);

  SearchResults failedToLoadMore() => _with(loadMoreFailed: true);

  SearchResults _with({
    List<ProviderListing>? items,
    int? page,
    bool? hasMore,
    bool loadingMore = false,
    bool loadMoreFailed = false,
  }) =>
      SearchResults(
        items: items ?? this.items,
        page: page ?? this.page,
        hasMore: hasMore ?? this.hasMore,
        mode: mode,
        count: count,
        moreThanCount: moreThanCount,
        city: city,
        professional: professional,
        loadingMore: loadingMore,
        loadMoreFailed: loadMoreFailed,
      );
}
