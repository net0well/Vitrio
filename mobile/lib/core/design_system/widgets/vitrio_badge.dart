import 'package:flutter/material.dart';

import '../tokens/vitrio_colors.dart';
import '../tokens/vitrio_space.dart';
import '../tokens/vitrio_text.dart';

enum VitrioBadgeTone { verified, brand, neutral, warning, accent, danger, info }

class VitrioBadge extends StatelessWidget {
  const VitrioBadge({super.key, required this.label, this.tone = VitrioBadgeTone.brand, this.icon});

  final String label;
  final VitrioBadgeTone tone;
  final Widget? icon;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    final (background, foreground) = switch (tone) {
      VitrioBadgeTone.verified => (c.greenSoft, c.green),
      VitrioBadgeTone.brand => (c.plumSoft, c.plum),
      VitrioBadgeTone.neutral => (c.surfaceInset, c.ink2),
      VitrioBadgeTone.warning => (c.amberSoft, c.amberInk),
      VitrioBadgeTone.accent => (c.lime, c.onLime),
      VitrioBadgeTone.danger => (c.dangerSoft, c.danger),
      VitrioBadgeTone.info => (c.infoSoft, c.info),
    };
    return Container(
      height: VitrioSpace.badge,
      padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp10),
      decoration: ShapeDecoration(color: background, shape: const StadiumBorder()),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          if (icon != null) ...[
            IconTheme(data: IconThemeData(size: 14, color: foreground), child: icon!),
            const SizedBox(width: VitrioSpace.sp6),
          ],
          Text(label, style: VitrioText.caption.copyWith(color: foreground)),
        ],
      ),
    );
  }
}
