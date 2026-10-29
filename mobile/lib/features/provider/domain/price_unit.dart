enum PriceUnit {
  perService,
  perHour,
  perDay,
  perSquareMeter,
  perUnit;

  static PriceUnit fromJson(Object? json) => PriceUnit.values.firstWhere((value) => value.name == json, orElse: () => PriceUnit.values.first);
}
