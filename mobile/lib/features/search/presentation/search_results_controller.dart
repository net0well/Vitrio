import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/network/api_result.dart';
import '../../../core/presentation/no_retry.dart';
import '../data/search_providers.dart';
import '../domain/search_area.dart';
import '../domain/search_filters.dart';
import 'search_filters_controller.dart';
import 'search_queries.dart';
import 'search_results.dart';

part 'search_results_controller.g.dart';

@Riverpod(retry: noRetry)
class SearchResultsController extends _$SearchResultsController {
  static const lastPage = 10;

  @override
  Future<SearchResults?> build() async {
    final area = await ref.watch(searchAreaProvider.future);
    final filters = ref.watch(searchFiltersControllerProvider);
    if (area == null) {
      return null;
    }

    return switch (await ref.read(searchRepositoryProvider).search(area, _effective(filters, area), page: 1)) {
      ApiSuccess(:final data) => SearchResults.first(data),
      ApiFailure(:final problem) => throw problem,
    };
  }

  Future<void> loadMore() async {
    final current = state.value;
    final area = ref.read(searchAreaProvider).value;
    if (current == null || area == null || !current.hasMore || current.loadingMore || current.page >= lastPage) {
      return;
    }

    final filters = ref.read(searchFiltersControllerProvider);
    state = AsyncData(current.loading());
    final next = await ref.read(searchRepositoryProvider).search(area, _effective(filters, area), page: current.page + 1);
    if (filters != ref.read(searchFiltersControllerProvider)) {
      return;
    }

    state = AsyncData(switch (next) {
      ApiSuccess(:final data) => current.append(data),
      ApiFailure() => current.failedToLoadMore(),
    });
  }

  static SearchFilters _effective(SearchFilters filters, SearchArea area) => area.hasPoint ? filters : filters.withoutLocationOnly();
}
