enum MeiStatus {
  pending,
  verified,
  failed,
  lost;

  static MeiStatus fromJson(Object? json) => MeiStatus.values.firstWhere((value) => value.name == json, orElse: () => MeiStatus.values.first);
}
