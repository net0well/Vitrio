import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/network/api_result.dart';
import '../../../core/presentation/no_retry.dart';
import '../../orders/domain/cancellation_reason.dart';
import '../../orders/domain/decline_reason.dart';
import '../../provider/presentation/provider_queries.dart';
import '../data/provider_orders_providers.dart';
import '../data/provider_orders_repository.dart';
import '../domain/provider_order.dart';
import '../domain/quote_draft.dart';
import 'provider_agenda_queries.dart';
import 'provider_orders_controller.dart';

part 'provider_order_controller.g.dart';

@Riverpod(retry: noRetry)
class ProviderOrderController extends _$ProviderOrderController {
  @override
  Future<ProviderOrder> build(String orderId) async => switch (await _repository.get(orderId)) {
    ApiSuccess(:final data) => data,
    ApiFailure(:final problem) => throw problem,
  };

  ProviderOrdersRepository get _repository => ref.read(providerOrdersRepositoryProvider);

  Future<ApiResult<ProviderOrder>> sendQuote(QuoteDraft quote, {required String idempotencyKey}) =>
      _apply(_repository.sendQuote(orderId, quote, idempotencyKey: idempotencyKey));

  Future<ApiResult<ProviderOrder>> decline(DeclineReason reason, String? details) =>
      _apply(_repository.decline(orderId, reason: reason, details: details));

  Future<ApiResult<ProviderOrder>> reschedule({required DateTime date, required String start, required int durationMinutes}) =>
      _apply(_repository.reschedule(orderId, date: date, start: start, durationMinutes: durationMinutes));

  Future<ApiResult<ProviderOrder>> cancel(CancellationReason reason, String? details) =>
      _apply(_repository.cancel(orderId, reason: reason, details: details));

  Future<ApiResult<ProviderOrder>> complete() => _apply(_repository.complete(orderId));

  Future<ApiResult<ProviderOrder>> _apply(Future<ApiResult<ProviderOrder>> action) async {
    final result = await action;
    switch (result) {
      case ApiSuccess(:final data):
        state = AsyncData(data);
        ref.invalidate(providerOrdersControllerProvider);
        ref.invalidate(providerDashboardProvider);
        ref.invalidate(providerAgendaProvider);
      case ApiFailure(:final problem) when problem.status == 409:
        ref.invalidateSelf();
        ref.invalidate(providerOrdersControllerProvider);
        ref.invalidate(providerDashboardProvider);
      case ApiFailure():
        break;
    }

    return result;
  }
}
