import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/routes.dart';
import '../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../core/design_system/tokens/vitrio_radius.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/design_system/widgets/vitrio_avatar.dart';
import '../../../core/design_system/widgets/vitrio_button.dart';
import '../../../core/design_system/widgets/vitrio_button_size.dart';
import '../../../core/design_system/widgets/vitrio_button_variant.dart';
import '../../../core/design_system/widgets/vitrio_icon.dart';
import '../../../core/design_system/widgets/vitrio_icon_button.dart';
import '../../../core/design_system/widgets/vitrio_icon_button_variant.dart';
import '../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../core/design_system/widgets/vitrio_section_header.dart';
import '../../../core/design_system/widgets/vitrio_switch_row.dart';
import '../../../l10n/app_localizations.dart';
import '../domain/provider_dashboard.dart';
import '../domain/provider_profile.dart';
import '../domain/provider_status.dart';
import 'provider_controller.dart';
import 'provider_queries.dart';
import 'provider_share.dart';
import 'provider_step.dart';

class ProviderDashboardScreen extends ConsumerWidget {
  const ProviderDashboardScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final profile = ref.watch(providerControllerProvider).value;
    final dashboard = ref.watch(providerDashboardProvider);
    return AnnotatedRegion<SystemUiOverlayStyle>(
      value: SystemUiOverlayStyle.light,
      child: Scaffold(
        body: switch ((profile, dashboard)) {
          (final ProviderProfile profile, AsyncData(value: final dashboard)) => RefreshIndicator(
              onRefresh: () => ref.refresh(providerDashboardProvider.future),
              child: _Dashboard(profile: profile, dashboard: dashboard),
            ),
          (_, AsyncError()) => Center(
              child: VitrioButton(
                label: l10n.commonRetry,
                variant: VitrioButtonVariant.ghost,
                onPressed: () => ref.invalidate(providerDashboardProvider),
              ),
            ),
          _ => const Center(child: CircularProgressIndicator()),
        },
      ),
    );
  }
}

class _Dashboard extends ConsumerWidget {
  const _Dashboard({required this.profile, required this.dashboard});

  final ProviderProfile profile;
  final ProviderDashboard dashboard;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final published = dashboard.status == ProviderStatus.published;
    return ListView(
      padding: EdgeInsets.zero,
      children: [
        _Header(profile: profile, dashboard: dashboard),
        Padding(
          padding: const EdgeInsets.fromLTRB(VitrioSpace.sp20, VitrioSpace.sp20, VitrioSpace.sp20, VitrioSpace.sp24),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              _ProfileCard(profile: profile, dashboard: dashboard),
              const SizedBox(height: VitrioSpace.sp24),
              VitrioSectionHeader(l10n.providerNewOrders),
              const SizedBox(height: VitrioSpace.sp12),
              Container(
                padding: const EdgeInsets.all(VitrioSpace.sp16),
                decoration: BoxDecoration(
                  color: c.surfaceRaised,
                  borderRadius: const BorderRadius.all(VitrioRadius.card),
                  border: Border.all(color: c.lineSoft),
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(l10n.providerNoOrders, style: VitrioText.bodyStrong.copyWith(color: c.ink)),
                    const SizedBox(height: VitrioSpace.sp4),
                    Text(
                      published ? l10n.providerNoOrdersShare : l10n.providerNoOrdersPublish,
                      style: VitrioText.meta.copyWith(color: c.inkMuted),
                    ),
                    if (published) ...[
                      const SizedBox(height: VitrioSpace.sp12),
                      VitrioButton(
                        label: l10n.providerShare,
                        variant: VitrioButtonVariant.secondary,
                        size: VitrioButtonSize.md,
                        leadingIcon: VitrioIconName.share,
                        onPressed: () => shareProviderProfile(context, ref, profile.id),
                      ),
                    ],
                  ],
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }
}

class _Header extends ConsumerWidget {
  const _Header({required this.profile, required this.dashboard});

  final ProviderProfile profile;
  final ProviderDashboard dashboard;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final published = dashboard.status == ProviderStatus.published;
    final available = published && dashboard.availableToday;
    final place = profile.serviceArea?.neighborhood ?? profile.serviceArea?.city.name;
    return Container(
      padding: EdgeInsets.fromLTRB(
        VitrioSpace.sp20,
        MediaQuery.paddingOf(context).top + VitrioSpace.sp20,
        VitrioSpace.sp20,
        VitrioSpace.sp24,
      ),
      decoration: BoxDecoration(
        color: c.hero,
        borderRadius: const BorderRadius.vertical(bottom: VitrioRadius.hero),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            children: [
              VitrioAvatar(name: profile.displayName, imageUrl: profile.avatarUrl, onHero: true),
              const SizedBox(width: VitrioSpace.sp12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      l10n.homeGreeting(profile.displayName.split(' ').first),
                      style: VitrioText.section.copyWith(color: c.onHero),
                    ),
                    const SizedBox(height: 1),
                    Text(
                      place == null ? profile.headline : '${profile.headline} · $place',
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                      style: VitrioText.meta.copyWith(color: c.onHeroMuted),
                    ),
                  ],
                ),
              ),
              const SizedBox(width: VitrioSpace.sp12),
              VitrioIconButton(
                icon: VitrioIconName.bell,
                label: l10n.notificationsTitle,
                variant: VitrioIconButtonVariant.glass,
                onPressed: () => const NotificationsRoute().push<void>(context),
              ),
            ],
          ),
          const SizedBox(height: VitrioSpace.sp20),
          VitrioSwitchRow(
            onHero: true,
            label: available ? l10n.providerAvailableTitle : l10n.providerUnavailableTitle,
            description: !published
                ? l10n.providerAvailableNeedsPublish
                : available
                    ? l10n.providerAvailableSubtitle
                    : l10n.providerUnavailableSubtitle,
            value: available,
            onChanged: published
                ? (value) async {
                    await ref.read(providerControllerProvider.notifier).run((repository) => repository.setAvailableToday(value));
                    ref.invalidate(providerDashboardProvider);
                  }
                : null,
          ),
          const SizedBox(height: VitrioSpace.sp14),
          Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Expanded(child: _Metric(value: '0', label: l10n.providerMetricNewOrders, highlight: true)),
              const SizedBox(width: VitrioSpace.sp10),
              Expanded(child: _Metric(value: '–', label: l10n.providerMetricRating)),
              const SizedBox(width: VitrioSpace.sp10),
              Expanded(child: _Metric(value: '0', label: l10n.providerMetricServicesMonth)),
            ],
          ),
        ],
      ),
    );
  }
}

