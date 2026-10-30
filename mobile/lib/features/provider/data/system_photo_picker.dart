import 'package:image_picker/image_picker.dart';

import '../domain/picked_photo.dart';
import 'photo_picker.dart';

final class SystemPhotoPicker implements PhotoPicker {
  SystemPhotoPicker() : _picker = ImagePicker();

  static const _maxSide = 4000.0;

  final ImagePicker _picker;

  @override
  Future<PickedPhoto?> pick() async {
    final file = await _picker.pickImage(source: ImageSource.gallery, maxWidth: _maxSide, maxHeight: _maxSide, imageQuality: 90);
    if (file == null) {
      return null;
    }

    final name = file.name.toLowerCase();
    final contentType = name.endsWith('.png')
        ? 'image/png'
        : name.endsWith('.webp')
            ? 'image/webp'
            : 'image/jpeg';
    return PickedPhoto(bytes: await file.readAsBytes(), contentType: contentType);
  }
}
