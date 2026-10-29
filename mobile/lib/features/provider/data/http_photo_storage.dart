import 'package:http/http.dart' as http;

import '../domain/photo_upload.dart';
import '../domain/picked_photo.dart';
import 'photo_storage.dart';

final class HttpPhotoStorage implements PhotoStorage {
  const HttpPhotoStorage(this._http);

  final http.Client _http;

  @override
  Future<bool> upload(PhotoUpload upload, PickedPhoto photo) async {
    try {
      final response = await _http
          .put(Uri.parse(upload.uploadUrl), headers: {'Content-Type': photo.contentType}, body: photo.bytes)
          .timeout(const Duration(minutes: 1));
      return response.statusCode >= 200 && response.statusCode < 300;
    } on Exception {
      return false;
    }
  }
}
