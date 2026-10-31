import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/network/api_result.dart';
import '../../../core/presentation/no_retry.dart';
import '../data/favorites_providers.dart';
import '../domain/favorites_page.dart';

part 'favorites_controller.g.dart';

@Riverpod(retry: noRetry)
class FavoritesController extends _$FavoritesController {
  static const lastPage = 10;

  bool _loadingMore = false;

  @override
  Future<FavoritesPage> build() async => switch (await ref.read(favoritesRepositoryProvider).list(page: 1)) {
        ApiSuccess(:final data) => data,
        ApiFailure(:final problem) => throw problem,
      };

  Future<void> loadMore() async {
    final current = state.value;
    if (current == null || !current.hasMore || _loadingMore || current.page >= lastPage) {
      return;
    }

    _loadingMore = true;
    final next = await ref.read(favoritesRepositoryProvider).list(page: current.page + 1);
    _loadingMore = false;
    if (next case ApiSuccess(:final data)) {
      state = AsyncData(
        FavoritesPage(
          items: [...current.items, ...data.items],
          page: data.page,
          hasMore: data.hasMore,
        ),
      );
    }
  }
}
