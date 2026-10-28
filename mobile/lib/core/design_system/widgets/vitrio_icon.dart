import 'package:flutter/widgets.dart';
import 'package:flutter_svg/flutter_svg.dart';

import '../tokens/vitrio_colors.dart';
import 'vitrio_icon_name.dart';

class VitrioIcon extends StatelessWidget {
  const VitrioIcon(this.icon, {super.key, this.size, this.color, this.semanticLabel});

  final VitrioIconName icon;
  final double? size;
  final Color? color;
  final String? semanticLabel;

  @override
  Widget build(BuildContext context) {
    final iconTheme = IconTheme.of(context);
    final dimension = size ?? iconTheme.size ?? 20;
    return SvgPicture.asset(
      icon.assetPath,
      width: dimension,
      height: dimension,
      colorFilter: ColorFilter.mode(
        color ?? iconTheme.color ?? context.vitrio.ink,
        BlendMode.srcIn,
      ),
      semanticsLabel: semanticLabel,
      excludeFromSemantics: semanticLabel == null,
    );
  }
}
