import 'package:flutter/material.dart';

import '../tokens/vitrio_colors.dart';
import '../tokens/vitrio_radius.dart';
import '../tokens/vitrio_shadow.dart';
import '../tokens/vitrio_space.dart';
import '../tokens/vitrio_text.dart';
import 'vitrio_avatar.dart';
import 'vitrio_badge.dart';
import 'vitrio_icon.dart';
import 'vitrio_icon_name.dart';
import 'vitrio_star.dart';

class VitrioProviderCard extends StatelessWidget {
  const VitrioProviderCard({
    super.key,
    required this.name,
    required this.profession,
    required this.priceLabel,
    required this.priceValue,
    required this.actionLabel,
    this.seed,
    this.initials,
    this.avatarUrl,
    this.verified = false,
    this.rating,
    this.meta,
    this.badges = const [],
    this.onTap,
  }) : compact = false;

  const VitrioProviderCard.compact({
    super.key,
    required this.name,
    required this.profession,
    this.seed,
    this.initials,
    this.avatarUrl,
    this.verified = false,
    this.rating,
    this.meta,
    this.badges = const [],
    this.onTap,
  })  : compact = true,
        priceLabel = '',
        priceValue = '',
        actionLabel = '';

  final String name;
  final String profession;
  final String? seed;
  final String? initials;
  final String? avatarUrl;
  final bool verified;
  final String? rating;
  final String? meta;
  final List<VitrioBadge> badges;
  final String priceLabel;
  final String priceValue;
  final String actionLabel;
  final bool compact;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    const radius = BorderRadius.all(VitrioRadius.card);
    return DecoratedBox(
      decoration: BoxDecoration(
        borderRadius: radius,
        boxShadow: compact ? null : VitrioShadow.sm(context),
      ),
      child: Material(
        color: c.surfaceRaised,
        shape: RoundedRectangleBorder(
          borderRadius: radius,
          side: BorderSide(color: c.lineSoft),
        ),
        child: InkWell(
          borderRadius: radius,
          onTap: onTap,
          child: Padding(
            padding: compact
                ? const EdgeInsets.all(VitrioSpace.sp16)
                : const EdgeInsets.symmetric(
                    horizontal: VitrioSpace.sp16,
                    vertical: VitrioSpace.sp14,
                  ),
            child: compact ? _compact(c) : _full(c),
          ),
        ),
      ),
    );
  }

  Widget _full(VitrioColors c) => Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              VitrioAvatar(
                name: name,
                initials: initials,
                imageUrl: avatarUrl,
                seed: seed,
                size: 60,
              ),
              const SizedBox(width: VitrioSpace.sp14),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    _name(c, VitrioText.labelLg, 16),
                    const SizedBox(height: VitrioSpace.sp4),
                    Text(
                      profession,
                      style: VitrioText.meta.copyWith(color: c.inkMuted),
                    ),
                    if (rating != null || meta != null) ...[
                      const SizedBox(height: VitrioSpace.sp4),
                      _ratingLine(c),
                    ],
                  ],
                ),
              ),
            ],
          ),
          if (badges.isNotEmpty) ...[
            const SizedBox(height: VitrioSpace.sp10),
            Wrap(
              spacing: VitrioSpace.sp6,
              runSpacing: VitrioSpace.sp6,
              children: badges,
            ),
          ],
          const SizedBox(height: VitrioSpace.sp12),
          Row(
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      priceLabel,
                      style: VitrioText.weighted(
                        VitrioText.caption,
                        600,
                      ).copyWith(color: c.inkMuted),
                    ),
                    Text(
                      priceValue,
                      style: VitrioText.price.copyWith(color: c.ink),
                    ),
                  ],
                ),
              ),
              Container(
                height: 40,
                padding: const EdgeInsets.symmetric(horizontal: 18),
                alignment: Alignment.center,
                decoration: BoxDecoration(
                  color: c.plum,
                  borderRadius: const BorderRadius.all(VitrioRadius.md),
                ),
                child: Text(
                  actionLabel,
                  style: VitrioText.label.copyWith(color: c.onPlum),
                ),
              ),
            ],
          ),
        ],
      );

  Widget _compact(VitrioColors c) => Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              VitrioAvatar(
                name: name,
                initials: initials,
                imageUrl: avatarUrl,
                seed: seed,
                size: 48,
              ),
              const SizedBox(width: VitrioSpace.sp12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    _name(c, VitrioText.bodyStrong, 15),
                    const SizedBox(height: 2),
                    Text(
                      profession,
                      overflow: TextOverflow.ellipsis,
                      style: VitrioText.meta.copyWith(color: c.inkMuted),
                    ),
                  ],
                ),
              ),
            ],
          ),
          if (rating != null || meta != null) ...[
            const SizedBox(height: VitrioSpace.sp12),
            _ratingLine(c),
          ],
          if (badges.isNotEmpty) ...[
            const SizedBox(height: VitrioSpace.sp12),
            Wrap(
              spacing: VitrioSpace.sp6,
              runSpacing: VitrioSpace.sp6,
              children: badges,
            ),
          ],
        ],
      );

  Widget _name(VitrioColors c, TextStyle style, double shieldSize) => Row(
        children: [
          Flexible(
            child: Text(
              name,
              overflow: TextOverflow.ellipsis,
              style: style.copyWith(color: c.ink),
            ),
          ),
          if (verified) ...[
            const SizedBox(width: VitrioSpace.sp6),
            VitrioIcon(
              VitrioIconName.shieldCheck,
              size: shieldSize,
              color: c.green,
            ),
          ],
        ],
      );

  Widget _ratingLine(VitrioColors c) => Row(
        children: [
          if (rating != null) ...[
            const VitrioStar(),
            const SizedBox(width: 5),
            Text(
              rating!,
              style: VitrioText.weighted(
                VitrioText.meta,
                800,
              ).copyWith(color: c.ink),
            ),
            const SizedBox(width: 5),
          ],
          if (meta != null)
            Flexible(
              child: Text(
                meta!,
                overflow: TextOverflow.ellipsis,
                style: VitrioText.weighted(
                  VitrioText.meta,
                  400,
                ).copyWith(color: c.inkMuted),
              ),
            ),
        ],
      );
}
