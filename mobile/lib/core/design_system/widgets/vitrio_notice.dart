import 'package:flutter/material.dart';

import '../tokens/vitrio_colors.dart';
import '../tokens/vitrio_radius.dart';
import '../tokens/vitrio_space.dart';
import '../tokens/vitrio_text.dart';
import 'vitrio_badge_tone.dart';
import 'vitrio_icon.dart';
import 'vitrio_icon_name.dart';

class VitrioNotice extends StatelessWidget {
  const VitrioNotice({super.key, required this.message, this.tone = VitrioBadgeTone.danger, this.icon});

  final String message;
  final VitrioBadgeTone tone;
  final VitrioIconName? icon;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    final (background, foreground, defaultIcon) = switch (tone) {
      VitrioBadgeTone.verified => (c.greenSoft, c.green, VitrioIconName.check),
      VitrioBadgeTone.warning => (c.amberSoft, c.amberInk, VitrioIconName.clock),
      VitrioBadgeTone.info || VitrioBadgeTone.neutral => (c.infoSoft, c.info, VitrioIconName.info),
      VitrioBadgeTone.brand || VitrioBadgeTone.accent => (c.plumSoft, c.plum, VitrioIconName.info),
      VitrioBadgeTone.danger => (c.dangerSoft, c.danger, VitrioIconName.info),
    };
    return Semantics(
      liveRegion: true,
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp12, vertical: VitrioSpace.sp10),
        decoration: BoxDecoration(color: background, borderRadius: const BorderRadius.all(VitrioRadius.md)),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Padding(
              padding: const EdgeInsets.only(top: 1),
              child: VitrioIcon(icon ?? defaultIcon, size: 16, color: foreground),
            ),
            const SizedBox(width: VitrioSpace.sp8),
            Expanded(
              child: Text(message, style: VitrioText.weighted(VitrioText.meta, 700).copyWith(color: foreground)),
            ),
          ],
        ),
      ),
    );
  }
}
