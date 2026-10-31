import '../../provider/domain/service_category.dart';
import 'search_sort.dart';

final class SearchFilters {
  const SearchFilters({
    this.query = '',
    this.category,
    this.maxDistanceKm,
    this.minRating,
    this.meiVerified = false,
    this.availableToday = false,
    this.maxPriceFrom,
    this.sort = SearchSort.relevance,
  });

  static const empty = SearchFilters();

  final String query;
  final ServiceCategory? category;
  final int? maxDistanceKm;
  final double? minRating;
  final bool meiVerified;
  final bool availableToday;
  final int? maxPriceFrom;
  final SearchSort sort;

  SearchFilters copyWith({
    String? query,
    ServiceCategory? Function()? category,
    int? Function()? maxDistanceKm,
    double? Function()? minRating,
    bool? meiVerified,
    bool? availableToday,
    int? Function()? maxPriceFrom,
    SearchSort? sort,
  }) =>
      SearchFilters(
        query: query ?? this.query,
        category: category == null ? this.category : category(),
        maxDistanceKm: maxDistanceKm == null ? this.maxDistanceKm : maxDistanceKm(),
        minRating: minRating == null ? this.minRating : minRating(),
        meiVerified: meiVerified ?? this.meiVerified,
        availableToday: availableToday ?? this.availableToday,
        maxPriceFrom: maxPriceFrom == null ? this.maxPriceFrom : maxPriceFrom(),
        sort: sort ?? this.sort,
      );

  SearchFilters withoutLocationOnly() => copyWith(
        maxDistanceKm: () => null,
        sort: sort == SearchSort.distance ? SearchSort.relevance : sort,
      );

  Map<String, String> toQuery() => {
        if (query.trim().isNotEmpty) 'q': query.trim(),
        if (category case final chosen?) 'categoryId': chosen.id.toString(),
        if (maxDistanceKm case final distance?) 'maxDistanceKm': distance.toString(),
        if (minRating case final rating?) 'minRating': rating.toString(),
        if (meiVerified) 'meiVerified': 'true',
        if (availableToday) 'availableToday': 'true',
        if (maxPriceFrom case final price?) 'maxPriceFrom': price.toString(),
        if (sort != SearchSort.relevance) 'sort': sort.name,
      };

  @override
  bool operator ==(Object other) =>
      other is SearchFilters &&
      other.query == query &&
      other.category?.id == category?.id &&
      other.maxDistanceKm == maxDistanceKm &&
      other.minRating == minRating &&
      other.meiVerified == meiVerified &&
      other.availableToday == availableToday &&
      other.maxPriceFrom == maxPriceFrom &&
      other.sort == sort;

  @override
  int get hashCode => Object.hash(
        query,
        category?.id,
        maxDistanceKm,
        minRating,
        meiVerified,
        availableToday,
        maxPriceFrom,
        sort,
      );
}
