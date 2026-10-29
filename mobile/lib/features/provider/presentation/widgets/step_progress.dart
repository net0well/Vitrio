import 'package:flutter/material.dart';

import '../../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../../core/design_system/tokens/vitrio_space.dart';
import '../../../../core/design_system/tokens/vitrio_text.dart';
import '../../../../l10n/app_localizations.dart';

class StepProgress extends StatelessWidget {
  const StepProgress({super.key, required this.step, required this.total});

  final int step;
  final int total;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    return Padding(
      padding: const EdgeInsets.only(bottom: VitrioSpace.sp20),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(AppLocalizations.of(context).providerStepOf(step, total), style: VitrioText.meta.copyWith(color: c.inkMuted)),
          const SizedBox(height: VitrioSpace.sp8),
          LinearProgressIndicator(value: step / total, minHeight: 4, borderRadius: BorderRadius.circular(2)),
        ],
      ),
    );
  }
}
