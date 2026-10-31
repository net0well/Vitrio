import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:share_plus/share_plus.dart';

import '../../../../app/app_config.dart';
import '../../../../app/routes.dart';
import '../../../../core/design_system/tokens/vitrio_space.dart';
import '../../../../core/design_system/widgets/vitrio_button.dart';
import '../../../../core/design_system/widgets/vitrio_button_size.dart';
import '../../../../core/design_system/widgets/vitrio_button_variant.dart';
import '../../../../core/design_system/widgets/vitrio_empty_state.dart';
import '../../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../../l10n/app_localizations.dart';
import '../../../auth/domain/auth_intent.dart';
import '../../../auth/presentation/pending_auth_intent.dart';
import '../../../auth/presentation/session_controller.dart';
import '../../../location/domain/city_summary.dart';
import '../../../provider/presentation/provider_step.dart';
import '../search_labels.dart';

class EmptyCityInvite extends ConsumerWidget {
  const EmptyCityInvite({super.key, this.city});

  final CitySummary? city;

  void _offerServices(BuildContext context, WidgetRef ref) {
    final me = ref.read(sessionControllerProvider).value;
    if (me == null) {
      ref.read(pendingAuthIntentProvider.notifier).remember(AuthIntent.offerServices);
      const SignUpRoute().push<void>(context);
    } else if (me.hasProviderProfile) {
      const ProviderDashboardRoute().go(context);
    } else {
      const ProviderOnboardingRoute(step: ProviderStep.about).push<void>(context);
    }
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: VitrioSpace.sp24),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          VitrioEmptyState(
            icon: VitrioIconName.pin,
            title: city == null ? l10n.emptyCityTitleNearby : l10n.emptyCityTitle(cityLabel(city!)),
            message: l10n.emptyCityText,
          ),
          const SizedBox(height: VitrioSpace.sp20),
          VitrioButton(
            label: l10n.emptyCityInvite,
            leadingIcon: VitrioIconName.share,
            size: VitrioButtonSize.md,
            onPressed: () => SharePlus.instance.share(
              ShareParams(
                text: l10n.emptyCityShareText(AppConfig.playStoreUrl),
              ),
            ),
          ),
          const SizedBox(height: VitrioSpace.sp8),
          VitrioButton(
            label: l10n.providerOfferServices,
            variant: VitrioButtonVariant.ghost,
            size: VitrioButtonSize.md,
            onPressed: () => _offerServices(context, ref),
          ),
        ],
      ),
    );
  }
}
