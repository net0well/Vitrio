import '../../../core/network/api_result.dart';
import '../../provider/domain/photo_upload.dart';
import '../../search/domain/provider_listing.dart';
import '../domain/cancellation_reason.dart';
import '../domain/customer_order.dart';
import '../domain/customer_orders_page.dart';
import '../domain/new_order.dart';
import '../domain/order_photo_copy.dart';

abstract interface class OrdersRepository {
  Future<ApiResult<PhotoUpload>> photoUpload({required String contentType, required int contentLength});

  Future<ApiResult<CustomerOrder>> create(NewOrder order, {required String idempotencyKey});

  Future<ApiResult<List<OrderPhotoCopy>>> copyPhotos(String orderId);

  Future<ApiResult<CustomerOrdersPage>> list({required bool open, required int page});

  Future<ApiResult<CustomerOrder>> get(String orderId);

  Future<ApiResult<List<ProviderListing>>> suggestions(String orderId);

  Future<ApiResult<CustomerOrder>> acceptQuote(String orderId, {required int quoteVersion});

  Future<ApiResult<CustomerOrder>> declineQuote(String orderId);

  Future<ApiResult<CustomerOrder>> cancel(String orderId, {required CancellationReason reason, String? details});

  Future<ApiResult<CustomerOrder>> complete(String orderId);
}
