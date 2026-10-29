enum ProviderStatus {
  draft,
  published,
  paused,
  suspended,
  removed;

  static ProviderStatus fromJson(Object? json) => ProviderStatus.values.firstWhere((value) => value.name == json, orElse: () => ProviderStatus.values.first);
}
