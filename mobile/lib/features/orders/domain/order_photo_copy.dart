final class OrderPhotoCopy {
  const OrderPhotoCopy({required this.objectKey, required this.previewUrl});

  factory OrderPhotoCopy.fromJson(Map<String, Object?> json) =>
      OrderPhotoCopy(objectKey: json['objectKey'] as String, previewUrl: json['previewUrl'] as String);

  final String objectKey;
  final String previewUrl;
}
