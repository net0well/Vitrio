import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/network/api_result.dart';
import '../../../core/presentation/no_retry.dart';
import '../../search/domain/provider_listing.dart';
import '../data/orders_providers.dart';
import '../data/orders_repository.dart';
import '../domain/cancellation_reason.dart';
import '../domain/customer_order.dart';
import 'customer_orders_controller.dart';

part 'customer_order_controller.g.dart';

@Riverpod(retry: noRetry)
class CustomerOrderController extends _$CustomerOrderController {
  @override
  Future<CustomerOrder> build(String orderId) async => switch (await _repository.get(orderId)) {
        ApiSuccess(:final data) => data,
        ApiFailure(:final problem) => throw problem,
      };

  OrdersRepository get _repository => ref.read(ordersRepositoryProvider);

  Future<ApiResult<CustomerOrder>> acceptQuote(int quoteVersion) => _apply(_repository.acceptQuote(orderId, quoteVersion: quoteVersion));

  Future<ApiResult<CustomerOrder>> declineQuote() => _apply(_repository.declineQuote(orderId));

  Future<ApiResult<CustomerOrder>> cancel(CancellationReason reason, String? details) =>
      _apply(_repository.cancel(orderId, reason: reason, details: details));

  Future<ApiResult<CustomerOrder>> complete() => _apply(_repository.complete(orderId));

  Future<ApiResult<CustomerOrder>> _apply(Future<ApiResult<CustomerOrder>> action) async {
    final result = await action;
    switch (result) {
      case ApiSuccess(:final data):
        state = AsyncData(data);
        ref.invalidate(customerOrdersControllerProvider);
        ref.invalidate(latestOpenOrderProvider);
      case ApiFailure(:final problem) when problem.status == 409:
        ref.invalidateSelf();
      case ApiFailure():
        break;
    }

    return result;
  }
}

@Riverpod(retry: noRetry)
Future<List<ProviderListing>> orderSuggestions(Ref ref, String orderId) async =>
    switch (await ref.read(ordersRepositoryProvider).suggestions(orderId)) {
      ApiSuccess(:final data) => data,
      ApiFailure(:final problem) when problem.status == 409 => const [],
      ApiFailure(:final problem) => throw problem,
    };
