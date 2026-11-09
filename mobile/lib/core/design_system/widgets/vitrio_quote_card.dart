import 'package:flutter/material.dart';

import '../tokens/vitrio_colors.dart';
import '../tokens/vitrio_radius.dart';
import '../tokens/vitrio_shadow.dart';
import '../tokens/vitrio_space.dart';
import '../tokens/vitrio_text.dart';
import 'vitrio_badge.dart';
import 'vitrio_badge_tone.dart';
import 'vitrio_button.dart';
import 'vitrio_button_size.dart';
import 'vitrio_button_variant.dart';
import 'vitrio_icon.dart';
import 'vitrio_quote_row.dart';

class VitrioQuoteCard extends StatelessWidget {
  const VitrioQuoteCard({
    super.key,
    required this.title,
    required this.price,
    required this.rows,
    this.validity,
    this.message,
    this.acceptLabel,
    this.declineLabel,
    this.onAccept,
    this.onDecline,
    this.busy = false,
  });

  static const _declineWidth = 104.0;

  final String title;
  final String price;
  final List<VitrioQuoteRow> rows;
  final String? validity;
  final String? message;
  final String? acceptLabel;
  final String? declineLabel;
  final VoidCallback? onAccept;
  final VoidCallback? onDecline;
  final bool busy;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    return Container(
      padding: const EdgeInsets.all(VitrioSpace.sp20),
      decoration: BoxDecoration(
        color: c.surfaceRaised,
        borderRadius: const BorderRadius.all(VitrioRadius.hero),
        border: Border.all(color: c.plum, width: 1.5),
        boxShadow: VitrioShadow.md(context),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Wrap(
            alignment: WrapAlignment.spaceBetween,
            crossAxisAlignment: WrapCrossAlignment.center,
            spacing: VitrioSpace.sp10,
            runSpacing: VitrioSpace.sp8,
            children: [
              Text(title, style: VitrioText.bodyStrong.copyWith(color: c.ink)),
              if (validity != null) VitrioBadge(label: validity!, tone: VitrioBadgeTone.warning),
            ],
          ),
          const SizedBox(height: VitrioSpace.sp8),
          Text(price, style: VitrioText.priceLg.copyWith(color: c.ink)),
          if (rows.isNotEmpty) ...[
            const SizedBox(height: VitrioSpace.sp12),
            for (final (index, row) in rows.indexed) ...[
              if (index > 0) const SizedBox(height: VitrioSpace.sp8),
              Row(
                children: [
                  VitrioIcon(row.icon, size: 17, color: row.positive ? c.green : c.plum),
                  const SizedBox(width: VitrioSpace.sp8),
                  Expanded(child: Text(row.text, style: VitrioText.body.copyWith(color: c.ink2))),
                ],
              ),
            ],
          ],
          if (message != null) ...[
            const SizedBox(height: VitrioSpace.sp12),
            Container(
              padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp12, vertical: VitrioSpace.sp10),
              decoration: BoxDecoration(color: c.surfaceInset, borderRadius: const BorderRadius.all(VitrioRadius.md)),
              child: Text(message!, style: VitrioText.weighted(VitrioText.meta, 400).copyWith(height: 1.5, color: c.ink2)),
            ),
          ],
          if (acceptLabel != null) ...[
            const SizedBox(height: VitrioSpace.sp16),
            Row(
              children: [
                if (declineLabel != null) ...[
                  SizedBox(
                    width: _declineWidth,
                    child: VitrioButton(
                      label: declineLabel!,
                      onPressed: busy ? null : onDecline,
                      variant: VitrioButtonVariant.secondary,
                      size: VitrioButtonSize.decision,
                    ),
                  ),
                  const SizedBox(width: VitrioSpace.sp10),
                ],
                Expanded(
                  child: VitrioButton(label: acceptLabel!, onPressed: onAccept, size: VitrioButtonSize.decision, busy: busy),
                ),
              ],
            ),
          ],
        ],
      ),
    );
  }
}
