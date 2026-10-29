import 'dart:async';

import 'package:vitrio/features/location/data/device_location.dart';
import 'package:vitrio/features/location/domain/device_permission.dart';
import 'package:vitrio/features/location/domain/device_position.dart';

final class FakeDeviceLocation implements DeviceLocation {
  DevicePermission current = DevicePermission.denied;
  DevicePermission afterRequest = DevicePermission.denied;
  DevicePosition? position = const DevicePosition(latitude: -21.255, longitude: -48.322, accuracyMeters: 30);
  String? neighborhood = 'Centro';
  int settingsOpened = 0;
  Completer<void>? positionGate;

  @override
  Future<DevicePermission> permission() async => current;

  @override
  Future<DevicePermission> requestPermission() async => current = afterRequest;

  @override
  Future<DevicePosition?> currentPosition() async {
    await positionGate?.future;
    return position;
  }

  @override
  Future<String?> neighborhoodAt(DevicePosition position) async => neighborhood;

  @override
  Future<void> openSettings() async => settingsOpened++;
}
