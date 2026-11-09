enum DeclineReason {
  noAvailability,
  outsideArea,
  notMyService,
  other;

  static DeclineReason fromJson(Object? json) => DeclineReason.values.firstWhere((value) => value.name == json, orElse: () => DeclineReason.values.first);
}
