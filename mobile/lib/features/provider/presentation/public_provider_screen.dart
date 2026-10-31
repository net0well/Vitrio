import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:share_plus/share_plus.dart';
import 'package:url_launcher/url_launcher.dart';

import '../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../core/design_system/tokens/vitrio_radius.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/design_system/widgets/vitrio_action_bar.dart';
import '../../../core/design_system/widgets/vitrio_avatar.dart';
import '../../../core/design_system/widgets/vitrio_badge.dart';
import '../../../core/design_system/widgets/vitrio_badge_tone.dart';
import '../../../core/design_system/widgets/vitrio_button.dart';
import '../../../core/design_system/widgets/vitrio_button_variant.dart';
import '../../../core/design_system/widgets/vitrio_chip.dart';
import '../../../core/design_system/widgets/vitrio_chip_style.dart';
import '../../../core/design_system/widgets/vitrio_empty_state.dart';
import '../../../core/design_system/widgets/vitrio_icon.dart';
import '../../../core/design_system/widgets/vitrio_icon_button.dart';
import '../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../core/design_system/widgets/vitrio_list_card.dart';
import '../../../core/design_system/widgets/vitrio_section_header.dart';
import '../../../core/design_system/widgets/vitrio_star.dart';
import '../../../core/design_system/widgets/vitrio_top_bar.dart';
import '../../../core/presentation/brazilian_format.dart';
import '../../../l10n/app_localizations.dart';
import '../../favorites/presentation/favorite_button.dart';
import '../domain/public_photo.dart';
import '../domain/public_provider.dart';
import 'provider_labels.dart';
import 'provider_queries.dart';
import 'widgets/service_price_row.dart';

class PublicProviderScreen extends ConsumerWidget {
  const PublicProviderScreen({super.key, required this.providerId});

  final String providerId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final provider = ref.watch(publicProviderProvider(providerId));
    if (provider case AsyncData(value: final PublicProvider value)) {
      return AnnotatedRegion<SystemUiOverlayStyle>(
        value: SystemUiOverlayStyle.light,
        child: Scaffold(
          body: Stack(
            children: [
              _Profile(provider: value),
              Positioned(
                top: 0,
                left: 0,
                right: 0,
                height: MediaQuery.paddingOf(context).top,
                child: ColoredBox(color: context.vitrio.hero),
              ),
            ],
          ),
          bottomNavigationBar: value.whatsAppPhone == null
              ? null
              : VitrioActionBar(
                  children: [
                    VitrioButton(
                      label: l10n.providerWhatsAppAction,
                      variant: VitrioButtonVariant.secondary,
                      leadingIcon: VitrioIconName.message,
                      onPressed: () => launchUrl(
                        Uri.parse('https://wa.me/${value.whatsAppPhone!.replaceAll(RegExp(r'\D'), '')}'),
                        mode: LaunchMode.externalApplication,
                      ),
                    ),
                  ],
                ),
        ),
      );
    }

    return Scaffold(
      appBar: const VitrioTopBar(),
      body: switch (provider) {
        AsyncData() => Center(
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp20),
              child: VitrioEmptyState(icon: VitrioIconName.user, message: l10n.providerNotFound),
            ),
          ),
        AsyncError() => Center(
            child: VitrioButton(
              label: l10n.commonRetry,
              variant: VitrioButtonVariant.ghost,
              onPressed: () => ref.invalidate(publicProviderProvider(providerId)),
            ),
          ),
        _ => const Center(child: CircularProgressIndicator()),
      },
    );
  }
}

class _Profile extends StatelessWidget {
  const _Profile({required this.provider});

  static const _coverHeight = 150.0;
  static const _avatarSize = 96.0;

