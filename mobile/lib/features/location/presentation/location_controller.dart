import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/network/api_result.dart';
import '../data/location_providers.dart';
import '../domain/city_summary.dart';
import '../domain/device_permission.dart';
import '../domain/location_mode.dart';
import '../domain/location_state.dart';
import 'location_outcome.dart';

part 'location_controller.g.dart';

@Riverpod(keepAlive: true)
class LocationController extends _$LocationController {
  @override
  Future<LocationState> build() async {
    final preferences = ref.read(locationPreferencesProvider);
    return switch (preferences.mode) {
      LocationMode.nearMe => await _nearMe() ?? _chosen(),
      LocationMode.chosenCity => _chosen(),
      null => LocationState.unset,
    };
  }

  Future<LocationOutcome> useNearMe() async {
    final device = ref.read(deviceLocationProvider);
    var permission = await device.permission();
    if (permission == DevicePermission.denied) {
      permission = await device.requestPermission();
    }

    if (permission != DevicePermission.granted) {
      if (permission == DevicePermission.deniedForever || permission == DevicePermission.serviceDisabled) {
        await device.openSettings();
        return LocationOutcome.openedSettings;
      }

      return LocationOutcome.permissionDenied;
    }

    final located = await _nearMe();
    if (located == null) {
      return LocationOutcome.notFound;
    }

    await ref.read(locationPreferencesProvider).useNearMe();
    state = AsyncData(located);
    return LocationOutcome.located;
  }

  Future<void> choose(CitySummary city) async {
    await ref.read(locationPreferencesProvider).choose(city);
    state = AsyncData(LocationState(mode: LocationMode.chosenCity, city: city));
  }

  Future<LocationState?> _nearMe() async {
    final device = ref.read(deviceLocationProvider);
    if (await device.permission() != DevicePermission.granted) {
      return null;
    }

    final position = await device.currentPosition();
    if (position == null) {
      return null;
    }

    final city = await ref.read(citiesRepositoryProvider).locate(latitude: position.latitude, longitude: position.longitude);
    if (city is! ApiSuccess<CitySummary>) {
      return null;
    }

    final neighborhood = position.isPrecise ? await device.neighborhoodAt(position) : null;
    return LocationState(mode: LocationMode.nearMe, city: city.data, neighborhood: neighborhood);
  }

  LocationState _chosen() {
    final city = ref.read(locationPreferencesProvider).chosenCity;
    return city == null ? LocationState.unset : LocationState(mode: LocationMode.chosenCity, city: city);
  }
}
