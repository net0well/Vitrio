import 'preferred_period.dart';

final class NewOrder {
  const NewOrder({
    required this.providerId,
    required this.title,
    required this.description,
    required this.preferredDay,
    required this.preferredPeriod,
    required this.addressId,
    required this.photoKeys,
    this.serviceId,
  });

  final String providerId;
  final String? serviceId;
  final String title;
  final String description;
  final DateTime preferredDay;
  final PreferredPeriod preferredPeriod;
  final String addressId;
  final List<String> photoKeys;

  Map<String, Object?> toJson() => {
        'providerId': providerId,
        'serviceId': serviceId,
        'title': title,
        'description': description,
        'preferredDay': _date(preferredDay),
        'preferredPeriod': preferredPeriod.name,
        'addressId': addressId,
        'photoKeys': photoKeys,
      };

  static String _date(DateTime day) =>
      '${day.year.toString().padLeft(4, '0')}-${day.month.toString().padLeft(2, '0')}-${day.day.toString().padLeft(2, '0')}';
}
