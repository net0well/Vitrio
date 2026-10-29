import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/design_system/tokens/vitrio_space.dart';
import '../../../../core/design_system/tokens/vitrio_text.dart';
import '../../../../core/design_system/widgets/vitrio_avatar.dart';
import '../../../../core/network/api_result.dart';
import '../../../../core/network/problem_details.dart';
import '../../../../core/presentation/form_page.dart';
import '../../../../core/presentation/problem_message.dart';
import '../../../../l10n/app_localizations.dart';
import '../../domain/app_mode.dart';
import '../app_mode_controller.dart';
import '../provider_controller.dart';
import '../provider_labels.dart';
import '../widgets/step_progress.dart';

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
    final profile = ref.watch(providerControllerProvider).value;
    final missing = profile?.missing ?? const <String>[];
    final theme = Theme.of(context);
    return FormPage(
      title: l10n.providerStepReview,
      actionLabel: l10n.providerPublish,
      busy: _busy,
      onAction: profile == null || missing.isNotEmpty ? null : _publish,
      message: _problem == null ? null : problemMessage(l10n, _problem!),
      children: [
        if (!widget.editing) const StepProgress(step: 4, total: 4),
        if (profile != null)
          Card(
            child: Padding(
              padding: const EdgeInsets.all(VitrioSpace.sp16),
              child: Row(
                children: [
                  VitrioAvatar(name: profile.displayName, imageUrl: profile.avatarUrl, size: 60),
                  const SizedBox(width: VitrioSpace.sp16),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(profile.displayName, style: VitrioText.bodyStrong),
                        Text(profile.headline, style: theme.textTheme.bodyMedium),
                        if (profile.serviceArea case final area?)
                          Text(l10n.providerServes('${area.city.name}, ${area.city.state}', area.radiusKm), style: theme.textTheme.bodySmall),
                        if (profile.services.isNotEmpty)
                          Text(priceText(l10n, profile.services.first), style: theme.textTheme.bodySmall),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ),
        const SizedBox(height: VitrioSpace.sp20),
        if (missing.isEmpty)
          Text(l10n.providerReviewReady, style: theme.textTheme.titleMedium)
        else ...[
          Text(l10n.providerReviewMissing, style: theme.textTheme.titleMedium),
          const SizedBox(height: VitrioSpace.sp8),
          for (final code in missing) Text('• ${missingLabel(l10n, code)}'),
        ],
      ],
    );
  }
}
