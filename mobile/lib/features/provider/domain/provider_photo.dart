import 'photo_status.dart';

final class ProviderPhoto {
  const ProviderPhoto({required this.id, required this.status, this.url, this.thumbnailUrl, this.caption, this.rejectionReason});

  factory ProviderPhoto.fromJson(Map<String, Object?> json) => ProviderPhoto(
        id: json['id'] as String,
        url: json['url'] as String?,
        thumbnailUrl: json['thumbnailUrl'] as String?,
        caption: json['caption'] as String?,
        status: PhotoStatus.fromJson(json['status']),
        rejectionReason: json['rejectionReason'] as String?,
      );

  final String id;
  final String? url;
  final String? thumbnailUrl;
  final String? caption;
  final PhotoStatus status;
  final String? rejectionReason;
}
