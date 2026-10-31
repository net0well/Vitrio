import '../../provider/domain/price_unit.dart';

final class ListingPrice {
  const ListingPrice({required this.from, required this.unit});

  factory ListingPrice.fromJson(Map<String, Object?> json) => ListingPrice(
        from: (json['from'] as num).toDouble(),
        unit: PriceUnit.fromJson(json['unit']),
      );

  final double from;
  final PriceUnit unit;
}
