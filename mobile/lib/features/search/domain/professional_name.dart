final class ProfessionalName {
  const ProfessionalName({required this.singular, required this.plural});

  factory ProfessionalName.fromJson(Map<String, Object?> json) => ProfessionalName(
        singular: json['singular'] as String,
        plural: json['plural'] as String,
      );

  final String singular;
  final String plural;
}
