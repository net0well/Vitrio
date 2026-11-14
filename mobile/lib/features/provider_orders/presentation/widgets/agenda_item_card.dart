import 'package:flutter/material.dart';

import '../../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../../core/design_system/tokens/vitrio_radius.dart';
import '../../../../core/design_system/tokens/vitrio_space.dart';
import '../../../../core/design_system/tokens/vitrio_text.dart';
import '../../../../core/design_system/widgets/vitrio_badge.dart';
import '../../../../core/design_system/widgets/vitrio_badge_tone.dart';
import '../../../../core/design_system/widgets/vitrio_icon.dart';
import '../../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../../l10n/app_localizations.dart';
import '../../../orders/presentation/order_labels.dart';
import '../../domain/agenda_item.dart';
import '../../domain/agenda_item_kind.dart';

class AgendaItemCard extends StatelessWidget {
  const AgendaItemCard({super.key, required this.item, required this.onTap});

  static const _timeWidth = 52.0;

  final AgendaItem item;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final validUntil = item.quoteValidUntil;
    final done = item.kind == AgendaItemKind.done;
    return Material(
      color: c.surfaceRaised,
      shape: RoundedRectangleBorder(
        borderRadius: const BorderRadius.all(VitrioRadius.xxl),
        side: BorderSide(color: c.lineSoft),
      ),
      child: InkWell(
        borderRadius: const BorderRadius.all(VitrioRadius.xxl),
        onTap: onTap,
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp16, vertical: VitrioSpace.sp14),
          child: Row(
            children: [
              SizedBox(
                width: _timeWidth,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(item.start, style: VitrioText.price.copyWith(color: done ? c.inkMuted : c.ink)),
                    const SizedBox(height: 1),
                    Text(l10n.agendaUntil(item.end), style: VitrioText.weighted(VitrioText.caption, 600).copyWith(color: c.inkMuted)),
                  ],
                ),
              ),
              const SizedBox(width: VitrioSpace.sp14),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(item.title, style: VitrioText.bodyStrong.copyWith(color: c.ink)),
                    const SizedBox(height: 3),
                    Text(
                      '${item.customerName} · ${item.neighborhood}',
                      style: VitrioText.weighted(VitrioText.meta, 400).copyWith(color: c.inkMuted),
                    ),
                    if (item.kind == AgendaItemKind.quoted && validUntil != null) ...[
                      const SizedBox(height: VitrioSpace.sp6),
                      VitrioBadge(
                        label: l10n.orderQuoteValidUntil(l10n.orderDayAndTime(shortDate(l10n, validUntil), formatTime(validUntil))),
                        tone: VitrioBadgeTone.info,
                      ),
                    ],
                    if (done) ...[
                      const SizedBox(height: VitrioSpace.sp6),
                      VitrioBadge(
                        label: l10n.orderStatusCompleted,
                        tone: VitrioBadgeTone.verified,
                        icon: VitrioIcon(VitrioIconName.check, size: 12, color: c.green, strokeWidth: 3),
                      ),
                    ],
                  ],
                ),
              ),
              const SizedBox(width: VitrioSpace.sp14),
              VitrioIcon(VitrioIconName.chevronRight, size: 18, color: c.inkMuted),
            ],
          ),
        ),
      ),
    );
  }
}
