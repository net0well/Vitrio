import '../../location/domain/city_summary.dart';
import '../domain/provider_listing.dart';
import '../domain/search_mode.dart';
import '../domain/search_page.dart';

final class SearchResults {
  const SearchResults({
    required this.items,
    required this.page,
    required this.hasMore,
    required this.mode,
    this.city,
    this.loadingMore = false,
    this.loadMoreFailed = false,
  });

  factory SearchResults.first(SearchPage page) => SearchResults(
        items: page.items,
        page: page.page,
        hasMore: page.hasMore,
        mode: page.mode,
        city: page.city,
      );

  final List<ProviderListing> items;
  final int page;
  final bool hasMore;
  final SearchMode mode;
  final CitySummary? city;
  final bool loadingMore;
  final bool loadMoreFailed;

  SearchResults append(SearchPage next) => SearchResults(
        items: [...items, ...next.items],
        page: next.page,
        hasMore: next.hasMore,
        mode: mode,
        city: city,
      );

  SearchResults loading() => SearchResults(
        items: items,
        page: page,
        hasMore: hasMore,
        mode: mode,
        city: city,
        loadingMore: true,
      );

  SearchResults failedToLoadMore() => SearchResults(
        items: items,
        page: page,
        hasMore: hasMore,
        mode: mode,
        city: city,
        loadMoreFailed: true,
      );
}
