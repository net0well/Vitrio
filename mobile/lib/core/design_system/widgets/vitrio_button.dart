import 'package:flutter/material.dart';

import '../tokens/vitrio_colors.dart';
import '../tokens/vitrio_radius.dart';
import '../tokens/vitrio_space.dart';
import '../tokens/vitrio_text.dart';
import 'vitrio_button_size.dart';
import 'vitrio_button_variant.dart';
import 'vitrio_icon.dart';
import 'vitrio_icon_name.dart';

class VitrioButton extends StatelessWidget {
  const VitrioButton({
    super.key,
    required this.label,
    required this.onPressed,
    this.variant = VitrioButtonVariant.primary,
    this.size = VitrioButtonSize.lg,
    this.leadingIcon,
    this.trailingIcon,
    this.busy = false,
  });

  final String label;
  final VoidCallback? onPressed;
  final VitrioButtonVariant variant;
  final VitrioButtonSize size;
  final VitrioIconName? leadingIcon;
  final VitrioIconName? trailingIcon;
  final bool busy;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    final (Color background, Color foreground, Color iconColor, BorderSide side) = switch (variant) {
      VitrioButtonVariant.primary => (c.plum, c.onPlum, c.plumAccent, BorderSide.none),
      VitrioButtonVariant.secondary => (c.surfaceRaised, c.ink, c.ink, BorderSide(color: c.line)),
      VitrioButtonVariant.ghost => (Colors.transparent, c.plum, c.plum, BorderSide.none),
      VitrioButtonVariant.danger => (c.dangerSoft, c.danger, c.danger, BorderSide.none),
      VitrioButtonVariant.accent => (c.lime, c.onLime, c.onLime, BorderSide.none),
      VitrioButtonVariant.onHero => (Colors.transparent, c.onHero, c.onHero, BorderSide(color: c.onHeroMuted, width: 1.5)),
    };
    final (double height, Radius radius, TextStyle text, double padding) = switch (size) {
      VitrioButtonSize.lg => (VitrioSpace.cta, VitrioRadius.xl, VitrioText.labelLg, VitrioSpace.sp20),
      VitrioButtonSize.md => (VitrioSpace.tap, VitrioRadius.md, VitrioText.label, VitrioSpace.sp16),
      VitrioButtonSize.sm => (36, VitrioRadius.sm, VitrioText.meta, VitrioSpace.sp12),
    };
    final iconSize = size == VitrioButtonSize.lg ? 18.0 : 16.0;
    final enabled = onPressed != null;
    final style = ButtonStyle(
      backgroundColor: WidgetStatePropertyAll(background),
      foregroundColor: WidgetStatePropertyAll(foreground),
      overlayColor: WidgetStatePropertyAll(foreground.withValues(alpha: 0.08)),
      side: WidgetStatePropertyAll(side),
      shape: WidgetStatePropertyAll(RoundedRectangleBorder(borderRadius: BorderRadius.all(radius))),
      minimumSize: WidgetStatePropertyAll(Size(VitrioSpace.tap, height)),
      padding: WidgetStatePropertyAll(
        EdgeInsets.symmetric(horizontal: variant == VitrioButtonVariant.ghost ? VitrioSpace.sp6 : padding),
      ),
      textStyle: WidgetStatePropertyAll(VitrioText.weighted(text, 800)),
      elevation: const WidgetStatePropertyAll(0),
      tapTargetSize: MaterialTapTargetSize.shrinkWrap,
    );
    final child = busy
        ? SizedBox.square(dimension: 20, child: CircularProgressIndicator(strokeWidth: 2.5, color: foreground))
        : Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              if (leadingIcon != null) ...[
                VitrioIcon(leadingIcon!, size: iconSize, color: foreground),
                const SizedBox(width: VitrioSpace.sp8),
              ],
              Flexible(child: Text(label, overflow: TextOverflow.ellipsis, maxLines: 1)),
              if (trailingIcon != null) ...[
                const SizedBox(width: VitrioSpace.sp8),
                VitrioIcon(trailingIcon!, size: iconSize, color: iconColor),
              ],
            ],
          );
    final button = switch (variant) {
      VitrioButtonVariant.secondary || VitrioButtonVariant.onHero => OutlinedButton(
          onPressed: busy ? () {} : onPressed,
          style: style,
          child: child,
        ),
      VitrioButtonVariant.ghost => TextButton(onPressed: busy ? () {} : onPressed, style: style, child: child),
      _ => FilledButton(onPressed: busy ? () {} : onPressed, style: style, child: child),
    };
    return Opacity(opacity: enabled || busy ? 1 : 0.4, child: button);
  }
}
