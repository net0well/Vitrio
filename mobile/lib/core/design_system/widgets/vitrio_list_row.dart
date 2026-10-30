import 'package:flutter/material.dart';

import '../tokens/vitrio_colors.dart';
import '../tokens/vitrio_space.dart';
import '../tokens/vitrio_text.dart';
import 'vitrio_icon.dart';
import 'vitrio_icon_name.dart';

class VitrioListRow extends StatelessWidget {
  const VitrioListRow({
    super.key,
    required this.title,
    this.subtitle,
    this.icon,
    this.trailing,
    this.onTap,
    this.danger = false,
    this.chevron = true,
  });

  final String title;
  final String? subtitle;
  final VitrioIconName? icon;
  final Widget? trailing;
  final VoidCallback? onTap;
  final bool danger;
  final bool chevron;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    final color = danger ? c.danger : c.ink;
    return InkWell(
      onTap: onTap,
      child: ConstrainedBox(
        constraints: const BoxConstraints(minHeight: VitrioSpace.controlLg),
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp16, vertical: VitrioSpace.sp14),
          child: Row(
            children: [
              if (icon != null) ...[
                VitrioIcon(icon!, size: 20, color: danger ? c.danger : c.inkMuted),
                const SizedBox(width: VitrioSpace.sp12),
              ],
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(title, style: VitrioText.bodyStrong.copyWith(color: color)),
                    if (subtitle != null) ...[
                      const SizedBox(height: 2),
                      Text(subtitle!, style: VitrioText.meta.copyWith(color: c.inkMuted)),
                    ],
                  ],
                ),
              ),
              if (trailing != null) ...[const SizedBox(width: VitrioSpace.sp12), trailing!]
              else if (onTap != null && chevron && !danger) ...[
                const SizedBox(width: VitrioSpace.sp12),
                VitrioIcon(VitrioIconName.chevronRight, size: 18, color: c.inkMuted),
              ],
            ],
          ),
        ),
      ),
    );
  }
}
