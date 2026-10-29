import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/routes.dart';
import '../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../core/design_system/tokens/vitrio_radius.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/design_system/widgets/vitrio_avatar.dart';
import '../../../core/design_system/widgets/vitrio_badge.dart';
import '../../../core/design_system/widgets/vitrio_badge_tone.dart';
import '../../../core/design_system/widgets/vitrio_icon.dart';
import '../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../l10n/app_localizations.dart';
import '../domain/mei_status.dart';
import '../domain/provider_dashboard.dart';
import '../domain/provider_profile.dart';
import '../domain/provider_status.dart';
import 'provider_controller.dart';
import 'provider_labels.dart';
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
    return Scaffold(
      appBar: AppBar(title: Text(l10n.tabDashboard)),
      body: switch ((profile, dashboard)) {
        (final ProviderProfile profile, AsyncData(value: final dashboard)) => RefreshIndicator(
            onRefresh: () => ref.refresh(providerDashboardProvider.future),
            child: _Dashboard(profile: profile, dashboard: dashboard),
          ),
        (_, AsyncError()) => Center(
            child: TextButton(onPressed: () => ref.invalidate(providerDashboardProvider), child: Text(l10n.commonRetry)),
          ),
        _ => const Center(child: CircularProgressIndicator()),
      },
    );
  }
}

class _Dashboard extends ConsumerWidget {
  const _Dashboard({required this.profile, required this.dashboard});

  final ProviderProfile profile;
  final ProviderDashboard dashboard;

  void _openNextStep(BuildContext context, WidgetRef ref) => switch (dashboard.nextStep) {
        'about' => const ProviderEditRoute(step: ProviderStep.about).push<void>(context),
        'categories' || 'serviceArea' || 'contactPhone' => const ProviderEditRoute(step: ProviderStep.showcase).push<void>(context),
        'services' => const ProviderEditRoute(step: ProviderStep.photos).push<void>(context),
        'publish' => const ProviderEditRoute(step: ProviderStep.review).push<void>(context),
        'availability' => const ProviderAvailabilityRoute().push<void>(context),
        _ => shareProviderProfile(context, ref, profile.id),
      };

  String _nextStepLabel(AppLocalizations l10n) => switch (dashboard.nextStep) {
        'publish' => l10n.providerNextPublish,
        'share' => l10n.providerNextShare,
        'availability' => l10n.providerNextAvailability,
        final code => missingLabel(l10n, code),
      };

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final published = dashboard.status == ProviderStatus.published;
    return ListView(
      padding: const EdgeInsets.all(VitrioSpace.sp20),
      children: [
        DecoratedBox(
          decoration: BoxDecoration(color: c.hero, borderRadius: const BorderRadius.all(VitrioRadius.hero)),
          child: Padding(
            padding: const EdgeInsets.all(VitrioSpace.sp20),
            child: Row(
              children: [
                VitrioAvatar(name: profile.displayName, imageUrl: profile.avatarUrl, size: 56),
                const SizedBox(width: VitrioSpace.sp16),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(profile.displayName, style: VitrioText.section.copyWith(color: c.onHero)),
                      const SizedBox(height: VitrioSpace.sp6),
                      VitrioBadge(
                        label: statusLabel(l10n, dashboard.status),
                        tone: published ? VitrioBadgeTone.accent : VitrioBadgeTone.neutral,
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
        ),
        if (published)
          SwitchListTile(
            contentPadding: EdgeInsets.zero,
            title: Text(l10n.providerAvailableToday),
            subtitle: Text(l10n.providerAvailableTodayHint),
            value: dashboard.availableToday,
            onChanged: (value) async {
              await ref.read(providerControllerProvider.notifier).run((repository) => repository.setAvailableToday(value));
              ref.invalidate(providerDashboardProvider);
            },
          ),
        const SizedBox(height: VitrioSpace.sp16),
        Text(l10n.providerCompleteness(dashboard.profileCompleteness), style: VitrioText.bodyStrong),
        const SizedBox(height: VitrioSpace.sp8),
        LinearProgressIndicator(value: dashboard.profileCompleteness / 100, minHeight: 6, borderRadius: BorderRadius.circular(3)),
        ListTile(
          contentPadding: EdgeInsets.zero,
          title: Text(l10n.providerNextStep, style: Theme.of(context).textTheme.bodySmall),
          subtitle: Text(_nextStepLabel(l10n), style: VitrioText.bodyStrong),
          trailing: const VitrioIcon(VitrioIconName.chevronRight),
          onTap: () => _openNextStep(context, ref),
        ),
        if (dashboard.meiStatus case final status?)
          ListTile(
            contentPadding: EdgeInsets.zero,
            leading: VitrioIcon(VitrioIconName.shieldCheck, color: status == MeiStatus.verified ? c.green : c.inkMuted),
            title: Text(switch (status) {
              MeiStatus.verified => l10n.providerMeiVerified,
              MeiStatus.pending => l10n.providerMeiPending,
              MeiStatus.lost => l10n.providerMeiLost,
              MeiStatus.failed => profile.mei?.reason ?? l10n.errorGeneric,
            }),
            onTap: () => const ProviderEditRoute(step: ProviderStep.showcase).push<void>(context),
          ),
        if (published) ...[
          const Divider(),
          ListTile(
            contentPadding: EdgeInsets.zero,
            leading: const VitrioIcon(VitrioIconName.user),
            title: Text(l10n.providerViewPublic),
            onTap: () => PublicProviderRoute(id: profile.id).push<void>(context),
          ),
          ListTile(
            contentPadding: EdgeInsets.zero,
            leading: const VitrioIcon(VitrioIconName.share),
            title: Text(l10n.providerShare),
            onTap: () => shareProviderProfile(context, ref, profile.id),
          ),
        ],
      ],
    );
  }
}
