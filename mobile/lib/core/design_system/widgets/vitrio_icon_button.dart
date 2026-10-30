import 'package:flutter/material.dart';

import '../tokens/vitrio_colors.dart';
import '../tokens/vitrio_radius.dart';
import '../tokens/vitrio_space.dart';
import 'vitrio_icon.dart';
import 'vitrio_icon_button_variant.dart';
import 'vitrio_icon_name.dart';

class VitrioIconButton extends StatelessWidget {
  const VitrioIconButton({
    super.key,
    required this.icon,
    required this.label,
    required this.onPressed,
    this.variant = VitrioIconButtonVariant.surface,
    this.dot = false,
    this.size = VitrioSpace.tap,
  });

  final VitrioIconName icon;
  final String label;
  final VoidCallback? onPressed;
  final VitrioIconButtonVariant variant;
  final bool dot;
  final double size;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    const rounded = RoundedRectangleBorder(borderRadius: BorderRadius.all(VitrioRadius.lg));
    final (Color background, Color foreground, BorderSide side, OutlinedBorder shape) = switch (variant) {
      VitrioIconButtonVariant.surface => (c.surfaceRaised, c.ink, BorderSide(color: c.line), const CircleBorder()),
      VitrioIconButtonVariant.soft => (c.plumSoft, c.plum, BorderSide.none, rounded),
      VitrioIconButtonVariant.solid => (c.plum, c.onPlum, BorderSide.none, rounded),
      VitrioIconButtonVariant.glass => (c.heroGlass, c.onHero, BorderSide.none, const CircleBorder()),
    };
    final glass = variant == VitrioIconButtonVariant.glass;
    return Stack(
      clipBehavior: Clip.none,
      children: [
        IconButton(
          tooltip: label,
          onPressed: onPressed,
          icon: VitrioIcon(icon, size: 20, color: foreground),
          style: IconButton.styleFrom(
            fixedSize: Size.square(size),
            minimumSize: Size.square(size),
            padding: EdgeInsets.zero,
            backgroundColor: background,
            foregroundColor: foreground,
            side: side,
            shape: shape,
          ),
        ),
        if (dot)
          PositionedDirectional(
            top: 9,
            end: 10,
            child: IgnorePointer(
              child: Container(
                width: 10,
                height: 10,
                decoration: BoxDecoration(
                  color: glass ? c.lime : c.alert,
                  shape: BoxShape.circle,
                  border: Border.all(color: glass ? c.hero : c.surfaceRaised, width: 2),
                ),
              ),
            ),
          ),
      ],
    );
  }
}
