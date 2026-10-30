import '../domain/photo_upload.dart';
import '../domain/picked_photo.dart';

abstract interface class PhotoStorage {
  Future<bool> upload(PhotoUpload upload, PickedPhoto photo);
}
