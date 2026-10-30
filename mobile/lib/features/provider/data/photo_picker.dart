import '../domain/picked_photo.dart';

abstract interface class PhotoPicker {
  Future<PickedPhoto?> pick();
}
