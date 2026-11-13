import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../../core/design_system/tokens/vitrio_radius.dart';
import '../../../../core/design_system/tokens/vitrio_space.dart';
import '../../../../core/design_system/tokens/vitrio_text.dart';
import '../../../../core/design_system/widgets/vitrio_badge.dart';
import '../../../../core/design_system/widgets/vitrio_icon.dart';
import '../../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../../core/presentation/clock.dart';
import '../../../../l10n/app_localizations.dart';
import '../../../orders/domain/city_time.dart';
import '../../../orders/presentation/order_labels.dart';
import '../../domain/provider_order_summary.dart';
import '../provider_order_labels.dart';

class ProviderOrderCard extends ConsumerWidget {
  const ProviderOrderCard({super.key, required this.order, required this.onTap});

  final ProviderOrderSummary order;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final now = cityNow(ref.watch(clockProvider)(), order.cityOffset);
    final schedule = order.schedule;
    final finished = !order.status.isOpen && schedule != null;
    final meta = [order.customerName, order.neighborhood, if (finished) shortDate(l10n, schedule.date)].join(' · ');
    final (status, tone) = providerStatus(
      l10n,
      status: order.status,
      cityNow: now,
      schedule: schedule,
      quoteValidUntil: order.quoteValidUntil,
      cancelledBy: order.cancelledBy,
    );
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
          padding: const EdgeInsets.all(VitrioSpace.sp16),
          child: Row(
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(order.title, style: VitrioText.bodyStrong.copyWith(color: c.ink)),
                    const SizedBox(height: VitrioSpace.sp4),
                    Text(meta, style: VitrioText.weighted(VitrioText.meta, 400).copyWith(color: c.inkMuted)),
                    const SizedBox(height: VitrioSpace.sp8),
                    VitrioBadge(label: status, tone: tone, dot: true),
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
