// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'location_providers.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(citiesRepository)
final citiesRepositoryProvider = CitiesRepositoryProvider._();

final class CitiesRepositoryProvider
    extends
        $FunctionalProvider<
          CitiesRepository,
          CitiesRepository,
          CitiesRepository
        >
    with $Provider<CitiesRepository> {
  CitiesRepositoryProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'citiesRepositoryProvider',
        isAutoDispose: false,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$citiesRepositoryHash();

  @$internal
  @override
  $ProviderElement<CitiesRepository> $createElement($ProviderPointer pointer) =>
      $ProviderElement(pointer);

  @override
  CitiesRepository create(Ref ref) {
    return citiesRepository(ref);
  }

  /// {@macro riverpod.override_with_value}
  Override overrideWithValue(CitiesRepository value) {
    return $ProviderOverride(
      origin: this,
      providerOverride: $SyncValueProvider<CitiesRepository>(value),
    );
  }
}

String _$citiesRepositoryHash() => r'40888e1769e7660925cd038c6e7c541933c9a2b1';

@ProviderFor(deviceLocation)
final deviceLocationProvider = DeviceLocationProvider._();

final class DeviceLocationProvider
    extends $FunctionalProvider<DeviceLocation, DeviceLocation, DeviceLocation>
    with $Provider<DeviceLocation> {
  DeviceLocationProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'deviceLocationProvider',
        isAutoDispose: false,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$deviceLocationHash();

  @$internal
  @override
  $ProviderElement<DeviceLocation> $createElement($ProviderPointer pointer) =>
      $ProviderElement(pointer);

  @override
  DeviceLocation create(Ref ref) {
    return deviceLocation(ref);
  }

  /// {@macro riverpod.override_with_value}
  Override overrideWithValue(DeviceLocation value) {
    return $ProviderOverride(
      origin: this,
      providerOverride: $SyncValueProvider<DeviceLocation>(value),
    );
  }
}

String _$deviceLocationHash() => r'5b80fdf2b60529b5f5f43c6fcb46eaa8ab71efa3';

@ProviderFor(locationPreferences)
final locationPreferencesProvider = LocationPreferencesProvider._();

final class LocationPreferencesProvider
    extends
        $FunctionalProvider<
          LocationPreferences,
          LocationPreferences,
          LocationPreferences
        >
    with $Provider<LocationPreferences> {
  LocationPreferencesProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'locationPreferencesProvider',
        isAutoDispose: false,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$locationPreferencesHash();

  @$internal
  @override
  $ProviderElement<LocationPreferences> $createElement(
    $ProviderPointer pointer,
  ) => $ProviderElement(pointer);

  @override
  LocationPreferences create(Ref ref) {
    return locationPreferences(ref);
  }

  /// {@macro riverpod.override_with_value}
  Override overrideWithValue(LocationPreferences value) {
    return $ProviderOverride(
      origin: this,
      providerOverride: $SyncValueProvider<LocationPreferences>(value),
    );
  }
}

String _$locationPreferencesHash() =>
    r'5a5c2153d14c2b660948d540219193f842831669';
