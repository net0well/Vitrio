enum SearchMode {
  nearMe,
  city;

  static SearchMode fromJson(Object? json) => json == 'nearMe' ? SearchMode.nearMe : SearchMode.city;
}
