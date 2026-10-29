import '../domain/device_permission.dart';
import '../domain/device_position.dart';

abstract interface class DeviceLocation {
  Future<DevicePermission> permission();

  Future<DevicePermission> requestPermission();

  Future<DevicePosition?> currentPosition();

  Future<String?> neighborhoodAt(DevicePosition position);

  Future<void> openSettings();
}
