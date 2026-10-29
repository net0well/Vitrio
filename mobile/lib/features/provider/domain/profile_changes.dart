final class ProfileChanges {
  const ProfileChanges({
    this.displayName,
    this.headline,
    this.about,
    this.yearsOfExperience,
    this.contactPhone,
    this.whatsAppEnabled,
    this.handlesEmergencies,
  });

  final String? displayName;
  final String? headline;
  final String? about;
  final int? yearsOfExperience;
  final String? contactPhone;
  final bool? whatsAppEnabled;
  final bool? handlesEmergencies;

  Map<String, Object?> toJson() => {
        'displayName': ?displayName,
        'headline': ?headline,
        'about': ?about,
        'yearsOfExperience': ?yearsOfExperience,
        'contactPhone': ?contactPhone,
        'whatsAppEnabled': ?whatsAppEnabled,
        'handlesEmergencies': ?handlesEmergencies,
      };
}
