import '../../../core/network/json_body.dart';
import '../../location/domain/city_summary.dart';
import 'provider_listing.dart';
import 'search_mode.dart';

final class SearchPage {
  const SearchPage({
    required this.items,
    required this.page,
    required this.hasMore,
    required this.mode,
    this.city,
  });

  factory SearchPage.fromJson(Map<String, Object?> json) {
    final context = json['context'] as Map<String, Object?>? ?? const {};
    final city = context['city'] as Map<String, Object?>?;
    return SearchPage(
      items: jsonObjects(json['items']).map(ProviderListing.fromJson).toList(growable: false),
      page: json['page'] as int,
      hasMore: json['hasMore'] == true,
      mode: SearchMode.fromJson(context['mode']),
      city: city == null ? null : CitySummary.fromJson(city),
    );
  }

  final List<ProviderListing> items;
  final int page;
  final bool hasMore;
  final SearchMode mode;
  final CitySummary? city;
}
