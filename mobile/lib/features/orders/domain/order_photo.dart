import 'order_photo_status.dart';

final class OrderPhoto {
  const OrderPhoto({required this.id, required this.status, this.url});

  factory OrderPhoto.fromJson(Map<String, Object?> json) => OrderPhoto(
        id: json['id'] as String,
        status: OrderPhotoStatus.fromJson(json['status']),
        url: json['url'] as String?,
      );

  final String id;
  final OrderPhotoStatus status;
  final String? url;
}
