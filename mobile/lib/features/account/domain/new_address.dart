final class NewAddress {
  const NewAddress({
    required this.label,
    required this.cep,
    required this.number,
    this.complement,
    this.street,
    this.neighborhood,
    this.state,
    this.cityIbgeCode,
  });

  final String label;
  final String cep;
  final String number;
  final String? complement;
  final String? street;
  final String? neighborhood;
  final String? state;
  final int? cityIbgeCode;

  Map<String, Object?> toJson() => {
        'label': label,
        'cep': cep,
        'number': number,
        'complement': complement,
        'street': street,
        'neighborhood': neighborhood,
        'state': state,
        'cityIbgeCode': cityIbgeCode,
      };
}
