import 'package:flutter/material.dart';

import '../../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../../core/design_system/tokens/vitrio_text.dart';

class OrderSectionLabel extends StatelessWidget {
  const OrderSectionLabel(this.text, {super.key});

  final String text;

  @override
  Widget build(BuildContext context) => Semantics(
    header: true,
    child: Text(
      text.toUpperCase(),
      style: VitrioText.weighted(VitrioText.meta, 800).copyWith(color: context.vitrio.inkMuted, letterSpacing: 0.4),
    ),
  );
}
