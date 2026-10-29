import 'package:flutter/material.dart';

import '../../../../core/design_system/tokens/vitrio_space.dart';

class SectionTitle extends StatelessWidget {
  const SectionTitle(this.text, {super.key, this.hint});

  final String text;
  final String? hint;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Padding(
      padding: const EdgeInsets.only(top: VitrioSpace.sp24, bottom: VitrioSpace.sp8),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(text, style: theme.textTheme.titleMedium),
          if (hint != null) ...[
            const SizedBox(height: VitrioSpace.sp4),
            Text(hint!, style: theme.textTheme.bodySmall),
          ],
        ],
      ),
    );
  }
}
