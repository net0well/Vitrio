final class NewAddress {
  const NewAddress({required this.label, required this.cep, required this.number, this.complement});

  final String label;
  final String cep;
  final String number;
  final String? complement;

  Map<String, Object?> toJson() => {'label': label, 'cep': cep, 'number': number, 'complement': complement};
}
