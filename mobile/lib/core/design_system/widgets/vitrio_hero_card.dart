import 'package:flutter/material.dart';

import '../tokens/vitrio_colors.dart';
import '../tokens/vitrio_radius.dart';
import '../tokens/vitrio_space.dart';
import '../tokens/vitrio_text.dart';
import 'vitrio_badge.dart';
import 'vitrio_badge_tone.dart';
import 'vitrio_icon.dart';
import 'vitrio_icon_name.dart';

class VitrioHeroCard extends StatelessWidget {
  const VitrioHeroCard({
    super.key,
    required this.title,
    required this.actionLabel,
    required this.onAction,
    this.text,
    this.tag,
    this.icon,
  });

  final String title;
  final String? text;
  final String? tag;
  final VitrioIconName? icon;
  final String actionLabel;
  final VoidCallback onAction;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    return ClipRRect(
      borderRadius: const BorderRadius.all(VitrioRadius.hero),
      child: ColoredBox(
        color: c.hero,
        child: Stack(
          children: [
            if (icon != null)
              PositionedDirectional(
                end: -18,
                top: 18,
                child: ExcludeSemantics(
                  child: Opacity(opacity: 0.3, child: VitrioIcon(icon!, size: 150, color: c.lime, strokeWidth: 1.2)),
                ),
              ),
            Padding(
              padding: const EdgeInsets.all(VitrioSpace.sp20),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  if (tag != null) ...[
                    VitrioBadge(label: tag!, tone: VitrioBadgeTone.accent, dot: true),
                    const SizedBox(height: VitrioSpace.sp10),
                  ],
                  Semantics(
                    header: true,
                    child: Text(title, style: VitrioText.title.copyWith(color: c.onHero)),
                  ),
                  if (text != null) ...[
                    const SizedBox(height: VitrioSpace.sp4),
                    ConstrainedBox(
                      constraints: const BoxConstraints(maxWidth: 240),
                      child: Text(text!, style: VitrioText.body.copyWith(color: c.onHeroMuted)),
                    ),
                  ],
                  const SizedBox(height: VitrioSpace.sp14),
                  Material(
                    color: c.surfaceRaised,
                    borderRadius: const BorderRadius.all(VitrioRadius.md),
                    child: InkWell(
                      borderRadius: const BorderRadius.all(VitrioRadius.md),
                      onTap: onAction,
                      child: SizedBox(
                        height: 40,
                        child: Padding(
                          padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp16),
                          child: Row(
                            mainAxisSize: MainAxisSize.min,
                            children: [
                              Text(actionLabel, style: VitrioText.label.copyWith(color: c.plum)),
                              const SizedBox(width: VitrioSpace.sp6),
                              VitrioIcon(VitrioIconName.arrowRight, size: 16, color: c.plum),
                            ],
                          ),
                        ),
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
