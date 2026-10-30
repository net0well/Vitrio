import 'package:flutter/material.dart';

import '../tokens/vitrio_colors.dart';
import '../tokens/vitrio_radius.dart';
import '../tokens/vitrio_space.dart';
import '../tokens/vitrio_text.dart';
import 'vitrio_switch.dart';

class VitrioSwitchRow extends StatelessWidget {
  const VitrioSwitchRow({
    super.key,
    required this.label,
    required this.value,
    required this.onChanged,
    this.description,
    this.onHero = false,
  });

  final String label;
  final String? description;
  final bool value;
  final ValueChanged<bool>? onChanged;
  final bool onHero;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    const radius = BorderRadius.all(VitrioRadius.xl);
    return MergeSemantics(
      child: Material(
        color: onHero ? c.heroGlass : c.surfaceRaised,
        shape: RoundedRectangleBorder(
          borderRadius: radius,
          side: onHero ? BorderSide.none : BorderSide(color: c.lineSoft),
        ),
        child: InkWell(
          borderRadius: radius,
          onTap: onChanged == null ? null : () => onChanged!(!value),
          child: Padding(
            padding: onHero
                ? const EdgeInsets.symmetric(horizontal: VitrioSpace.sp14, vertical: VitrioSpace.sp12)
                : const EdgeInsets.symmetric(horizontal: VitrioSpace.sp16, vertical: VitrioSpace.sp14),
            child: Row(
              children: [
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        label,
                        style: (onHero ? VitrioText.label : VitrioText.bodyStrong).copyWith(color: onHero ? c.onHero : c.ink),
                      ),
                      if (description != null) ...[
                        const SizedBox(height: 2),
                        Text(
                          description!,
                          style: VitrioText.weighted(VitrioText.caption, 600).copyWith(color: onHero ? c.onHeroMuted : c.inkMuted),
                        ),
                      ],
                    ],
                  ),
                ),
                const SizedBox(width: VitrioSpace.sp12),
                VitrioSwitch(value: value, onChanged: onChanged, onHero: onHero),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
