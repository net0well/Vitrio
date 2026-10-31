import 'package:flutter/material.dart';

import '../tokens/vitrio_colors.dart';
import '../tokens/vitrio_space.dart';
import '../tokens/vitrio_text.dart';

class VitrioActionBar extends StatelessWidget {
  const VitrioActionBar({super.key, required this.children, this.note, this.top});

  final List<Widget> children;
  final String? note;
  final Widget? top;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    final keyboard = MediaQuery.viewInsetsOf(context).bottom;
    return Padding(
      padding: EdgeInsets.only(bottom: keyboard),
      child: DecoratedBox(
        decoration: BoxDecoration(
          color: c.surfaceRaised,
          border: Border(top: BorderSide(color: c.line)),
        ),
        child: SafeArea(
          top: false,
          minimum:
              EdgeInsets.fromLTRB(VitrioSpace.sp20, VitrioSpace.sp12, VitrioSpace.sp20, keyboard > 0 ? VitrioSpace.sp12 : VitrioSpace.sp28),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              if (top != null) ...[top!, const SizedBox(height: VitrioSpace.sp10)],
              if (note != null) ...[
                Text(
                  note!,
                  textAlign: TextAlign.center,
                  style: VitrioText.weighted(VitrioText.caption, 600).copyWith(color: c.inkMuted),
                ),
                const SizedBox(height: VitrioSpace.sp8),
              ],
              Row(
                children: [
                  for (final (index, child) in children.indexed) ...[
                    if (index > 0) const SizedBox(width: VitrioSpace.sp10),
                    if (index == children.length - 1) Expanded(child: child) else child,
                  ],
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}
