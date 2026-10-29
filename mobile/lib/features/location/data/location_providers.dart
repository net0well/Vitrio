import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/network/network_providers.dart';
import '../../../core/storage/preferences.dart';
import 'cities_repository.dart';
import 'device_location.dart';
import 'http_cities_repository.dart';
import 'location_preferences.dart';
import 'system_device_location.dart';

part 'location_providers.g.dart';

@Riverpod(keepAlive: true)
CitiesRepository citiesRepository(Ref ref) => HttpCitiesRepository(ref.watch(apiClientProvider));

@Riverpod(keepAlive: true)
DeviceLocation deviceLocation(Ref ref) => SystemDeviceLocation();

@Riverpod(keepAlive: true)
LocationPreferences locationPreferences(Ref ref) => LocationPreferences(ref.watch(preferencesProvider));
