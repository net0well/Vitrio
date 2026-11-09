enum OrderPhotoStatus {
  processing,
  ready,
  failed,
  removed;

  static OrderPhotoStatus fromJson(Object? json) => OrderPhotoStatus.values.firstWhere((value) => value.name == json, orElse: () => OrderPhotoStatus.values.first);
}
