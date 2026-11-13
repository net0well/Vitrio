final class OrderProviderCard {
  const OrderProviderCard({
    required this.id,
    required this.displayName,
    required this.headline,
    required this.initials,
    required this.isNew,
    this.avatarUrl,
    this.ratingScore,
    this.reviewCount,
    this.whatsAppPhone,
  });

  factory OrderProviderCard.fromJson(Map<String, Object?> json) {
    final rating = json['rating'] as Map<String, Object?>?;
    return OrderProviderCard(
      id: json['id'] as String,
      displayName: json['displayName'] as String,
      headline: json['headline'] as String,
      initials: json['initials'] as String,
      avatarUrl: json['avatarUrl'] as String?,
      ratingScore: (rating?['score'] as num?)?.toDouble(),
      reviewCount: rating?['count'] as int?,
      isNew: json['isNew'] == true,
      whatsAppPhone: json['whatsAppPhone'] as String?,
    );
  }

  final String id;
  final String displayName;
  final String headline;
  final String initials;
  final String? avatarUrl;
  final double? ratingScore;
  final int? reviewCount;
  final bool isNew;
  final String? whatsAppPhone;

  String get firstName => displayName.split(' ').first;
}
