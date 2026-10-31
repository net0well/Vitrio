import 'package:flutter/material.dart';

import '../../../../app/routes.dart';
import '../../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../../core/design_system/tokens/vitrio_radius.dart';
import '../../../../core/design_system/tokens/vitrio_space.dart';
import '../../../../core/design_system/tokens/vitrio_text.dart';
import '../../../../core/design_system/widgets/vitrio_button.dart';
import '../../../../core/design_system/widgets/vitrio_button_size.dart';
import '../../../../core/design_system/widgets/vitrio_button_variant.dart';
import '../../../../core/design_system/widgets/vitrio_icon.dart';
import '../../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../../l10n/app_localizations.dart';

class SignInInvite extends StatelessWidget {
  const SignInInvite({super.key});

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
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
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Container(
                width: 40,
                height: 40,
                decoration: BoxDecoration(
                  color: c.plumSoft,
                  borderRadius: const BorderRadius.all(VitrioRadius.md),
                ),
                alignment: Alignment.center,
                child: VitrioIcon(VitrioIconName.user, size: 20, color: c.plum),
              ),
              const SizedBox(width: VitrioSpace.sp12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(l10n.homeSignInTitle, style: VitrioText.bodyStrong.copyWith(color: c.ink)),
                    const SizedBox(height: VitrioSpace.sp4),
                    Text(l10n.homeSignInText, style: VitrioText.meta.copyWith(color: c.inkMuted)),
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: VitrioSpace.sp14),
          Row(
            children: [
              Expanded(
                child: VitrioButton(
                  label: l10n.authSignIn,
                  size: VitrioButtonSize.md,
                  onPressed: () => const SignInRoute().push<void>(context),
                ),
              ),
              const SizedBox(width: VitrioSpace.sp10),
              Expanded(
                child: VitrioButton(
                  label: l10n.authCreateAccount,
                  variant: VitrioButtonVariant.secondary,
                  size: VitrioButtonSize.md,
                  onPressed: () => const SignUpRoute().push<void>(context),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}
