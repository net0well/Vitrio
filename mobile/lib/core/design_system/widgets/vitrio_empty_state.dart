import 'package:flutter/material.dart';

import '../tokens/vitrio_colors.dart';
import '../tokens/vitrio_radius.dart';
import '../tokens/vitrio_space.dart';
import '../tokens/vitrio_text.dart';
import 'vitrio_icon.dart';
import 'vitrio_icon_name.dart';

class VitrioEmptyState extends StatelessWidget {
  const VitrioEmptyState({
    super.key,
    required this.icon,
    this.title,
    required this.message,
    this.actionLabel,
    this.onAction,
  });

  final VitrioIconName icon;
  final String? title;
  final String message;
  final String? actionLabel;
  final VoidCallback? onAction;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    return Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        Container(
          width: VitrioSpace.controlLg,
          height: VitrioSpace.controlLg,
          decoration: BoxDecoration(
            color: c.plumSoft,
            borderRadius: const BorderRadius.all(VitrioRadius.lg),
          ),
          alignment: Alignment.center,
          child: VitrioIcon(icon, size: 24, color: c.plum),
        ),
        const SizedBox(height: VitrioSpace.sp16),
        if (title != null) ...[
          Text(
            title!,
            textAlign: TextAlign.center,
            style: VitrioText.title.copyWith(color: c.ink),
          ),
          const SizedBox(height: VitrioSpace.sp8),
        ],
        Text(
          message,
          textAlign: TextAlign.center,
          style: VitrioText.body.copyWith(color: c.ink2),
        ),
        if (actionLabel != null && onAction != null) ...[
          const SizedBox(height: VitrioSpace.sp20),
          OutlinedButton(onPressed: onAction, child: Text(actionLabel!)),
        ],
      ],
    );
  }
}
