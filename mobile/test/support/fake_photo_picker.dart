import 'dart:typed_data';

import 'package:vitrio/features/provider/data/photo_picker.dart';
import 'package:vitrio/features/provider/domain/picked_photo.dart';

final class FakePhotoPicker implements PhotoPicker {
  PickedPhoto? next = PickedPhoto(bytes: Uint8List.fromList(const [1, 2, 3]), contentType: 'image/jpeg');

  @override
  Future<PickedPhoto?> pick() async => next;
}
