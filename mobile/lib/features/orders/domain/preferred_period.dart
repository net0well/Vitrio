enum PreferredPeriod {
  morning,
  afternoon,
  evening;

  static PreferredPeriod fromJson(Object? json) => PreferredPeriod.values.firstWhere((value) => value.name == json, orElse: () => PreferredPeriod.values.first);
}
