final class PublicPhoto {
  const PublicPhoto({required this.id, required this.url, required this.thumbnailUrl, this.caption});

  factory PublicPhoto.fromJson(Map<String, Object?> json) => PublicPhoto(
        id: json['id'] as String,
        url: json['url'] as String,
        thumbnailUrl: json['thumbnailUrl'] as String,
        caption: json['caption'] as String?,
      );

  final String id;
  final String url;
  final String thumbnailUrl;
  final String? caption;
}
