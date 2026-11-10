import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../../core/design_system/tokens/vitrio_radius.dart';
import '../../../../core/design_system/tokens/vitrio_space.dart';
import '../../../../core/design_system/tokens/vitrio_text.dart';
import '../../../../core/design_system/widgets/vitrio_avatar.dart';
import '../../../../core/design_system/widgets/vitrio_avatar_kind.dart';
import '../../../../core/design_system/widgets/vitrio_avatar_tone.dart';
import '../../../../core/design_system/widgets/vitrio_badge.dart';
import '../../../../core/design_system/widgets/vitrio_icon.dart';
import '../../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../../core/presentation/brazilian_format.dart';
import '../../../../core/presentation/clock.dart';
import '../../../../l10n/app_localizations.dart';
import '../../../orders/domain/city_time.dart';
import '../../../orders/presentation/order_labels.dart';
import '../../domain/provider_order.dart';
import '../provider_order_labels.dart';
import 'order_info_tile.dart';
import 'order_photo_strip.dart';

class OrderRequestCard extends ConsumerWidget {
  const OrderRequestCard({super.key, required this.order});

  static const _avatar = 40.0;

  final ProviderOrder order;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final now = cityNow(ref.watch(clockProvider)(), order.cityOffset);
    final customer = order.customer;
    final distance = order.distanceKm;
    return Container(
      padding: const EdgeInsets.all(VitrioSpace.sp16),
      decoration: BoxDecoration(
        color: c.surfaceRaised,
        borderRadius: const BorderRadius.all(VitrioRadius.card),
        border: Border.all(color: c.lineSoft),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            children: [
              VitrioAvatar(name: customer.displayName, size: _avatar, kind: VitrioAvatarKind.client, tone: VitrioAvatarTone.info),
              const SizedBox(width: VitrioSpace.sp10),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(customer.displayName, style: VitrioText.bodyStrong.copyWith(color: c.ink)),
                    const SizedBox(height: 1),
                    Text(
                      l10n.respondMemberSince(memberSinceMonth(customer.memberSince), customer.orderCount),
                      style: VitrioText.weighted(VitrioText.caption, 600).copyWith(color: c.inkMuted),
                    ),
                  ],
                ),
              ),
              const SizedBox(width: VitrioSpace.sp10),
              Text(agoLabel(l10n, order.createdAt, now), style: VitrioText.weighted(VitrioText.caption, 700).copyWith(color: c.inkMuted)),
            ],
          ),
          const SizedBox(height: VitrioSpace.sp14),
          Align(
            alignment: Alignment.centerLeft,
            child: VitrioBadge(label: order.serviceTitle),
          ),
          const SizedBox(height: VitrioSpace.sp10),
          Text(order.title, style: VitrioText.labelLg.copyWith(color: c.ink)),
          const SizedBox(height: VitrioSpace.sp8),
          Text(order.description, style: VitrioText.body.copyWith(height: 1.5, color: c.ink2)),
          if (order.photos.isNotEmpty || order.photosRemoved) ...[
            const SizedBox(height: VitrioSpace.sp12),
            OrderPhotoStrip(photos: order.photos, removed: order.photosRemoved),
          ],
          const SizedBox(height: VitrioSpace.sp14),
          Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Expanded(
                child: OrderInfoTile(
                  label: l10n.respondWhen,
                  value:
                      '${capitalized(relativeDay(l10n, order.preferredDay, now))} · ${periodLabel(l10n, order.preferredPeriod).toLowerCase()}',
                ),
              ),
              const SizedBox(width: VitrioSpace.sp8),
              Expanded(
                child: OrderInfoTile(
                  label: l10n.respondWhere,
                  value: distance == null
                      ? order.neighborhood
                      : '${order.neighborhood} · ${l10n.providerDistance(formatDistance(distance))}',
                ),
              ),
            ],
          ),
          if (order.address == null) ...[
            const SizedBox(height: VitrioSpace.sp10),
            Row(
              children: [
                VitrioIcon(VitrioIconName.lock, size: 14, color: c.inkMuted),
                const SizedBox(width: VitrioSpace.sp6),
                Expanded(
                  child: Text(
                    l10n.respondAddressLocked(customer.firstName),
                    style: VitrioText.weighted(VitrioText.caption, 600).copyWith(color: c.inkMuted),
                  ),
                ),
              ],
            ),
          ],
        ],
      ),
    );
  }
}
