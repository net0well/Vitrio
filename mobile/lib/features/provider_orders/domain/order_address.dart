final class OrderAddress {
  const OrderAddress({
    required this.street,
    required this.number,
    required this.neighborhood,
    required this.cityName,
    required this.state,
    required this.cep,
    this.complement,
    this.latitude,
    this.longitude,
  });

  factory OrderAddress.fromJson(Map<String, Object?> json) => OrderAddress(
    street: json['street'] as String,
    number: json['number'] as String,
    complement: json['complement'] as String?,
    neighborhood: json['neighborhood'] as String,
    cityName: json['cityName'] as String,
    state: json['state'] as String,
    cep: json['cep'] as String,
    latitude: (json['latitude'] as num?)?.toDouble(),
    longitude: (json['longitude'] as num?)?.toDouble(),
  );

  final String street;
  final String number;
  final String? complement;
  final String neighborhood;
  final String cityName;
  final String state;
  final String cep;
  final double? latitude;
  final double? longitude;
}
