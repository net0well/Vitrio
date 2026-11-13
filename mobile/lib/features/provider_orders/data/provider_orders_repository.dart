import '../../../core/network/api_result.dart';
import '../../orders/domain/cancellation_reason.dart';
import '../../orders/domain/decline_reason.dart';
import '../domain/agenda_item.dart';
import '../domain/provider_order.dart';
import '../domain/provider_order_tab.dart';
import '../domain/provider_orders_page.dart';
import '../domain/quote_draft.dart';

abstract interface class ProviderOrdersRepository {
  Future<ApiResult<ProviderOrdersPage>> list({required ProviderOrderTab tab, required int page});

  Future<ApiResult<ProviderOrder>> get(String orderId);

  Future<ApiResult<ProviderOrder>> sendQuote(String orderId, QuoteDraft quote, {required String idempotencyKey});

  Future<ApiResult<ProviderOrder>> decline(String orderId, {required DeclineReason reason, String? details});

  Future<ApiResult<ProviderOrder>> reschedule(
    String orderId, {
    required DateTime date,
    required String start,
    required int durationMinutes,
  });

  Future<ApiResult<ProviderOrder>> cancel(String orderId, {required CancellationReason reason, String? details});

  Future<ApiResult<ProviderOrder>> complete(String orderId);

  Future<ApiResult<List<AgendaItem>>> agenda({required DateTime from, required DateTime to});
}
