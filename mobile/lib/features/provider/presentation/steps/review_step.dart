import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../../core/design_system/tokens/vitrio_radius.dart';
import '../../../../core/design_system/tokens/vitrio_space.dart';
import '../../../../core/design_system/tokens/vitrio_text.dart';
import '../../../../core/design_system/widgets/vitrio_badge.dart';
import '../../../../core/design_system/widgets/vitrio_badge_tone.dart';
import '../../../../core/design_system/widgets/vitrio_icon.dart';
import '../../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../../core/design_system/widgets/vitrio_notice.dart';
import '../../../../core/design_system/widgets/vitrio_provider_card.dart';
import '../../../../core/design_system/widgets/vitrio_section_header.dart';
import '../../../../core/network/api_result.dart';
import '../../../../core/network/problem_details.dart';
import '../../../../core/presentation/problem_message.dart';
import '../../../../l10n/app_localizations.dart';
import '../../domain/app_mode.dart';
import '../../domain/mei_status.dart';
import '../app_mode_controller.dart';
import '../provider_controller.dart';
import '../provider_labels.dart';
import '../provider_step.dart';
import '../widgets/provider_step_page.dart';

class ReviewStep extends ConsumerStatefulWidget {
  const ReviewStep({super.key, required this.editing, required this.onDone});

  final bool editing;
  final VoidCallback onDone;

  @override
  ConsumerState<ReviewStep> createState() => _ReviewStepState();
}

class _ReviewStepState extends ConsumerState<ReviewStep> {
  bool _busy = false;
  ProblemDetails? _problem;

  Future<void> _publish() async {
    setState(() {
      _busy = true;
      _problem = null;
    });
    final result = await ref.read(providerControllerProvider.notifier).run((repository) => repository.publish());
    if (!mounted) {
      return;
    }

    switch (result) {
      case ApiSuccess():
        await ref.read(appModeControllerProvider.notifier).change(AppMode.provider);
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(AppLocalizations.of(context).providerPublished)));
          widget.onDone();
        }
      case ApiFailure(:final problem):
        setState(() {
          _busy = false;
          _problem = problem;
        });
    }
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final profile = ref.watch(providerControllerProvider).value;
    final missing = profile?.missing ?? const <String>[];
    final firstPrice = profile == null || profile.services.isEmpty ? null : priceValue(l10n, profile.services.first);
    final area = profile?.serviceArea;
    return ProviderStepPage(
      step: ProviderStep.review,
      editing: widget.editing,
      busy: _busy,
      onAction: profile == null || missing.isNotEmpty ? null : _publish,
      message: _problem == null ? null : problemMessage(l10n, _problem!),
      children: [
        if (profile != null) ...[
          VitrioSectionHeader(l10n.providerPreviewTitle),
          const SizedBox(height: VitrioSpace.sp12),
          VitrioProviderCard(
            name: profile.displayName,
            profession: profile.headline,
            seed: profile.id,
            avatarUrl: profile.avatarUrl,
            verified: profile.mei?.status == MeiStatus.verified,
            meta: area?.city.name,
            badges: [
              VitrioBadge(label: l10n.providerNew, tone: VitrioBadgeTone.warning),
              if (profile.mei?.status == MeiStatus.verified) VitrioBadge(label: l10n.providerBadgeMei, tone: VitrioBadgeTone.verified),
            ],
            priceLabel: firstPrice == null ? l10n.providerPriceLabel : l10n.providerFromCapital,
            priceValue: firstPrice ?? l10n.providerServiceOnQuote,
            actionLabel: l10n.providerSeeProfile,
          ),
          const SizedBox(height: VitrioSpace.sp24),
        ],
        if (missing.isEmpty)
          VitrioNotice(message: l10n.providerReviewReady, tone: VitrioBadgeTone.verified)
        else
          Container(
            padding: const EdgeInsets.all(VitrioSpace.sp16),
            decoration: BoxDecoration(color: c.amberSoft, borderRadius: const BorderRadius.all(VitrioRadius.card)),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    VitrioIcon(VitrioIconName.info, size: 18, color: c.amberInk),
                    const SizedBox(width: VitrioSpace.sp8),
                    Text(l10n.providerReviewMissing, style: VitrioText.bodyStrong.copyWith(color: c.amberInk)),
                  ],
                ),
                const SizedBox(height: VitrioSpace.sp8),
                for (final code in missing)
                  Padding(
                    padding: const EdgeInsets.only(top: VitrioSpace.sp4),
                    child: Text(
                      '• ${missingLabel(l10n, code)}',
                      style: VitrioText.weighted(VitrioText.meta, 700).copyWith(color: c.amberInk),
                    ),
                  ),
              ],
            ),
          ),
      ],
    );
  }
}
