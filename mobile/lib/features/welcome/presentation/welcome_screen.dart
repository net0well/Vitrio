import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/routes.dart';
import '../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/design_system/widgets/vitrio_button.dart';
import '../../../core/design_system/widgets/vitrio_button_variant.dart';
import '../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../core/design_system/widgets/vitrio_wordmark.dart';
import '../../../l10n/app_localizations.dart';
import 'welcome_controller.dart';

class WelcomeScreen extends ConsumerWidget {
  const WelcomeScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final c = context.vitrio;
    final l10n = AppLocalizations.of(context);

    return AnnotatedRegion<SystemUiOverlayStyle>(
      value: SystemUiOverlayStyle.light,
      child: Scaffold(
        backgroundColor: c.hero,
        body: SafeArea(
          child: Padding(
            padding: const EdgeInsets.fromLTRB(
              VitrioSpace.sp20,
              VitrioSpace.sp24,
              VitrioSpace.sp20,
              VitrioSpace.sp32,
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                const Align(
                  alignment: AlignmentDirectional.centerStart,
                  child: VitrioWordmark(onDark: true),
                ),
                const Spacer(),
                Text(l10n.welcomeTitle, style: VitrioText.displayXl.copyWith(color: c.onHero)),
                const SizedBox(height: VitrioSpace.sp12),
                Text(l10n.welcomeMessage, style: VitrioText.body.copyWith(color: c.onHeroMuted)),
                const SizedBox(height: VitrioSpace.sp32),
                VitrioButton(
                  label: l10n.welcomeFindProfessional,
                  trailingIcon: VitrioIconName.arrowRight,
                  onPressed: () async {
                    await ref.read(welcomeControllerProvider.notifier).complete();
                    if (context.mounted) {
                      const HomeRoute().go(context);
                    }
                  },
                ),
                const SizedBox(height: VitrioSpace.sp12),
                VitrioButton(
                  label: l10n.welcomeOfferServices,
                  variant: VitrioButtonVariant.onHero,
                  onPressed: () async {
                    await ref.read(welcomeControllerProvider.notifier).complete();
                    if (context.mounted) {
                      const AccountRoute().go(context);
                    }
                  },
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