  final PublicProvider provider;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final top = MediaQuery.paddingOf(context).top;
    final rating = provider.ratingScore;
    final hours = availabilitySummary(l10n, provider.availability, emergencies: provider.handlesEmergencies);
    final years = provider.yearsOfExperience;
    final metrics = [
      if (!provider.isNew && rating != null)
        _Metric(
          value: rating.toStringAsFixed(1).replaceAll('.', ','),
          label: l10n.providerMetricReviews(provider.reviewCount ?? 0),
          star: true,
        ),
      if (provider.distanceKm case final distance?)
        _Metric(value: l10n.providerDistance(formatDistance(distance)), label: l10n.providerMetricDistance),
    ];
    return ListView(
      padding: EdgeInsets.zero,
      children: [
        Stack(
          clipBehavior: Clip.none,
          children: [
            _Cover(height: _coverHeight + top, top: top, provider: provider),
            PositionedDirectional(
              start: VitrioSpace.sp20,
              bottom: -_avatarSize / 2,
              child: Container(
                width: _avatarSize,
                height: _avatarSize,
                padding: const EdgeInsets.all(VitrioSpace.sp4),
                decoration: BoxDecoration(color: c.surface, borderRadius: VitrioRadius.providerAvatar(_avatarSize)),
                child: VitrioAvatar(
                  name: provider.displayName,
                  imageUrl: provider.avatarUrl,
                  seed: provider.id,
                  size: _avatarSize - VitrioSpace.sp8,
                ),
              ),
            ),
          ],
        ),
        Padding(
          padding: const EdgeInsets.fromLTRB(VitrioSpace.sp20, _avatarSize / 2 + VitrioSpace.sp12, VitrioSpace.sp20, VitrioSpace.sp24),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  Flexible(
                    child: Semantics(
                      header: true,
                      child: Text(provider.displayName, style: VitrioText.title.copyWith(fontSize: 26, color: c.ink)),
                    ),
                  ),
                  if (provider.meiVerified) ...[
                    const SizedBox(width: VitrioSpace.sp8),
                    VitrioIcon(VitrioIconName.shieldCheck, size: 20, color: c.green, semanticLabel: l10n.providerBadgeMei),
                  ],
                ],
              ),
              const SizedBox(height: VitrioSpace.sp4),
              Text(
                years == null ? provider.headline : '${provider.headline} · ${l10n.providerYearsExperience(years)}',
                style: VitrioText.weighted(VitrioText.body, 600).copyWith(color: c.inkMuted),
              ),
              const SizedBox(height: VitrioSpace.sp12),
              Wrap(
                spacing: VitrioSpace.sp8,
                runSpacing: VitrioSpace.sp8,
                children: [
                  if (provider.meiVerified)
                    VitrioBadge(label: l10n.providerBadgeMei, tone: VitrioBadgeTone.verified, icon: const VitrioIcon(VitrioIconName.shieldCheck)),
                  if (provider.availableToday) VitrioBadge(label: l10n.providerBadgeToday, dot: true, dotColor: c.green),
                  if (provider.isNew) VitrioBadge(label: l10n.providerNew, tone: VitrioBadgeTone.warning),
                ],
              ),
              if (metrics.isNotEmpty) ...[
                const SizedBox(height: VitrioSpace.sp20),
                Row(
                  children: [
                    for (var index = 0; index < 3; index++) ...[
                      if (index > 0) const SizedBox(width: VitrioSpace.sp10),
                      Expanded(child: index < metrics.length ? metrics[index] : const SizedBox.shrink()),
                    ],
                  ],
                ),
              ],
              if (provider.about case final about?) ...[
                const SizedBox(height: VitrioSpace.sp24),
                VitrioSectionHeader(l10n.providerAboutTitle),
                const SizedBox(height: VitrioSpace.sp8),
                Text(about, style: VitrioText.body.copyWith(color: c.ink2, height: 1.6)),
              ],
              const SizedBox(height: VitrioSpace.sp24),
              VitrioSectionHeader(l10n.providerWhereWhen),
              const SizedBox(height: VitrioSpace.sp10),
              if (hours != null) ...[
                _InfoLine(icon: VitrioIconName.clock, text: hours),
                const SizedBox(height: VitrioSpace.sp8),
              ],
              _InfoLine(icon: VitrioIconName.pin, text: l10n.providerRadiusFrom(provider.radiusKm, '${provider.city.name}, ${provider.city.state}')),
              if (provider.neighborhoods.isNotEmpty) ...[
                const SizedBox(height: VitrioSpace.sp12),
                _Neighborhoods(names: provider.neighborhoods),
              ],
              const SizedBox(height: VitrioSpace.sp24),
              VitrioSectionHeader(l10n.providerServices),
              const SizedBox(height: VitrioSpace.sp12),
              VitrioListCard(children: [for (final service in provider.services) ServicePriceRow(service: service)]),
              const SizedBox(height: VitrioSpace.sp24),
              VitrioSectionHeader(l10n.providerWorks),
              const SizedBox(height: VitrioSpace.sp12),
              if (provider.photos.isEmpty)
                Text(l10n.providerNoPhotos, style: VitrioText.body.copyWith(color: c.inkMuted))
              else
                GridView.count(
                  crossAxisCount: 3,
                  shrinkWrap: true,
                  physics: const NeverScrollableScrollPhysics(),
                  mainAxisSpacing: VitrioSpace.sp8,
                  crossAxisSpacing: VitrioSpace.sp8,
                  children: [for (final photo in provider.photos) _PhotoTile(photo: photo)],
                ),
              const SizedBox(height: VitrioSpace.sp24),
              VitrioSectionHeader(l10n.providerReviews),
              const SizedBox(height: VitrioSpace.sp12),
              _Reviews(provider: provider),
            ],
          ),
        ),
      ],
    );
  }
}

