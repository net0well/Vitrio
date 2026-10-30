import '../../location/domain/city_summary.dart';

final class ServiceArea {
  const ServiceArea({
    required this.cep,
    required this.number,
    required this.city,
    required this.radiusKm,
    required this.neighborhoods,
    required this.locationApproximate,
    this.street,
    this.complement,
    this.neighborhood,
  });

  factory ServiceArea.fromJson(Map<String, Object?> json) => ServiceArea(
        cep: json['cep'] as String,
        street: json['street'] as String?,
        number: json['number'] as String,
        complement: json['complement'] as String?,
        neighborhood: json['neighborhood'] as String?,
        city: CitySummary.fromJson(json['city'] as Map<String, Object?>),
        radiusKm: json['radiusKm'] as int,
        neighborhoods: (json['neighborhoods'] as List<Object?>).whereType<String>().toList(growable: false),
        locationApproximate: json['locationApproximate'] == true,
      );

  final String cep;
  final String? street;
  final String number;
  final String? complement;
  final String? neighborhood;
  final CitySummary city;
  final int radiusKm;
  final List<String> neighborhoods;
  final bool locationApproximate;
}
