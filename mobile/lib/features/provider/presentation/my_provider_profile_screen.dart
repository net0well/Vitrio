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
import '../../../core/design_system/widgets/vitrio_button.dart';
import '../../../core/design_system/widgets/vitrio_button_size.dart';
import '../../../core/design_system/widgets/vitrio_button_variant.dart';
import '../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../core/design_system/widgets/vitrio_list_card.dart';
import '../../../core/design_system/widgets/vitrio_list_row.dart';
import '../../../core/design_system/widgets/vitrio_notice.dart';
import '../../../core/design_system/widgets/vitrio_section_header.dart';
import '../../../core/design_system/widgets/vitrio_top_bar.dart';
import '../../../l10n/app_localizations.dart';
import '../domain/provider_status.dart';
import 'provider_controller.dart';
import 'provider_labels.dart';
import 'provider_share.dart';
import 'provider_step.dart';

class MyProviderProfileScreen extends ConsumerWidget {
  const MyProviderProfileScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final profile = ref.watch(providerControllerProvider).value;
    if (profile == null) {
      return Scaffold(appBar: VitrioTopBar(title: l10n.tabMyProfile), body: const Center(child: CircularProgressIndicator()));
    }

    final published = profile.status == ProviderStatus.published;
    final tone = switch (profile.status) {
      ProviderStatus.published => VitrioBadgeTone.verified,
      ProviderStatus.paused => VitrioBadgeTone.warning,
      ProviderStatus.suspended || ProviderStatus.removed => VitrioBadgeTone.danger,
      ProviderStatus.draft => VitrioBadgeTone.neutral,
    };
    return Scaffold(
      appBar: VitrioTopBar(title: l10n.tabMyProfile),
      body: ListView(
        padding: const EdgeInsets.fromLTRB(VitrioSpace.sp20, VitrioSpace.sp16, VitrioSpace.sp20, VitrioSpace.sp24),
        children: [
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
                Row(
                  children: [
                    VitrioAvatar(name: profile.displayName, imageUrl: profile.avatarUrl, seed: profile.id, size: 60),
                    const SizedBox(width: VitrioSpace.sp14),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(profile.displayName, style: VitrioText.labelLg.copyWith(color: c.ink)),
                          const SizedBox(height: 2),
                          Text(profile.headline, style: VitrioText.meta.copyWith(color: c.inkMuted)),
                          const SizedBox(height: VitrioSpace.sp6),
                          VitrioBadge(label: statusLabel(l10n, profile.status), tone: tone),
                        ],
                      ),
                    ),
                  ],
                ),
                if (published) ...[
                  const SizedBox(height: VitrioSpace.sp14),
                  Row(
                    children: [
                      Expanded(
                        child: VitrioButton(
                          label: l10n.providerViewPublic,
                          variant: VitrioButtonVariant.secondary,
                          size: VitrioButtonSize.md,
                          onPressed: () => PublicProviderRoute(id: profile.id).push<void>(context),
                        ),
                      ),
                      const SizedBox(width: VitrioSpace.sp10),
                      VitrioButton(
                        label: l10n.providerShareShort,
                        variant: VitrioButtonVariant.secondary,
                        size: VitrioButtonSize.md,
                        leadingIcon: VitrioIconName.share,
                        onPressed: () => shareProviderProfile(context, ref, profile.id),
                      ),
                    ],
                  ),
                ],
              ],
            ),
          ),
          if (profile.status == ProviderStatus.paused) ...[
            const SizedBox(height: VitrioSpace.sp12),
            VitrioNotice(message: l10n.providerPausedHint, tone: VitrioBadgeTone.warning),
          ],
          const SizedBox(height: VitrioSpace.sp24),
          VitrioSectionHeader(l10n.providerYourShowcase),
          const SizedBox(height: VitrioSpace.sp12),
          VitrioListCard(
            children: [
              VitrioListRow(
                icon: VitrioIconName.user,
                title: l10n.providerStepAbout,
                onTap: () => const ProviderEditRoute(step: ProviderStep.about).push<void>(context),
              ),
              VitrioListRow(
                icon: VitrioIconName.pin,
                title: l10n.providerStepShowcase,
                onTap: () => const ProviderEditRoute(step: ProviderStep.showcase).push<void>(context),
              ),
              VitrioListRow(
                icon: VitrioIconName.image,
                title: l10n.providerStepPhotos,
                onTap: () => const ProviderEditRoute(step: ProviderStep.photos).push<void>(context),
              ),
              VitrioListRow(
                icon: VitrioIconName.clock,
                title: l10n.providerAvailability,
                onTap: () => const ProviderAvailabilityRoute().push<void>(context),
              ),
            ],
          ),
          const SizedBox(height: VitrioSpace.sp12),
          VitrioListCard(
            children: [
              if (published)
                VitrioListRow(
                  icon: VitrioIconName.x,
                  title: l10n.providerPause,
                  onTap: () => ref.read(providerControllerProvider.notifier).run((repository) => repository.pause()),
                )
              else if (profile.status == ProviderStatus.paused)
                VitrioListRow(
                  icon: VitrioIconName.check,
                  title: l10n.providerResume,
                  onTap: () => ref.read(providerControllerProvider.notifier).run((repository) => repository.publish()),
                )
              else
                VitrioListRow(
                  icon: VitrioIconName.check,
                  title: l10n.providerStepReview,
                  onTap: () => const ProviderEditRoute(step: ProviderStep.review).push<void>(context),
                ),
              VitrioListRow(
                icon: VitrioIconName.sliders,
                title: l10n.accountTitle,
                onTap: () => const ProviderAccountRoute().push<void>(context),
              ),
            ],
          ),
        ],
      ),
    );
  }
}
