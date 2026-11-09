import '../../../core/network/api_client.dart';
import '../../../core/network/api_result.dart';
import '../../../core/network/json_body.dart';
import '../../provider/domain/photo_upload.dart';
import '../../search/domain/provider_listing.dart';
import '../domain/cancellation_reason.dart';
import '../domain/customer_order.dart';
import '../domain/customer_orders_page.dart';
import '../domain/new_order.dart';
import '../domain/order_photo_copy.dart';
import 'orders_repository.dart';

final class HttpOrdersRepository implements OrdersRepository {
  const HttpOrdersRepository(this._api);

  static const _orders = '/api/v1/orders';

  final ApiClient _api;

  @override
  Future<ApiResult<PhotoUpload>> photoUpload({required String contentType, required int contentLength}) => _api.post(
        '$_orders/photos/upload-url',
        body: {'contentType': contentType, 'contentLength': contentLength},
        decode: (json) => PhotoUpload.fromJson(jsonObject(json)),
      );

  @override
  Future<ApiResult<CustomerOrder>> create(NewOrder order, {required String idempotencyKey}) =>
      _api.post(_orders, body: order.toJson(), idempotencyKey: idempotencyKey, decode: _order);

  @override
  Future<ApiResult<List<OrderPhotoCopy>>> copyPhotos(String orderId) => _api.post(
        '$_orders/$orderId/photos/copies',
        decode: (json) => jsonObjects(json).map(OrderPhotoCopy.fromJson).toList(growable: false),
      );

  @override
  Future<ApiResult<CustomerOrdersPage>> list({required bool open, required int page}) => _api.get(
        _orders,
        query: {'status': open ? 'open' : 'closed', 'page': page.toString()},
        decode: (json) => CustomerOrdersPage.fromJson(jsonObject(json), page: page),
      );

  @override
  Future<ApiResult<CustomerOrder>> get(String orderId) => _api.get('$_orders/$orderId', decode: _order);

  @override
  Future<ApiResult<List<ProviderListing>>> suggestions(String orderId) => _api.get(
        '$_orders/$orderId/suggestions',
        decode: (json) => jsonObjects(json).map(ProviderListing.fromJson).toList(growable: false),
      );

  @override
  Future<ApiResult<CustomerOrder>> acceptQuote(String orderId, {required int quoteVersion}) =>
      _api.post('$_orders/$orderId/quote/accept', body: {'quoteVersion': quoteVersion}, decode: _order);

  @override
  Future<ApiResult<CustomerOrder>> declineQuote(String orderId) => _api.post('$_orders/$orderId/quote/decline', decode: _order);

  @override
  Future<ApiResult<CustomerOrder>> cancel(String orderId, {required CancellationReason reason, String? details}) =>
      _api.post('$_orders/$orderId/cancel', body: {'reason': reason.name, 'details': details}, decode: _order);

  @override
  Future<ApiResult<CustomerOrder>> complete(String orderId) => _api.post('$_orders/$orderId/complete', decode: _order);

  static CustomerOrder _order(Object? json) => CustomerOrder.fromJson(jsonObject(json));
}
