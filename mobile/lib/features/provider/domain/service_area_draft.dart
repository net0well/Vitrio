final class ServiceAreaDraft {
  const ServiceAreaDraft({
    required this.cep,
    required this.number,
    required this.radiusKm,
    required this.neighborhoods,
    this.complement,
  });

  final String cep;
  final String number;
  final String? complement;
  final int radiusKm;
  final List<String> neighborhoods;

  Map<String, Object?> toJson() => {
        'cep': cep,
        'number': number,
        'complement': ?complement,
        'radiusKm': radiusKm,
        'neighborhoods': neighborhoods,
      };
}
