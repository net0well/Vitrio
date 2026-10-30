import 'city_summary.dart';

final class CepAddress {
  const CepAddress({required this.cep, required this.city, this.street, this.neighborhood});

  factory CepAddress.fromJson(Map<String, Object?> json) => CepAddress(
        cep: json['cep'] as String,
        street: json['street'] as String?,
        neighborhood: json['neighborhood'] as String?,
        city: CitySummary.fromJson(json['city'] as Map<String, Object?>),
      );

  final String cep;
  final String? street;
  final String? neighborhood;
  final CitySummary city;
}
