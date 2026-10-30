import 'package:flutter/material.dart';

import '../tokens/vitrio_colors.dart';
import '../tokens/vitrio_space.dart';
import '../tokens/vitrio_text.dart';
import 'vitrio_chip_style.dart';
import 'vitrio_icon.dart';
import 'vitrio_icon_name.dart';

class VitrioChip extends StatelessWidget {
  const VitrioChip({
    super.key,
    required this.label,
    required this.selected,
    this.onSelected,
    this.style = VitrioChipStyle.solid,
    this.trailing,
  });

  final String label;
  final bool selected;
  final ValueChanged<bool>? onSelected;
  final VitrioChipStyle style;
  final Widget? trailing;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    final (background, foreground, border) = switch ((style, selected)) {
      (VitrioChipStyle.solid, true) => (c.plum, c.onPlum, c.plum),
      (VitrioChipStyle.soft, true) => (c.plumSoft, c.plum, c.plum),
      _ => (c.surfaceRaised, c.ink, c.line),
    };
    return Semantics(
      selected: selected,
      button: onSelected != null,
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          customBorder: const StadiumBorder(),
          onTap: onSelected == null ? null : () => onSelected!(!selected),
          child: AnimatedContainer(
            duration: const Duration(milliseconds: 180),
            curve: Curves.easeOut,
            height: VitrioSpace.chip,
            padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp14),
            decoration: ShapeDecoration(
              color: background,
              shape: StadiumBorder(side: BorderSide(color: border, width: selected && style == VitrioChipStyle.soft ? 1.5 : 1)),
            ),
            child: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                if (selected && style == VitrioChipStyle.solid) ...[
                  VitrioIcon(VitrioIconName.check, size: 14, color: c.plumAccent, strokeWidth: 3),
                  const SizedBox(width: VitrioSpace.sp6),
                ],
                Text(label, style: VitrioText.weighted(VitrioText.meta, selected ? 800 : 700).copyWith(color: foreground)),
                if (trailing != null) ...[const SizedBox(width: VitrioSpace.sp6), trailing!],
              ],
            ),
          ),
        ),
      ),
    );
  }
}
