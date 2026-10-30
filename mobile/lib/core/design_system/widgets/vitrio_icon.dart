import 'dart:convert';

import 'package:flutter/services.dart';
import 'package:flutter/widgets.dart';
import 'package:flutter_svg/flutter_svg.dart';

import '../tokens/vitrio_colors.dart';
import 'vitrio_icon_name.dart';

class VitrioIcon extends StatelessWidget {
  const VitrioIcon(this.icon, {super.key, this.size, this.color, this.semanticLabel, this.strokeWidth});

  final VitrioIconName icon;
  final double? size;
  final Color? color;
  final String? semanticLabel;
  final double? strokeWidth;

  @override
  Widget build(BuildContext context) {
    final iconTheme = IconTheme.of(context);
    final dimension = size ?? iconTheme.size ?? 20;
    final stroke = strokeWidth ?? (dimension <= 16 ? 2.4 : dimension <= 18 ? 2.2 : null);
    return SvgPicture(
      stroke == null ? SvgAssetLoader(icon.assetPath) : _StrokedAssetLoader(icon.assetPath, stroke),
      width: dimension,
      height: dimension,
      colorFilter: ColorFilter.mode(color ?? iconTheme.color ?? context.vitrio.ink, BlendMode.srcIn),
      semanticsLabel: semanticLabel,
      excludeFromSemantics: semanticLabel == null,
    );
  }
}

class _StrokedAssetLoader extends SvgLoader<ByteData> {
  const _StrokedAssetLoader(this.assetName, this.strokeWidth);

  final String assetName;
  final double strokeWidth;

  @override
  Future<ByteData?> prepareMessage(BuildContext? context) =>
      (context == null ? rootBundle : DefaultAssetBundle.of(context)).load(assetName);

  @override
  String provideSvg(ByteData? message) => utf8
      .decode(Uint8List.sublistView(message!), allowMalformed: true)
      .replaceAll(RegExp(r'stroke-width="[\d.]+"'), 'stroke-width="$strokeWidth"');

  @override
  SvgCacheKey cacheKey(BuildContext? context) =>
      SvgCacheKey(theme: getTheme(context), colorMapper: colorMapper, keyData: (assetName, strokeWidth));

  @override
  int get hashCode => Object.hash(assetName, strokeWidth);

  @override
  bool operator ==(Object other) => other is _StrokedAssetLoader && other.assetName == assetName && other.strokeWidth == strokeWidth;
}
