import 'dart:ui' as ui;

import 'package:flutter/painting.dart';
import 'package:vitrio/features/provider/data/photo_picker.dart';
import 'package:vitrio/features/provider/domain/picked_photo.dart';

final class GeneratedPhotoPicker implements PhotoPicker {
  @override
  Future<PickedPhoto?> pick() async {
    final recorder = ui.PictureRecorder();
    Canvas(recorder)
      ..drawRect(const Rect.fromLTWH(0, 0, 800, 600), Paint()..color = const Color(0xFF3D1F56))
      ..drawCircle(const Offset(400, 300), 200, Paint()..color = const Color(0xFFC6F05A));
    final image = await recorder.endRecording().toImage(800, 600);
    final png = await image.toByteData(format: ui.ImageByteFormat.png);
    return PickedPhoto(bytes: png!.buffer.asUint8List(), contentType: 'image/png');
  }
}
