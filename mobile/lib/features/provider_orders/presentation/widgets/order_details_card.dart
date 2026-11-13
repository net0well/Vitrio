import 'package:flutter/material.dart';

import '../../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../../core/design_system/tokens/vitrio_radius.dart';
import '../../../../core/design_system/tokens/vitrio_space.dart';
import '../../../../core/design_system/tokens/vitrio_text.dart';
import '../../../../core/design_system/widgets/vitrio_badge.dart';
import '../../../../l10n/app_localizations.dart';
import '../../../orders/presentation/order_labels.dart';
import '../../domain/provider_order.dart';
import 'order_photo_strip.dart';
import 'order_section_label.dart';

class OrderDetailsCard extends StatelessWidget {
  const OrderDetailsCard({super.key, required this.order, required this.now});

  final ProviderOrder order;
  final DateTime now;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final preference = l10n.orderPreferenceLine(
      relativeDay(l10n, order.preferredDay, now),
      periodLabel(l10n, order.preferredPeriod).toLowerCase(),
      periodHours(l10n, order.preferredPeriod),
    );
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        OrderSectionLabel(l10n.providerOrderSection),
        const SizedBox(height: VitrioSpace.sp10),
        Container(
          padding: const EdgeInsets.all(VitrioSpace.sp16),
          decoration: BoxDecoration(
            color: c.surfaceRaised,
            borderRadius: const BorderRadius.all(VitrioRadius.card),
            border: Border.all(color: c.lineSoft),
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Align(
                alignment: Alignment.centerLeft,
                child: VitrioBadge(label: order.serviceTitle),
              ),
              const SizedBox(height: VitrioSpace.sp10),
              Text(order.description, style: VitrioText.body.copyWith(height: 1.5, color: c.ink2)),
              if (order.photos.isNotEmpty || order.photosRemoved) ...[
                const SizedBox(height: VitrioSpace.sp12),
                OrderPhotoStrip(photos: order.photos, removed: order.photosRemoved),
              ],
              const SizedBox(height: VitrioSpace.sp12),
              Text(
                l10n.providerCustomerAsked(order.customer.firstName, preference),
                style: VitrioText.weighted(VitrioText.caption, 600).copyWith(color: c.inkMuted),
              ),
            ],
          ),
        ),
      ],
    );
  }
}
