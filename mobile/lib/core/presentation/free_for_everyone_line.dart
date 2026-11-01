import 'package:flutter/widgets.dart';

import '../../l10n/app_localizations.dart';
import '../design_system/tokens/vitrio_colors.dart';
import '../design_system/tokens/vitrio_space.dart';
import '../design_system/tokens/vitrio_text.dart';
import '../design_system/widgets/vitrio_icon.dart';
import '../design_system/widgets/vitrio_icon_name.dart';

class FreeForEveryoneLine extends StatelessWidget {
  const FreeForEveryoneLine({super.key, this.compact = false});

  final bool compact;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    return Row(
      children: [
        VitrioIcon(VitrioIconName.check, size: compact ? 18 : 20, color: c.lime),
        const SizedBox(width: VitrioSpace.sp8),
        Flexible(
          child: Text(
            AppLocalizations.of(context).brandFreeForEveryone,
            style: (compact ? VitrioText.label : VitrioText.bodyStrong).copyWith(color: c.onHero),
          ),
        ),
      ],
    );
  }
}
