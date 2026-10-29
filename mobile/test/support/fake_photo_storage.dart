import 'package:vitrio/features/provider/data/photo_storage.dart';
import 'package:vitrio/features/provider/domain/photo_upload.dart';
import 'package:vitrio/features/provider/domain/picked_photo.dart';

final class FakePhotoStorage implements PhotoStorage {
  final List<PhotoUpload> uploads = [];

  @override
  Future<bool> upload(PhotoUpload upload, PickedPhoto photo) async {
    uploads.add(upload);
    return true;
  }
}
