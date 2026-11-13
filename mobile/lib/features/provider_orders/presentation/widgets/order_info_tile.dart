import 'package:flutter/material.dart';

import '../../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../../core/design_system/tokens/vitrio_radius.dart';
import '../../../../core/design_system/tokens/vitrio_space.dart';
import '../../../../core/design_system/tokens/vitrio_text.dart';

class OrderInfoTile extends StatelessWidget {
  const OrderInfoTile({super.key, required this.label, required this.value});

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    return MergeSemantics(
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp12, vertical: VitrioSpace.sp10),
        decoration: BoxDecoration(color: c.surfaceInset, borderRadius: const BorderRadius.all(VitrioRadius.md)),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(label, style: VitrioText.micro.copyWith(color: c.inkMuted)),
            const SizedBox(height: 2),
            Text(value, style: VitrioText.label.copyWith(color: c.ink)),
          ],
        ),
      ),
    );
  }
}
