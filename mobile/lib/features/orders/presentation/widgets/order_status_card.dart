import 'package:flutter/material.dart';

import '../../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../../core/design_system/tokens/vitrio_radius.dart';
import '../../../../core/design_system/tokens/vitrio_space.dart';
import '../../../../core/design_system/tokens/vitrio_text.dart';
import '../../../../core/design_system/widgets/vitrio_icon.dart';
import '../../../../core/design_system/widgets/vitrio_icon_name.dart';

class OrderStatusCard extends StatelessWidget {
  const OrderStatusCard({super.key, required this.title, required this.positive, this.body, this.footnote});

  static const _mark = 32.0;

  final String title;
  final String? body;
  final String? footnote;
  final bool positive;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    return Container(
      padding: const EdgeInsets.all(VitrioSpace.sp16),
      decoration: BoxDecoration(
        color: c.surfaceRaised,
        borderRadius: const BorderRadius.all(VitrioRadius.card),
        border: Border.all(color: c.lineSoft),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            children: [
              Container(
                width: _mark,
                height: _mark,
                alignment: Alignment.center,
                decoration: BoxDecoration(color: positive ? c.greenSoft : c.dangerSoft, shape: BoxShape.circle),
                child: VitrioIcon(
                  positive ? VitrioIconName.check : VitrioIconName.x,
                  size: 16,
                  color: positive ? c.green : c.danger,
                  strokeWidth: 3,
                ),
              ),
              const SizedBox(width: VitrioSpace.sp10),
              Expanded(
                child: Text(title, style: VitrioText.bodyStrong.copyWith(color: c.ink)),
              ),
            ],
          ),
          if (body != null) ...[
            const SizedBox(height: VitrioSpace.sp12),
            Text(body!, style: VitrioText.body.copyWith(height: 1.5, color: c.ink2)),
          ],
          if (footnote != null) ...[
            const SizedBox(height: VitrioSpace.sp6),
            Text(footnote!, style: VitrioText.meta.copyWith(height: 1.5, color: c.inkMuted)),
          ],
        ],
      ),
    );
  }
}
