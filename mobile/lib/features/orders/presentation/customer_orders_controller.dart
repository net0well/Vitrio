import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/network/api_result.dart';
import '../../../core/presentation/no_retry.dart';
import '../data/orders_providers.dart';
import '../domain/customer_order_summary.dart';
import '../domain/customer_orders_page.dart';

part 'customer_orders_controller.g.dart';

@Riverpod(retry: noRetry)
class CustomerOrdersController extends _$CustomerOrdersController {
  static const lastPage = 50;

  bool _loadingMore = false;

  @override
  Future<CustomerOrdersPage> build({required bool open}) async =>
      switch (await ref.read(ordersRepositoryProvider).list(open: open, page: 1)) {
        ApiSuccess(:final data) => data,
        ApiFailure(:final problem) => throw problem,
      };

  Future<void> loadMore() async {
    final current = state.value;
    if (current == null || !current.hasMore || _loadingMore || current.page >= lastPage) {
      return;
    }

    _loadingMore = true;
    final next = await ref.read(ordersRepositoryProvider).list(open: open, page: current.page + 1);
    _loadingMore = false;
    if (next case ApiSuccess(:final data)) {
      state = AsyncData(CustomerOrdersPage(items: [...current.items, ...data.items], page: data.page, hasMore: data.hasMore));
    }
  }
}

@Riverpod(retry: noRetry)
Future<CustomerOrderSummary?> latestOpenOrder(Ref ref) async =>
    switch (await ref.read(ordersRepositoryProvider).list(open: true, page: 1)) {
      ApiSuccess(:final data) => data.items.firstOrNull,
      ApiFailure() => null,
    };
