final class ProviderCategory {
  const ProviderCategory({required this.id, required this.name, required this.isPrimary});

  factory ProviderCategory.fromJson(Map<String, Object?> json) =>
      ProviderCategory(id: json['id'] as int, name: json['name'] as String, isPrimary: json['isPrimary'] == true);

  final int id;
  final String name;
  final bool isPrimary;
}
