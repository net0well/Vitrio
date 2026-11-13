enum OrderParty {
  customer,
  provider,
  system;

  static OrderParty fromJson(Object? json) => OrderParty.values.firstWhere((value) => value.name == json, orElse: () => OrderParty.values.first);
}
