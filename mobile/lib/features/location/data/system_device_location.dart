import 'dart:async';

import 'package:flutter/widgets.dart';
import 'package:geocoding/geocoding.dart';
import 'package:geolocator/geolocator.dart';

import '../domain/device_permission.dart';
import '../domain/device_position.dart';
import 'device_location.dart';

final class SystemDeviceLocation implements DeviceLocation {
  SystemDeviceLocation() : _geocoding = Geocoding(locale: const Locale('pt', 'BR'));

  static const _positionTimeout = Duration(seconds: 10);

  final Geocoding _geocoding;

  @override
  Future<DevicePermission> permission() async {
    if (!await Geolocator.isLocationServiceEnabled()) {
      return DevicePermission.serviceDisabled;
    }

    return _map(await Geolocator.checkPermission());
  }

  @override
  Future<DevicePermission> requestPermission() async => _map(await Geolocator.requestPermission());

  @override
  Future<DevicePosition?> currentPosition() async {
    try {
      final position = await Geolocator.getCurrentPosition(
        locationSettings: const LocationSettings(accuracy: LocationAccuracy.medium, timeLimit: _positionTimeout),
      );
      return DevicePosition(latitude: position.latitude, longitude: position.longitude, accuracyMeters: position.accuracy);
    } on TimeoutException {
      return null;
    } on LocationServiceDisabledException {
      return null;
    } on PermissionDeniedException {
      return null;
    }
  }

  @override
  Future<String?> neighborhoodAt(DevicePosition position) async {
    try {
      final places = await _geocoding.placemarkFromCoordinates(position.latitude, position.longitude);
      final neighborhood = places.isEmpty ? null : places.first.subLocality?.trim();
      return neighborhood == null || neighborhood.isEmpty ? null : neighborhood;
    } on Exception {
      return null;
    }
  }

  @override
  Future<void> openSettings() async => Geolocator.openAppSettings();

  static DevicePermission _map(LocationPermission permission) => switch (permission) {
        LocationPermission.always || LocationPermission.whileInUse => DevicePermission.granted,
        LocationPermission.deniedForever => DevicePermission.deniedForever,
        _ => DevicePermission.denied,
      };
}