class _Cover extends StatelessWidget {
  const _Cover({required this.height, required this.top, required this.provider});

  final double height;
  final double top;
  final PublicProvider provider;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    Widget glass(double end, double offset, double blockHeight, Color color) => PositionedDirectional(
          end: end,
          top: offset,
          width: 96,
          height: blockHeight,
          child: DecoratedBox(decoration: BoxDecoration(color: color, borderRadius: const BorderRadius.all(VitrioRadius.hero))),
        );
    return SizedBox(
      height: height,
      child: ClipRect(
        child: ColoredBox(
          color: c.hero,
          child: Stack(
            children: [
              glass(80, -30, 120, c.onHero.withValues(alpha: 0.06)),
              glass(-26, -10, 70, c.lime.withValues(alpha: 0.22)),
              glass(-26, 70, 70, c.onHero.withValues(alpha: 0.06)),
              glass(80, 100, 70, c.onHero.withValues(alpha: 0.04)),
              PositionedDirectional(
                start: VitrioSpace.sp20,
                top: top + VitrioSpace.sp16,
                child: VitrioIconButton(
                  icon: VitrioIconName.chevronLeft,
                  label: MaterialLocalizations.of(context).backButtonTooltip,
                  onPressed: () => Navigator.of(context).maybePop(),
                ),
              ),
              PositionedDirectional(
                end: VitrioSpace.sp20,
                top: top + VitrioSpace.sp16,
                child: Row(
                  children: [
                    FavoriteButton(providerId: provider.id, saved: provider.isFavorite),
                    const SizedBox(width: VitrioSpace.sp10),
                    VitrioIconButton(
                      icon: VitrioIconName.share,
                      label: l10n.providerShare,
                      onPressed: () => SharePlus.instance.share(ShareParams(text: l10n.providerShareText(provider.shareUrl))),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _Metric extends StatelessWidget {
  const _Metric({required this.value, required this.label, this.star = false});

  final String value;
  final String label;
  final bool star;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    return MergeSemantics(
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp8, vertical: VitrioSpace.sp12),
        decoration: BoxDecoration(
          color: c.surfaceRaised,
          borderRadius: const BorderRadius.all(VitrioRadius.xxl),
          border: Border.all(color: c.lineSoft),
        ),
        child: Column(
          children: [
            Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                if (star) ...[const VitrioStar(size: 18), const SizedBox(width: VitrioSpace.sp4)],
                Text(value, style: VitrioText.title.copyWith(color: c.ink)),
              ],
            ),
            const SizedBox(height: 2),
            Text(label, textAlign: TextAlign.center, style: VitrioText.weighted(VitrioText.caption, 600).copyWith(color: c.inkMuted)),
          ],
        ),
      ),
    );
  }
}

class _InfoLine extends StatelessWidget {
  const _InfoLine({required this.icon, required this.text});

  final VitrioIconName icon;
  final String text;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Padding(padding: const EdgeInsets.only(top: 1), child: VitrioIcon(icon, size: 18, color: c.plum)),
        const SizedBox(width: VitrioSpace.sp8),
        Expanded(child: Text(text, style: VitrioText.body.copyWith(color: c.ink2))),
      ],
    );
  }
}

class _Neighborhoods extends StatefulWidget {
  const _Neighborhoods({required this.names});

  final List<String> names;

  @override
  State<_Neighborhoods> createState() => _NeighborhoodsState();
}

class _NeighborhoodsState extends State<_Neighborhoods> {
  static const _visible = 5;

  bool _expanded = false;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final hidden = widget.names.length - _visible;
    final shown = _expanded || hidden <= 0 ? widget.names : widget.names.take(_visible);
    return Wrap(
      spacing: VitrioSpace.sp8,
      runSpacing: VitrioSpace.sp8,
      children: [
        for (final name in shown) VitrioChip(label: name, selected: false),
        if (!_expanded && hidden > 0)
          VitrioChip(
            label: l10n.providerMoreNeighborhoods(hidden),
            selected: true,
            style: VitrioChipStyle.soft,
            onSelected: (_) => setState(() => _expanded = true),
          ),
      ],
    );
  }
}

class _PhotoTile extends StatelessWidget {
  const _PhotoTile({required this.photo});

