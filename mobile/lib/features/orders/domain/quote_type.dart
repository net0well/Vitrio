enum QuoteType {
  fixedPrice,
  technicalVisit;

  static QuoteType fromJson(Object? json) => QuoteType.values.firstWhere((value) => value.name == json, orElse: () => QuoteType.values.first);
}
