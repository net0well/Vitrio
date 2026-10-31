import 'package:flutter/material.dart';

import '../../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../../core/design_system/tokens/vitrio_radius.dart';
import '../../../../core/design_system/tokens/vitrio_space.dart';

class ListingSkeleton extends StatelessWidget {
  const ListingSkeleton({super.key});

  static const _cards = 3;
  static const _cardHeight = 176.0;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    return Column(
      children: [
        for (var index = 0; index < _cards; index++) ...[
          Container(
            height: _cardHeight,
            decoration: BoxDecoration(
              color: c.track,
              borderRadius: const BorderRadius.all(VitrioRadius.card),
            ),
          ),
          const SizedBox(height: VitrioSpace.sp12),
        ],
      ],
    );
  }
}
