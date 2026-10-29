final class PhotoUpload {
  const PhotoUpload({required this.uploadUrl, required this.objectKey});

  factory PhotoUpload.fromJson(Map<String, Object?> json) =>
      PhotoUpload(uploadUrl: json['uploadUrl'] as String, objectKey: json['objectKey'] as String);

  final String uploadUrl;
  final String objectKey;
}
