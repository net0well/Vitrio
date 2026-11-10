import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../../core/design_system/tokens/vitrio_radius.dart';
import '../../../../core/design_system/tokens/vitrio_space.dart';
import '../../../../core/design_system/tokens/vitrio_text.dart';
import '../../../../core/design_system/widgets/vitrio_badge.dart';
import '../../../../core/design_system/widgets/vitrio_badge_tone.dart';
import '../../../../core/design_system/widgets/vitrio_button.dart';
import '../../../../core/design_system/widgets/vitrio_button_size.dart';
import '../../../../core/design_system/widgets/vitrio_button_variant.dart';
import '../../../../core/design_system/widgets/vitrio_icon.dart';
import '../../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../../core/presentation/brazilian_format.dart';
import '../../../../core/presentation/clock.dart';
import '../../../../l10n/app_localizations.dart';
import '../../../orders/domain/city_time.dart';
import '../../../orders/presentation/order_labels.dart';
import '../../domain/provider_order_summary.dart';
import '../provider_order_labels.dart';

class NewOrderCard extends ConsumerWidget {
  const NewOrderCard({
    super.key,
    required this.order,
    required this.onDecline,
    required this.onRespond,
    this.highlighted = false,
    this.newBadge = true,
    this.showDeadline = true,
  });

  static const _declineWidth = 96.0;

  final ProviderOrderSummary order;
  final VoidCallback onDecline;
  final VoidCallback onRespond;
  final bool highlighted;
  final bool newBadge;
  final bool showDeadline;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final now = cityNow(ref.watch(clockProvider)(), order.cityOffset);
    final distance = order.distanceKm;
    final where = [
      order.customerName.split(' ').first,
      order.neighborhood,
      if (distance != null) l10n.providerDistance(formatDistance(distance)),
    ].join(' · ');
    return Container(
      padding: const EdgeInsets.all(VitrioSpace.sp16),
      decoration: BoxDecoration(
        color: c.surfaceRaised,
        borderRadius: const BorderRadius.all(VitrioRadius.card),
        border: Border.all(color: highlighted ? c.plum : c.lineSoft, width: highlighted ? 1.5 : 1),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            children: [
              Flexible(
                child: Align(
                  alignment: Alignment.centerLeft,
                  child: newBadge
                      ? VitrioBadge(label: l10n.providerNewOrderBadge(order.serviceTitle), tone: VitrioBadgeTone.accent)
                      : VitrioBadge(label: order.serviceTitle),
                ),
              ),
              const SizedBox(width: VitrioSpace.sp10),
              Text(agoLabel(l10n, order.createdAt, now), style: VitrioText.weighted(VitrioText.caption, 700).copyWith(color: c.inkMuted)),
            ],
          ),
          const SizedBox(height: VitrioSpace.sp10),
          Text(order.title, style: VitrioText.labelLg.copyWith(color: c.ink)),
          const SizedBox(height: VitrioSpace.sp6),
          _Line(icon: VitrioIconName.pin, text: where, color: c.inkMuted),
          const SizedBox(height: VitrioSpace.sp6),
          _Line(
            icon: VitrioIconName.calendar,
            text: preferenceLine(l10n, order.preferredDay, order.preferredPeriod, now),
            color: c.ink2,
            weight: 700,
          ),
          if (showDeadline) ...[
            const SizedBox(height: VitrioSpace.sp6),
            _Line(
              icon: VitrioIconName.clock,
              text: l10n.orderRespondBy(relativeMoment(l10n, order.respondBy, now)),
              color: c.amberInk,
              weight: 700,
              small: true,
            ),
          ],
          if (highlighted) ...[
            const SizedBox(height: VitrioSpace.sp8),
            Text(
              order.description,
              maxLines: 2,
              overflow: TextOverflow.ellipsis,
              style: VitrioText.weighted(VitrioText.meta, 400).copyWith(height: 1.5, color: c.inkMuted),
            ),
            if (order.photoCount > 0) ...[
              const SizedBox(height: VitrioSpace.sp8),
              _Line(
                icon: VitrioIconName.camera,
                text: l10n.orderPhotosAttached(order.photoCount),
                color: c.inkMuted,
                weight: 700,
                small: true,
              ),
            ],
          ],
          const SizedBox(height: VitrioSpace.sp14),
          Row(
            children: [
              SizedBox(
                width: _declineWidth,
                child: VitrioButton(
                  label: l10n.orderQuoteDecline,
                  variant: VitrioButtonVariant.secondary,
                  size: VitrioButtonSize.md,
                  onPressed: onDecline,
                ),
              ),
              const SizedBox(width: VitrioSpace.sp10),
              Expanded(
                child: VitrioButton(label: l10n.providerSendQuote, size: VitrioButtonSize.md, onPressed: onRespond),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class _Line extends StatelessWidget {
  const _Line({required this.icon, required this.text, required this.color, this.weight = 600, this.small = false});

  final VitrioIconName icon;
  final String text;
  final Color color;
  final int weight;
  final bool small;

  @override
  Widget build(BuildContext context) => Row(
    children: [
      VitrioIcon(icon, size: 14, color: color),
      const SizedBox(width: VitrioSpace.sp6),
      Expanded(
        child: Text(text, style: VitrioText.weighted(small ? VitrioText.caption : VitrioText.meta, weight).copyWith(color: color)),
      ),
    ],
  );
}
