import 'city_summary.dart';
import 'location_mode.dart';

final class LocationState {
  const LocationState({required this.mode, required this.city, this.neighborhood});

  static const unset = LocationState(mode: null, city: null);

  final LocationMode? mode;
  final CitySummary? city;
  final String? neighborhood;

  bool get isSet => city != null;
}
