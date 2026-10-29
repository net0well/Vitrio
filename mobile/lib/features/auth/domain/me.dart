final class Me {
  const Me({
    required this.id,
    required this.displayName,
    required this.initials,
    required this.email,
    required this.phoneNumber,
    required this.roles,
    this.hasProviderProfile = false,
  });

  factory Me.fromJson(Map<String, Object?> json) {
    final id = json['id'];
    final displayName = json['displayName'];
    final initials = json['initials'];
    final email = json['email'];
    final phoneNumber = json['phoneNumber'];
    final roles = json['roles'];
    if (id is! String || displayName is! String || initials is! String || email is! String || roles is! List<Object?>) {
      throw const FormatException('Account response without id, displayName, initials, email or roles.');
    }

    return Me(
      id: id,
      displayName: displayName,
      initials: initials,
      email: email,
      phoneNumber: phoneNumber is String ? phoneNumber : null,
      roles: roles.whereType<String>().toList(growable: false),
      hasProviderProfile: json['hasProviderProfile'] == true,
    );
  }

  final String id;
  final String displayName;
  final String initials;
  final String email;
  final String? phoneNumber;
  final List<String> roles;
  final bool hasProviderProfile;
}
