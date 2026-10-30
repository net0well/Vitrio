import 'package:flutter/material.dart';

import '../../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../../core/design_system/tokens/vitrio_radius.dart';
import '../../../../core/design_system/tokens/vitrio_space.dart';
import '../../../../l10n/app_localizations.dart';

class StepProgress extends StatelessWidget {
  const StepProgress({super.key, required this.step, required this.total});

  final int step;
  final int total;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    return Semantics(
      label: AppLocalizations.of(context).providerStepOf(step, total),
      excludeSemantics: true,
      child: Row(
        children: [
          for (var index = 1; index <= total; index++) ...[
            if (index > 1) const SizedBox(width: VitrioSpace.sp6),
            Expanded(
              child: Container(
                height: 6,
                decoration: BoxDecoration(
                  color: index <= step ? c.plum : c.line,
                  borderRadius: const BorderRadius.all(VitrioRadius.full),
                ),
              ),
            ),
          ],
        ],
      ),
    );
  }
}
