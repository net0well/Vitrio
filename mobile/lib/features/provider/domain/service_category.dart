final class ServiceCategory {
  const ServiceCategory({required this.id, required this.slug, required this.name, required this.icon, this.professionalPlural});

  factory ServiceCategory.fromJson(Map<String, Object?> json) => ServiceCategory(
        id: json['id'] as int,
        slug: json['slug'] as String,
        name: json['name'] as String,
        icon: json['icon'] as String,
        professionalPlural: json['professionalPlural'] as String?,
      );

  final int id;
  final String slug;
  final String name;
  final String icon;
  final String? professionalPlural;
}
