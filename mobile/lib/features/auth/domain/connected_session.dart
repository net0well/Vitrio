final class ConnectedSession {
  const ConnectedSession({
    required this.id,
    required this.deviceName,
    required this.lastSeenAt,
    required this.current,
  });

  factory ConnectedSession.fromJson(Map<String, Object?> json) {
    final id = json['id'];
    final deviceName = json['deviceName'];
    final lastSeenAt = json['lastSeenAt'];
    final current = json['current'];
    if (id is! String || deviceName is! String || lastSeenAt is! String || current is! bool) {
      throw const FormatException('Session response without id, deviceName, lastSeenAt or current.');
    }

    return ConnectedSession(id: id, deviceName: deviceName, lastSeenAt: DateTime.parse(lastSeenAt), current: current);
  }

  final String id;
  final String deviceName;
  final DateTime lastSeenAt;
  final bool current;
}
