import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/network/api_result.dart';
import '../../../core/presentation/no_retry.dart';
import '../data/provider_orders_providers.dart';
import '../domain/provider_order_tab.dart';
import '../domain/provider_orders_page.dart';

part 'provider_orders_controller.g.dart';

@Riverpod(retry: noRetry)
class ProviderOrdersController extends _$ProviderOrdersController {
  static const lastPage = 50;

  bool _loadingMore = false;

  @override
  Future<ProviderOrdersPage> build(ProviderOrderTab tab) async =>
      switch (await ref.read(providerOrdersRepositoryProvider).list(tab: tab, page: 1)) {
        ApiSuccess(:final data) => data,
        ApiFailure(:final problem) => throw problem,
      };

  Future<void> loadMore() async {
    final current = state.value;
    if (current == null || !current.hasMore || _loadingMore || current.page >= lastPage) {
      return;
    }

    _loadingMore = true;
    final next = await ref.read(providerOrdersRepositoryProvider).list(tab: tab, page: current.page + 1);
    _loadingMore = false;
    if (next case ApiSuccess(:final data)) {
      state = AsyncData(ProviderOrdersPage(items: [...current.items, ...data.items], page: data.page, hasMore: data.hasMore));
    }
  }
}
