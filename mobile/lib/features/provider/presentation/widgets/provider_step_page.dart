import 'package:flutter/material.dart';

import '../../../../app/routes.dart';
import '../../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../../core/design_system/tokens/vitrio_space.dart';
import '../../../../core/design_system/tokens/vitrio_text.dart';
import '../../../../core/design_system/widgets/vitrio_button.dart';
import '../../../../core/design_system/widgets/vitrio_button_size.dart';
import '../../../../core/design_system/widgets/vitrio_button_variant.dart';
import '../../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../../core/presentation/form_page.dart';
import '../../../../l10n/app_localizations.dart';
import '../provider_step.dart';
import 'step_progress.dart';

class ProviderStepPage extends StatelessWidget {
  const ProviderStepPage({
    super.key,
    required this.step,
    required this.editing,
    required this.children,
    required this.onAction,
    this.busy = false,
    this.message,
  });

  final ProviderStep step;
  final bool editing;
  final List<Widget> children;
  final VoidCallback? onAction;
  final bool busy;
  final String? message;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final number = step.index + 1;
    final total = ProviderStep.values.length;
    final last = step == ProviderStep.review;
    final (String title, String subtitle, String? next) = switch (step) {
      ProviderStep.about => (l10n.providerStepAbout, l10n.providerAboutSubtitle, l10n.providerNextAfterAbout),
      ProviderStep.showcase => (l10n.providerStepShowcase, l10n.providerShowcaseSubtitle, l10n.providerNextAfterShowcase),
      ProviderStep.photos => (l10n.providerStepPhotos, l10n.providerPhotosSubtitle, l10n.providerNextAfterPhotos),
      ProviderStep.review => (l10n.providerStepReview, l10n.providerReviewSubtitle, null),
    };
    return FormPage(
      title: title,
      topBarCenter: editing
          ? null
          : Text(
              l10n.providerStepOf(number, total),
              textAlign: TextAlign.center,
              style: VitrioText.label.copyWith(color: c.ink2),
            ),
      topBarTrailing: editing || step == ProviderStep.about
          ? null
          : VitrioButton(
              label: l10n.providerSkip,
              variant: VitrioButtonVariant.ghost,
              size: VitrioButtonSize.md,
              onPressed: () => const ProviderDashboardRoute().go(context),
            ),
      onBack: editing || step == ProviderStep.about
          ? null
          : () => ProviderOnboardingRoute(step: ProviderStep.values[step.index - 1]).go(context),
      header: editing
          ? null
          : Padding(
              padding: const EdgeInsets.only(bottom: VitrioSpace.sp20),
              child: StepProgress(step: number, total: total),
            ),
      heading: editing ? null : title,
      subtitle: editing ? null : subtitle,
      note: editing ? null : next,
      actionLabel: last ? l10n.providerPublish : editing ? l10n.commonSave : l10n.providerContinue,
      actionIcon: editing || last ? null : VitrioIconName.arrowRight,
      busy: busy,
      onAction: onAction,
      message: message,
      children: children,
    );
  }
}
