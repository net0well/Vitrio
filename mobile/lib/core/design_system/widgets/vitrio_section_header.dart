import 'package:flutter/material.dart';

import '../tokens/vitrio_colors.dart';
import '../tokens/vitrio_space.dart';
import '../tokens/vitrio_text.dart';

class VitrioSectionHeader extends StatelessWidget {
  const VitrioSectionHeader(this.title, {super.key, this.hint, this.count, this.actionLabel, this.onAction, this.trailing});

  static const _countSize = 24.0;

  final String title;
  final String? hint;
  final int? count;
  final String? actionLabel;
  final VoidCallback? onAction;
  final Widget? trailing;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          children: [
            Expanded(
              child: Row(
                children: [
                  Flexible(
                    child: Semantics(
                      header: true,
                      child: Text(title, style: VitrioText.section.copyWith(color: c.ink)),
                    ),
                  ),
                  if (count case final int count) ...[
                    const SizedBox(width: VitrioSpace.sp8),
                    Container(
                      constraints: const BoxConstraints(minWidth: _countSize),
                      height: _countSize,
                      padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp6),
                      alignment: Alignment.center,
                      decoration: ShapeDecoration(color: c.plum, shape: const StadiumBorder()),
                      child: Text('$count', style: VitrioText.caption.copyWith(color: c.onPlum)),
                    ),
                  ],
                ],
              ),
            ),
            ?trailing,
            if (actionLabel != null && onAction != null)
              TextButton(
                onPressed: onAction,
                style: TextButton.styleFrom(
                  minimumSize: const Size(VitrioSpace.tap, 32),
                  padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp4),
                  tapTargetSize: MaterialTapTargetSize.shrinkWrap,
                ),
                child: Text(actionLabel!, style: VitrioText.label.copyWith(color: c.plum)),
              ),
          ],
        ),
        if (hint != null) ...[
          const SizedBox(height: VitrioSpace.sp4),
          Text(hint!, style: VitrioText.weighted(VitrioText.caption, 600).copyWith(color: c.inkMuted)),
        ],
      ],
    );
  }
}
