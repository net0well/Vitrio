import 'dart:typed_data';

final class RequestPhoto {
  const RequestPhoto({required this.objectKey, this.bytes, this.url});

  final String objectKey;
  final Uint8List? bytes;
  final String? url;
}
