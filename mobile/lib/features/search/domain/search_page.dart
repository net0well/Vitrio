import '../../../core/network/json_body.dart';
import '../../location/domain/city_summary.dart';
import 'professional_name.dart';
import 'provider_listing.dart';
import 'search_mode.dart';

final class SearchPage {
  const SearchPage({
    required this.items,
    required this.page,
    required this.hasMore,
    required this.mode,
    required this.count,
    this.moreThanCount = false,
    this.city,
    this.professional,
  });

  factory SearchPage.fromJson(Map<String, Object?> json) {
    final context = json['context'] as Map<String, Object?>? ?? const {};
    final city = context['city'] as Map<String, Object?>?;
    final professional = context['professional'] as Map<String, Object?>?;
    return SearchPage(
      items: jsonObjects(json['items']).map(ProviderListing.fromJson).toList(growable: false),
      page: json['page'] as int,
      hasMore: json['hasMore'] == true,
      mode: SearchMode.fromJson(context['mode']),
      count: json['count'] as int,
      moreThanCount: json['moreThanCount'] == true,
      city: city == null ? null : CitySummary.fromJson(city),
      professional: professional == null ? null : ProfessionalName.fromJson(professional),
    );
  }

  final List<ProviderListing> items;
  final int page;
  final bool hasMore;
  final SearchMode mode;
  final int count;
  final bool moreThanCount;
  final CitySummary? city;
  final ProfessionalName? professional;
}
