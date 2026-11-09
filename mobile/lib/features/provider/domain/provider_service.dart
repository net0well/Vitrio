import 'price_unit.dart';

final class ProviderService {
  const ProviderService({required this.title, this.id, this.description, this.priceFrom, this.unit = PriceUnit.perService, this.onQuote = false});

  factory ProviderService.fromJson(Map<String, Object?> json) => ProviderService(
        id: json['id'] as String?,
        title: json['title'] as String,
        description: json['description'] as String?,
        priceFrom: (json['priceFrom'] as num?)?.toDouble(),
        unit: PriceUnit.fromJson(json['priceUnit']),
        onQuote: json['onQuote'] == true,
      );

  final String? id;
  final String title;
  final String? description;
  final double? priceFrom;
  final PriceUnit unit;
  final bool onQuote;

  Map<String, Object?> toJson() => {
        'title': title,
        'description': ?description,
        'priceFrom': onQuote ? null : priceFrom,
        'priceUnit': unit.name,
        'onQuote': onQuote,
      };
}
