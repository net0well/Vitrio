import 'package:flutter/material.dart';

import '../tokens/vitrio_colors.dart';
import '../tokens/vitrio_radius.dart';
import '../tokens/vitrio_space.dart';
import '../tokens/vitrio_text.dart';
import 'vitrio_icon.dart';
import 'vitrio_icon_name.dart';

class VitrioCategoryTile extends StatelessWidget {
  const VitrioCategoryTile({super.key, required this.icon, required this.label, required this.onTap, this.more = false});

  final VitrioIconName icon;
  final String label;
  final VoidCallback onTap;
  final bool more;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    const radius = BorderRadius.all(VitrioRadius.xxl);
    return Material(
      color: c.surfaceRaised,
      shape: RoundedRectangleBorder(borderRadius: radius, side: BorderSide(color: c.lineSoft)),
      child: InkWell(
        borderRadius: radius,
        onTap: onTap,
        child: Padding(
          padding: const EdgeInsets.fromLTRB(VitrioSpace.sp4, VitrioSpace.sp14, VitrioSpace.sp4, VitrioSpace.sp12),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Container(
                width: 40,
                height: 40,
                decoration: BoxDecoration(
                  color: more ? c.ink : c.plumSoft,
                  borderRadius: const BorderRadius.all(VitrioRadius.md),
                ),
                alignment: Alignment.center,
                child: VitrioIcon(icon, size: 20, color: more ? c.surfaceRaised : c.plum),
              ),
              const SizedBox(height: VitrioSpace.sp8),
              Text(
                label,
                textAlign: TextAlign.center,
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
                style: VitrioText.caption.copyWith(color: c.ink),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
