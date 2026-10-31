import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../provider/domain/service_category.dart';
import '../domain/search_filters.dart';
import '../domain/search_sort.dart';

part 'search_filters_controller.g.dart';

@Riverpod(keepAlive: true)
class SearchFiltersController extends _$SearchFiltersController {
  @override
  SearchFilters build() => SearchFilters.empty;

  void apply(SearchFilters filters) => state = filters;

  void search(String query) => state = state.copyWith(query: query);

  void startWithCategory(ServiceCategory category) => state = SearchFilters(category: category);

  void startWithAvailableToday() => state = const SearchFilters(availableToday: true);

  void startWithTopRated() => state = const SearchFilters(sort: SearchSort.rating);

  void removeCategory() => state = state.copyWith(category: () => null);

  void sortBy(SearchSort sort) => state = state.copyWith(sort: sort);

  void toggleSort(SearchSort sort) => state = state.copyWith(
        sort: state.sort == sort ? SearchSort.relevance : sort,
      );

  void toggleTopRatedOnly() => state = state.copyWith(
        minRating: () => state.minRating == topRatedMinimum ? null : topRatedMinimum,
      );

  void toggleMeiVerified() => state = state.copyWith(meiVerified: !state.meiVerified);

  void toggleAvailableToday() => state = state.copyWith(availableToday: !state.availableToday);

  static const topRatedMinimum = 4.5;
}
