import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/routes.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/design_system/widgets/vitrio_avatar.dart';
import '../../../core/design_system/widgets/vitrio_icon.dart';
import '../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../l10n/app_localizations.dart';
import '../domain/provider_status.dart';
import 'provider_controller.dart';
import 'provider_labels.dart';
import 'provider_step.dart';

class MyProviderProfileScreen extends ConsumerWidget {
  const MyProviderProfileScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final profile = ref.watch(providerControllerProvider).value;
    if (profile == null) {
      return Scaffold(appBar: AppBar(title: Text(l10n.tabMyProfile)), body: const Center(child: CircularProgressIndicator()));
    }

    ListTile section(VitrioIconName icon, String title, VoidCallback onTap) => ListTile(
          leading: VitrioIcon(icon),
          title: Text(title),
          trailing: const VitrioIcon(VitrioIconName.chevronRight),
          onTap: onTap,
        );

    return Scaffold(
      appBar: AppBar(title: Text(l10n.tabMyProfile)),
      body: ListView(
        padding: const EdgeInsets.symmetric(vertical: VitrioSpace.sp16),
        children: [
          ListTile(
            leading: VitrioAvatar(name: profile.displayName, imageUrl: profile.avatarUrl),
            title: Text(profile.displayName, style: VitrioText.bodyStrong),
            subtitle: Text('${profile.headline} · ${statusLabel(l10n, profile.status)}'),
          ),
          if (profile.status == ProviderStatus.paused)
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp16),
              child: Text(l10n.providerPausedHint, style: Theme.of(context).textTheme.bodySmall),
            ),
          const Divider(),
          section(VitrioIconName.user, l10n.providerStepAbout, () => const ProviderEditRoute(step: ProviderStep.about).push<void>(context)),
          section(VitrioIconName.pin, l10n.providerStepShowcase, () => const ProviderEditRoute(step: ProviderStep.showcase).push<void>(context)),
          section(VitrioIconName.image, l10n.providerStepPhotos, () => const ProviderEditRoute(step: ProviderStep.photos).push<void>(context)),
          section(VitrioIconName.clock, l10n.providerAvailability, () => const ProviderAvailabilityRoute().push<void>(context)),
          const Divider(),
          if (profile.status == ProviderStatus.published) ...[
            section(VitrioIconName.search, l10n.providerViewPublic, () => PublicProviderRoute(id: profile.id).push<void>(context)),
            ListTile(
              leading: const VitrioIcon(VitrioIconName.x),
              title: Text(l10n.providerPause),
              onTap: () => ref.read(providerControllerProvider.notifier).run((repository) => repository.pause()),
            ),
          ] else if (profile.status == ProviderStatus.paused)
            ListTile(
              leading: const VitrioIcon(VitrioIconName.check),
              title: Text(l10n.providerResume),
              onTap: () => ref.read(providerControllerProvider.notifier).run((repository) => repository.publish()),
            )
          else
            section(VitrioIconName.check, l10n.providerStepReview, () => const ProviderEditRoute(step: ProviderStep.review).push<void>(context)),
        ],
      ),
    );
  }
}
