import '../../location/domain/city_summary.dart';
import 'availability_slot.dart';
import 'provider_category.dart';
import 'provider_service.dart';
import 'public_photo.dart';

final class PublicProvider {
  const PublicProvider({
    required this.id,
    required this.displayName,
    required this.headline,
    required this.city,
    required this.neighborhoods,
    required this.radiusKm,
    required this.categories,
    required this.services,
    required this.availability,
    required this.handlesEmergencies,
    required this.meiVerified,
    required this.availableToday,
    required this.isNew,
    required this.photos,
    required this.shareUrl,
    this.about,
    this.yearsOfExperience,
    this.avatarUrl,
    this.ratingScore,
    this.reviewCount,
    this.whatsAppPhone,
    this.distanceKm,
  });

  factory PublicProvider.fromJson(Map<String, Object?> json) {
    final badges = json['badges'] as Map<String, Object?>? ?? const {};
    final rating = json['rating'] as Map<String, Object?>?;
    return PublicProvider(
      id: json['id'] as String,
      displayName: json['displayName'] as String,
      headline: json['headline'] as String,
      about: json['about'] as String?,
      yearsOfExperience: json['yearsOfExperience'] as int?,
      avatarUrl: json['avatarUrl'] as String?,
      city: CitySummary.fromJson(json['city'] as Map<String, Object?>),
      neighborhoods: (json['neighborhoods'] as List<Object?>? ?? const []).whereType<String>().toList(growable: false),
      radiusKm: json['radiusKm'] as int,
      categories: _list(json['categories'], ProviderCategory.fromJson),
      services: _list(json['services'], ProviderService.fromJson),
      availability: _list(json['availability'], AvailabilitySlot.fromJson),
      handlesEmergencies: json['handlesEmergencies'] == true,
      meiVerified: badges['meiVerified'] == true,
      availableToday: badges['availableToday'] == true,
      ratingScore: (rating?['score'] as num?)?.toDouble(),
      reviewCount: rating?['count'] as int?,
      isNew: json['isNew'] != false,
      whatsAppPhone: json['whatsAppPhone'] as String?,
      distanceKm: (json['distanceKm'] as num?)?.toDouble(),
      photos: _list(json['firstPhotos'], PublicPhoto.fromJson),
      shareUrl: json['shareUrl'] as String,
    );
  }

  final String id;
  final String displayName;
  final String headline;
  final String? about;
  final int? yearsOfExperience;
  final String? avatarUrl;
  final CitySummary city;
  final List<String> neighborhoods;
  final int radiusKm;
  final List<ProviderCategory> categories;
  final List<ProviderService> services;
  final List<AvailabilitySlot> availability;
  final bool handlesEmergencies;
  final bool meiVerified;
  final bool availableToday;
  final double? ratingScore;
  final int? reviewCount;
  final bool isNew;
  final String? whatsAppPhone;
  final double? distanceKm;
  final List<PublicPhoto> photos;
  final String shareUrl;

  static List<T> _list<T>(Object? json, T Function(Map<String, Object?>) decode) =>
      (json as List<Object?>? ?? const []).whereType<Map<String, Object?>>().map(decode).toList(growable: false);
}
