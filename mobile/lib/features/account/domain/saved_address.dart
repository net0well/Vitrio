final class SavedAddress {
  const SavedAddress({
    required this.id,
    required this.label,
    required this.cep,
    required this.street,
    required this.number,
    required this.neighborhood,
    required this.city,
    required this.state,
    this.complement,
  });

  factory SavedAddress.fromJson(Map<String, Object?> json) => SavedAddress(
        id: json['id'] as String,
        label: json['label'] as String,
        cep: json['cep'] as String,
        street: json['street'] as String,
        number: json['number'] as String,
        complement: json['complement'] as String?,
        neighborhood: json['neighborhood'] as String,
        city: json['city'] as String,
        state: json['state'] as String,
      );

  final String id;
  final String label;
  final String cep;
  final String street;
  final String number;
  final String? complement;
  final String neighborhood;
  final String city;
  final String state;
}
