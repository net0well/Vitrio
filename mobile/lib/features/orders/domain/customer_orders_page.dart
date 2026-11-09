import '../../../core/network/json_body.dart';
import 'customer_order_summary.dart';

final class CustomerOrdersPage {
  const CustomerOrdersPage({required this.items, required this.page, required this.hasMore});

  factory CustomerOrdersPage.fromJson(Map<String, Object?> json, {required int page}) => CustomerOrdersPage(
        items: jsonObjects(json['items']).map(CustomerOrderSummary.fromJson).toList(growable: false),
        page: page,
        hasMore: json['hasMore'] == true,
      );

  final List<CustomerOrderSummary> items;
  final int page;
  final bool hasMore;
}
