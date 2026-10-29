import 'availability_slot.dart';
import 'mei_info.dart';
import 'provider_category.dart';
import 'provider_service.dart';
import 'provider_status.dart';
import 'service_area.dart';

final class ProviderProfile {
  const ProviderProfile({
    required this.id,
    required this.status,
    required this.displayName,
    required this.headline,
    required this.whatsAppEnabled,
    required this.handlesEmergencies,
    required this.availableToday,
    required this.categories,
    required this.services,
    required this.availability,
    required this.missing,
    this.about,
    this.yearsOfExperience,
    this.avatarUrl,
    this.contactPhone,
    this.serviceArea,
    this.mei,
  });

  factory ProviderProfile.fromJson(Map<String, Object?> json) => ProviderProfile(
        id: json['id'] as String,
        status: ProviderStatus.fromJson(json['status']),
        displayName: json['displayName'] as String,
        headline: json['headline'] as String,
        about: json['about'] as String?,
        yearsOfExperience: json['yearsOfExperience'] as int?,
        avatarUrl: json['avatarUrl'] as String?,
        contactPhone: json['contactPhone'] as String?,
        whatsAppEnabled: json['whatsAppEnabled'] == true,
        handlesEmergencies: json['handlesEmergencies'] == true,
        availableToday: json['availableToday'] == true,
        categories: _list(json['categories'], ProviderCategory.fromJson),
        serviceArea: json['serviceArea'] is Map<String, Object?> ? ServiceArea.fromJson(json['serviceArea'] as Map<String, Object?>) : null,
        services: _list(json['services'], ProviderService.fromJson),
        availability: _list(json['availability'], AvailabilitySlot.fromJson),
        mei: json['mei'] is Map<String, Object?> ? MeiInfo.fromJson(json['mei'] as Map<String, Object?>) : null,
        missing: (json['missing'] as List<Object?>? ?? const []).whereType<String>().toList(growable: false),
      );

  final String id;
  final ProviderStatus status;
  final String displayName;
  final String headline;
  final String? about;
  final int? yearsOfExperience;
  final String? avatarUrl;
  final String? contactPhone;
  final bool whatsAppEnabled;
  final bool handlesEmergencies;
  final bool availableToday;
  final List<ProviderCategory> categories;
  final ServiceArea? serviceArea;
  final List<ProviderService> services;
  final List<AvailabilitySlot> availability;
  final MeiInfo? mei;
  final List<String> missing;

  static List<T> _list<T>(Object? json, T Function(Map<String, Object?>) decode) =>
      (json as List<Object?>? ?? const []).whereType<Map<String, Object?>>().map(decode).toList(growable: false);
}
