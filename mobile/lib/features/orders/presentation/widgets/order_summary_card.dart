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
import '../../../provider/presentation/provider_queries.dart';
import '../../domain/customer_order_summary.dart';
import '../../domain/order_status.dart';
import '../order_labels.dart';

class OrderSummaryCard extends ConsumerWidget {
  const OrderSummaryCard({super.key, required this.order, required this.onTap});

  static const _box = 44.0;

  final CustomerOrderSummary order;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final now = ref.watch(clockProvider)();
    final category = ref.watch(serviceCategoriesProvider).value?.where((candidate) => candidate.id == order.categoryId).firstOrNull;
    final tones = [(c.plumSoft, c.plum), (c.amberSoft, c.amberInk), (c.greenSoft, c.green), (c.infoSoft, c.info)];
    final (boxColor, iconColor) = tones[order.categoryId % tones.length];
    final schedule = order.schedule;
    final meta = order.status == OrderStatus.scheduled && schedule != null
        ? l10n.ordersScheduledMeta(order.providerName, serviceWhen(l10n, schedule, now))
        : l10n.ordersSentMeta(order.providerName, relativeMoment(l10n, order.createdAt, now));
    final tone = statusTone(order.status);
    return Material(
      color: c.surfaceRaised,
      shape: RoundedRectangleBorder(borderRadius: const BorderRadius.all(VitrioRadius.xxl), side: BorderSide(color: c.lineSoft)),
      child: InkWell(
        borderRadius: const BorderRadius.all(VitrioRadius.xxl),
        onTap: onTap,
        child: Padding(
          padding: const EdgeInsets.all(VitrioSpace.sp16),
          child: Row(
            children: [
              Container(
                width: _box,
                height: _box,
                alignment: Alignment.center,
                decoration: BoxDecoration(color: boxColor, borderRadius: const BorderRadius.all(VitrioRadius.lg)),
                child: VitrioIcon(
                  category == null ? VitrioIconName.orders : VitrioIconName.fromFileName(category.icon, fallback: VitrioIconName.orders),
                  color: iconColor,
                ),
              ),
              const SizedBox(width: VitrioSpace.sp14),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(order.title, style: VitrioText.bodyStrong.copyWith(color: c.ink)),
                    const SizedBox(height: VitrioSpace.sp4),
                    Text(meta, style: VitrioText.weighted(VitrioText.meta, 400).copyWith(color: c.inkMuted)),
                    const SizedBox(height: VitrioSpace.sp8),
                    VitrioBadge(label: statusLabel(l10n, order.status), tone: tone, dot: true),
                  ],
                ),
              ),
              const SizedBox(width: VitrioSpace.sp8),
              VitrioIcon(VitrioIconName.chevronRight, size: 18, color: c.inkMuted),
            ],
          ),
        ),
      ),
    );
  }
}
