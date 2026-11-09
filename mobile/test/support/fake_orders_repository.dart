import 'package:vitrio/core/network/api_result.dart';
import 'package:vitrio/core/network/problem_details.dart';
import 'package:vitrio/features/orders/data/orders_repository.dart';
import 'package:vitrio/features/orders/domain/cancellation_reason.dart';
import 'package:vitrio/features/orders/domain/customer_order.dart';
import 'package:vitrio/features/orders/domain/customer_order_summary.dart';
import 'package:vitrio/features/orders/domain/customer_orders_page.dart';
import 'package:vitrio/features/orders/domain/new_order.dart';
import 'package:vitrio/features/orders/domain/order_photo_copy.dart';
import 'package:vitrio/features/provider/domain/photo_upload.dart';
import 'package:vitrio/features/search/domain/provider_listing.dart';

final class FakeOrdersRepository implements OrdersRepository {
  final Map<String, CustomerOrder> orders = {};
  final List<CustomerOrderSummary> open = [];
  final List<CustomerOrderSummary> closed = [];
  final Map<String, List<ProviderListing>> suggested = {};
  final List<NewOrder> created = [];
  final List<String> idempotencyKeys = [];
  final List<String> calls = [];
  CustomerOrder? createdResult;
  ApiFailure<CustomerOrder>? failure;

  @override
  Future<ApiResult<PhotoUpload>> photoUpload({required String contentType, required int contentLength}) async =>
      const ApiSuccess(PhotoUpload(uploadUrl: 'https://storage.test/upload', objectKey: 'orders/upload-1'));

  @override
  Future<ApiResult<CustomerOrder>> create(NewOrder order, {required String idempotencyKey}) async {
    created.add(order);
    idempotencyKeys.add(idempotencyKey);
    if (failure case final ApiFailure<CustomerOrder> problem) {
      return problem;
    }

    final result = createdResult!;
    orders[result.id] = result;
    return ApiSuccess(result);
  }

  @override
  Future<ApiResult<List<OrderPhotoCopy>>> copyPhotos(String orderId) async => const ApiSuccess([]);

  @override
  Future<ApiResult<CustomerOrdersPage>> list({required bool open, required int page}) async =>
      ApiSuccess(CustomerOrdersPage(items: open ? this.open : closed, page: page, hasMore: false));

  @override
  Future<ApiResult<CustomerOrder>> get(String orderId) async =>
      orders[orderId] == null ? const ApiFailure(ProblemDetails(status: 404, code: 'order.not_found')) : ApiSuccess(orders[orderId]!);

  @override
  Future<ApiResult<List<ProviderListing>>> suggestions(String orderId) async => ApiSuccess(suggested[orderId] ?? const []);

  @override
  Future<ApiResult<CustomerOrder>> acceptQuote(String orderId, {required int quoteVersion}) async => _act('accept:$quoteVersion', orderId);

  @override
  Future<ApiResult<CustomerOrder>> declineQuote(String orderId) async => _act('declineQuote', orderId);

  @override
  Future<ApiResult<CustomerOrder>> cancel(String orderId, {required CancellationReason reason, String? details}) async =>
      _act('cancel:${reason.name}:${details ?? ''}', orderId);

  @override
  Future<ApiResult<CustomerOrder>> complete(String orderId) async => _act('complete', orderId);

  ApiResult<CustomerOrder> _act(String call, String orderId) {
    calls.add(call);
    return failure ?? ApiSuccess(orders[orderId]!);
  }
}
