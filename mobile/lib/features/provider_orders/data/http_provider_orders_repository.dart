import '../../../core/network/api_client.dart';
import '../../../core/network/api_result.dart';
import '../../../core/network/json_body.dart';
import '../../orders/domain/cancellation_reason.dart';
import '../../orders/domain/decline_reason.dart';
import '../domain/agenda_item.dart';
import '../domain/provider_order.dart';
import '../domain/provider_order_tab.dart';
import '../domain/provider_orders_page.dart';
import '../domain/quote_draft.dart';
import 'provider_orders_repository.dart';

final class HttpProviderOrdersRepository implements ProviderOrdersRepository {
  const HttpProviderOrdersRepository(this._api);

  static const _orders = '/api/v1/provider/orders';
  static const _agenda = '/api/v1/provider/agenda';

  final ApiClient _api;

  @override
  Future<ApiResult<ProviderOrdersPage>> list({required ProviderOrderTab tab, required int page}) => _api.get(
    _orders,
    query: {'tab': tab.query, 'page': page.toString()},
    decode: (json) => ProviderOrdersPage.fromJson(jsonObject(json), page: page),
  );

  @override
  Future<ApiResult<ProviderOrder>> get(String orderId) => _api.get('$_orders/$orderId', decode: _order);

  @override
  Future<ApiResult<ProviderOrder>> sendQuote(String orderId, QuoteDraft quote, {required String idempotencyKey}) =>
      _api.post('$_orders/$orderId/quote', body: quote.toJson(), idempotencyKey: idempotencyKey, decode: _order);

  @override
  Future<ApiResult<ProviderOrder>> decline(String orderId, {required DeclineReason reason, String? details}) =>
      _api.post('$_orders/$orderId/decline', body: {'reason': reason.name, 'details': details}, decode: _order);

  @override
  Future<ApiResult<ProviderOrder>> reschedule(
    String orderId, {
    required DateTime date,
    required String start,
    required int durationMinutes,
  }) => _api.post(
    '$_orders/$orderId/reschedule',
    body: {'date': QuoteDraft.isoDate(date), 'start': start, 'durationMinutes': durationMinutes},
    decode: _order,
  );

  @override
  Future<ApiResult<ProviderOrder>> cancel(String orderId, {required CancellationReason reason, String? details}) =>
      _api.post('$_orders/$orderId/cancel', body: {'reason': reason.name, 'details': details}, decode: _order);

  @override
  Future<ApiResult<ProviderOrder>> complete(String orderId) => _api.post('$_orders/$orderId/complete', decode: _order);

  @override
  Future<ApiResult<List<AgendaItem>>> agenda({required DateTime from, required DateTime to}) => _api.get(
    _agenda,
    query: {'from': QuoteDraft.isoDate(from), 'to': QuoteDraft.isoDate(to)},
    decode: (json) => jsonObjects(json).map(AgendaItem.fromJson).toList(growable: false),
  );

  static ProviderOrder _order(Object? json) => ProviderOrder.fromJson(jsonObject(json));
}
