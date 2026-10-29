import 'dart:convert';

import 'package:shared_preferences/shared_preferences.dart';

import '../domain/city_summary.dart';
import '../domain/location_mode.dart';

final class LocationPreferences {
  const LocationPreferences(this._preferences);

  static const maxRecent = 5;
  static const _modeKey = 'location_mode';
  static const _chosenKey = 'location_chosen_city';
  static const _recentKey = 'location_recent_cities';

  final SharedPreferences _preferences;

  LocationMode? get mode => switch (_preferences.getString(_modeKey)) {
        'nearMe' => LocationMode.nearMe,
        'chosenCity' => LocationMode.chosenCity,
        _ => null,
      };

  CitySummary? get chosenCity => _decode(_preferences.getString(_chosenKey));

  List<CitySummary> get recent =>
      (_preferences.getStringList(_recentKey) ?? const []).map(_decode).whereType<CitySummary>().toList(growable: false);

  Future<void> useNearMe() => _preferences.setString(_modeKey, LocationMode.nearMe.name);

  Future<void> choose(CitySummary city) async {
    await _preferences.setString(_modeKey, LocationMode.chosenCity.name);
    await _preferences.setString(_chosenKey, jsonEncode(city.toJson()));
    final recent = [city, ...this.recent.where((other) => other != city)].take(maxRecent);
    await _preferences.setStringList(_recentKey, recent.map((other) => jsonEncode(other.toJson())).toList());
  }

  static CitySummary? _decode(String? json) {
    if (json == null) {
      return null;
    }

    try {
      final decoded = jsonDecode(json);
      return decoded is Map<String, Object?> ? CitySummary.fromJson(decoded) : null;
    } on FormatException {
      return null;
    }
  }
}
