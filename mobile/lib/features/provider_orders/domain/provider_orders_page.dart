import '../../../core/network/json_body.dart';
import 'provider_order_summary.dart';

final class ProviderOrdersPage {
  const ProviderOrdersPage({required this.items, required this.page, required this.hasMore});

  factory ProviderOrdersPage.fromJson(Map<String, Object?> json, {required int page}) => ProviderOrdersPage(
    items: jsonObjects(json['items']).map(ProviderOrderSummary.fromJson).toList(growable: false),
    page: page,
    hasMore: json['hasMore'] == true,
  );

  final List<ProviderOrderSummary> items;
  final int page;
  final bool hasMore;
}
