import 'package:vitrio/core/network/api_result.dart';
import 'package:vitrio/core/network/problem_details.dart';
import 'package:vitrio/features/orders/domain/cancellation_reason.dart';
import 'package:vitrio/features/orders/domain/decline_reason.dart';
import 'package:vitrio/features/provider_orders/data/provider_orders_repository.dart';
import 'package:vitrio/features/provider_orders/domain/agenda_item.dart';
import 'package:vitrio/features/provider_orders/domain/provider_order.dart';
import 'package:vitrio/features/provider_orders/domain/provider_order_summary.dart';
import 'package:vitrio/features/provider_orders/domain/provider_order_tab.dart';
import 'package:vitrio/features/provider_orders/domain/provider_orders_page.dart';
import 'package:vitrio/features/provider_orders/domain/quote_draft.dart';

final class FakeProviderOrdersRepository implements ProviderOrdersRepository {
  final Map<String, ProviderOrder> orders = {};
  final Map<ProviderOrderTab, List<ProviderOrderSummary>> lists = {for (final tab in ProviderOrderTab.values) tab: []};
  final List<AgendaItem> agendaItems = [];
  final List<QuoteDraft> quotes = [];
  final List<String> idempotencyKeys = [];
  final List<String> calls = [];
  ProviderOrder? next;
  ApiFailure<ProviderOrder>? failure;

  @override
  Future<ApiResult<ProviderOrdersPage>> list({required ProviderOrderTab tab, required int page}) async =>
      ApiSuccess(ProviderOrdersPage(items: lists[tab]!, page: page, hasMore: false));

  @override
  Future<ApiResult<ProviderOrder>> get(String orderId) async =>
      orders[orderId] == null ? const ApiFailure(ProblemDetails(status: 404, code: 'order.not_found')) : ApiSuccess(orders[orderId]!);

  @override
  Future<ApiResult<ProviderOrder>> sendQuote(String orderId, QuoteDraft quote, {required String idempotencyKey}) async {
    quotes.add(quote);
    idempotencyKeys.add(idempotencyKey);
    return _act('quote', orderId);
  }

  @override
  Future<ApiResult<ProviderOrder>> decline(String orderId, {required DeclineReason reason, String? details}) async =>
      _act('decline:${reason.name}:${details ?? ''}', orderId);

  @override
  Future<ApiResult<ProviderOrder>> reschedule(String orderId, {required DateTime date, required String start, required int durationMinutes}) async =>
      _act('reschedule:${QuoteDraft.isoDate(date)}:$start:$durationMinutes', orderId);

  @override
  Future<ApiResult<ProviderOrder>> cancel(String orderId, {required CancellationReason reason, String? details}) async =>
      _act('cancel:${reason.name}:${details ?? ''}', orderId);

  @override
  Future<ApiResult<ProviderOrder>> complete(String orderId) async => _act('complete', orderId);

  @override
  Future<ApiResult<List<AgendaItem>>> agenda({required DateTime from, required DateTime to}) async {
    calls.add('agenda:${QuoteDraft.isoDate(from)}:${QuoteDraft.isoDate(to)}');
    return ApiSuccess([
      for (final item in agendaItems)
        if (!item.date.isBefore(from) && !item.date.isAfter(to)) item,
    ]);
  }

  ApiResult<ProviderOrder> _act(String call, String orderId) {
    calls.add(call);
    if (failure case final ApiFailure<ProviderOrder> problem) {
      return problem;
    }

    final result = next ?? orders[orderId]!;
    orders[orderId] = result;
    return ApiSuccess(result);
  }
}
