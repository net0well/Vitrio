final class DevicePosition {
  const DevicePosition({required this.latitude, required this.longitude, required this.accuracyMeters});

  static const preciseEnoughMeters = 1000.0;

  final double latitude;
  final double longitude;
  final double accuracyMeters;

  bool get isPrecise => accuracyMeters <= preciseEnoughMeters;
}