class _Metric extends StatelessWidget {
  const _Metric({required this.value, required this.label, this.highlight = false});

  final String value;
  final String label;
  final bool highlight;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    return MergeSemantics(
      child: Container(
        padding: const EdgeInsets.all(VitrioSpace.sp12),
        decoration: BoxDecoration(color: c.heroGlass, borderRadius: const BorderRadius.all(VitrioRadius.xl)),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(value, style: VitrioText.title.copyWith(color: highlight ? c.lime : c.onHero)),
            const SizedBox(height: 2),
            Text(label, style: VitrioText.weighted(VitrioText.caption, 600).copyWith(color: c.onHeroMuted)),
          ],
        ),
      ),
    );
  }
}

class _ProfileCard extends ConsumerWidget {
  const _ProfileCard({required this.profile, required this.dashboard});

  final ProviderProfile profile;
  final ProviderDashboard dashboard;

  void _open(BuildContext context, WidgetRef ref) => switch (dashboard.nextStep) {
        'about' => const ProviderEditRoute(step: ProviderStep.about).push<void>(context),
        'categories' || 'serviceArea' || 'contactPhone' => const ProviderEditRoute(step: ProviderStep.showcase).push<void>(context),
        'services' => const ProviderEditRoute(step: ProviderStep.photos).push<void>(context),
        'publish' => const ProviderEditRoute(step: ProviderStep.review).push<void>(context),
        'availability' => const ProviderAvailabilityRoute().push<void>(context),
        _ => shareProviderProfile(context, ref, profile.id),
      };

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final percent = dashboard.profileCompleteness;
    final (String hint, String action) = switch (dashboard.nextStep) {
      'about' => (l10n.providerHintAbout, l10n.providerCompleteProfile),
      'categories' => (l10n.providerHintCategories, l10n.providerCompleteProfile),
      'serviceArea' => (l10n.providerHintServiceArea, l10n.providerCompleteProfile),
      'contactPhone' => (l10n.providerHintContactPhone, l10n.providerCompleteProfile),
      'services' => (l10n.providerHintServices, l10n.providerCompleteProfile),
      'publish' when dashboard.status == ProviderStatus.paused => (l10n.providerPausedHint, l10n.providerResume),
      'publish' => (l10n.providerHintPublish, l10n.providerStepReview),
      'availability' => (l10n.providerHintAvailability, l10n.providerCompleteProfile),
      _ => (l10n.providerHintShare, l10n.providerShare),
    };
    return Container(
      padding: const EdgeInsets.all(VitrioSpace.sp16),
      decoration: BoxDecoration(
        color: c.surfaceRaised,
        borderRadius: const BorderRadius.all(VitrioRadius.card),
        border: Border.all(color: c.lineSoft),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Expanded(
                child: Text(
                  percent >= 100 ? l10n.providerProfileComplete : l10n.providerProfileAlmostReady,
                  style: VitrioText.bodyStrong.copyWith(color: c.ink),
                ),
              ),
              const SizedBox(width: VitrioSpace.sp10),
              Text(l10n.providerPercent(percent), style: VitrioText.bodyStrong.copyWith(color: c.plum)),
            ],
          ),
          const SizedBox(height: VitrioSpace.sp10),
          Semantics(
            label: l10n.providerCompleteness(percent),
            child: ClipRRect(
              borderRadius: const BorderRadius.all(VitrioRadius.full),
              child: LinearProgressIndicator(value: percent / 100, minHeight: 8, color: c.plum, backgroundColor: c.lineSoft),
            ),
          ),
          const SizedBox(height: VitrioSpace.sp10),
          Text(hint, style: VitrioText.weighted(VitrioText.meta, 400).copyWith(color: c.inkMuted)),
          const SizedBox(height: VitrioSpace.sp4),
          InkWell(
            onTap: () => _open(context, ref),
            child: ConstrainedBox(
              constraints: const BoxConstraints(minHeight: VitrioSpace.tap),
              child: Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Text(action, style: VitrioText.label.copyWith(color: c.plum)),
                  const SizedBox(width: VitrioSpace.sp6),
                  VitrioIcon(VitrioIconName.arrowRight, size: 16, color: c.plum),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}
