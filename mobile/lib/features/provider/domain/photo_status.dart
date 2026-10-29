enum PhotoStatus {
  processing,
  active,
  rejected,
  hidden;

  static PhotoStatus fromJson(Object? json) => PhotoStatus.values.firstWhere((value) => value.name == json, orElse: () => PhotoStatus.values.first);
}