  final PublicPhoto photo;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    final placeholder = ColoredBox(
      color: c.plum,
      child: Center(child: Opacity(opacity: 0.45, child: VitrioIcon(VitrioIconName.image, size: 30, color: c.onPlum))),
    );
    return Semantics(
      image: true,
      label: photo.caption,
      child: ClipRRect(
        borderRadius: const BorderRadius.all(VitrioRadius.xl),
        child: Stack(
          fit: StackFit.expand,
          children: [
            Image.network(photo.thumbnailUrl, fit: BoxFit.cover, errorBuilder: (_, _, _) => placeholder),
            if (photo.caption case final caption?)
              Align(
                alignment: Alignment.bottomCenter,
                child: Container(
                  width: double.infinity,
                  padding: const EdgeInsets.fromLTRB(VitrioSpace.sp10, VitrioSpace.sp20, VitrioSpace.sp10, VitrioSpace.sp10),
                  decoration: BoxDecoration(
                    gradient: LinearGradient(
                      begin: Alignment.topCenter,
                      end: Alignment.bottomCenter,
                      colors: [c.hero.withValues(alpha: 0), c.hero.withValues(alpha: 0.8)],
                    ),
                  ),
                  child: Text(caption, maxLines: 1, overflow: TextOverflow.ellipsis, style: VitrioText.caption.copyWith(color: c.onHero)),
                ),
              ),
          ],
        ),
      ),
    );
  }
}

class _Reviews extends StatelessWidget {
  const _Reviews({required this.provider});

  final PublicProvider provider;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final rating = provider.ratingScore;
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(VitrioSpace.sp16),
      decoration: BoxDecoration(
        color: c.surfaceRaised,
        borderRadius: const BorderRadius.all(VitrioRadius.card),
        border: Border.all(color: c.lineSoft),
      ),
      child: provider.isNew || rating == null
          ? Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                VitrioBadge(label: l10n.providerNew, tone: VitrioBadgeTone.warning),
                const SizedBox(height: VitrioSpace.sp10),
                Text(l10n.providerNoReviews, style: VitrioText.body.copyWith(color: c.ink2)),
              ],
            )
          : Row(
              children: [
                Text(rating.toStringAsFixed(1).replaceAll('.', ','), style: VitrioText.priceLg.copyWith(fontSize: 42, color: c.ink)),
                const SizedBox(width: VitrioSpace.sp16),
                Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Semantics(
                      label: l10n.providerRatingOf(rating.toStringAsFixed(1).replaceAll('.', ',')),
                      child: Row(
                        children: [
                          for (var star = 1; star <= 5; star++) ...[
                            if (star > 1) const SizedBox(width: 2),
                            VitrioStar(size: 16, empty: rating < star - 0.5),
                          ],
                        ],
                      ),
                    ),
                    const SizedBox(height: VitrioSpace.sp4),
                    Text(
                      l10n.providerMetricReviews(provider.reviewCount ?? 0),
                      style: VitrioText.weighted(VitrioText.caption, 600).copyWith(color: c.inkMuted),
                    ),
                  ],
                ),
              ],
            ),
    );
  }
}
