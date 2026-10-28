import 'package:flutter/material.dart';

import '../tokens/vitrio_colors.dart';
import '../tokens/vitrio_space.dart';
import 'vitrio_button_variant.dart';
import 'vitrio_icon.dart';
import 'vitrio_icon_name.dart';

class VitrioButton extends StatelessWidget {
  const VitrioButton({
    super.key,
    required this.label,
    required this.onPressed,
    this.variant = VitrioButtonVariant.accent,
    this.trailingIcon,
  });

  final String label;
  final VoidCallback? onPressed;
  final VitrioButtonVariant variant;
  final VitrioIconName? trailingIcon;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    final content = Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Flexible(child: Text(label, overflow: TextOverflow.ellipsis)),
        if (trailingIcon != null) ...[
          const SizedBox(width: VitrioSpace.sp8),
          VitrioIcon(trailingIcon!, size: 20),
        ],
      ],
    );

    return switch (variant) {
      VitrioButtonVariant.accent => FilledButton(
          onPressed: onPressed,
          style: FilledButton.styleFrom(
            backgroundColor: c.lime,
            foregroundColor: c.onLime,
            iconColor: c.onLime,
          ),
          child: content,
        ),
      VitrioButtonVariant.onHero => OutlinedButton(
          onPressed: onPressed,
          style: OutlinedButton.styleFrom(
            foregroundColor: c.onHero,
            iconColor: c.onHero,
            backgroundColor: Colors.transparent,
            side: BorderSide(color: c.onHeroMuted),
            minimumSize: const Size.fromHeight(VitrioSpace.cta),
          ),
          child: content,
        ),
    };
  }
}
