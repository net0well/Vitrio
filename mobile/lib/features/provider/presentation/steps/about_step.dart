import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/design_system/tokens/vitrio_space.dart';
import '../../../../core/design_system/widgets/vitrio_avatar.dart';
import '../../../../core/design_system/widgets/vitrio_button.dart';
import '../../../../core/design_system/widgets/vitrio_button_size.dart';
import '../../../../core/design_system/widgets/vitrio_button_variant.dart';
import '../../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../../core/design_system/widgets/vitrio_text_field.dart';
import '../../../../core/network/api_result.dart';
import '../../../../core/network/problem_details.dart';
import '../../../../core/presentation/problem_message.dart';
import '../../../../l10n/app_localizations.dart';
import '../../../auth/presentation/session_controller.dart';
import '../../data/provider_providers.dart';
import '../../domain/picked_photo.dart';
import '../../domain/profile_changes.dart';
import '../photo_outcome.dart';
import '../provider_controller.dart';
import '../provider_step.dart';
import '../widgets/provider_step_page.dart';

class AboutStep extends ConsumerStatefulWidget {
  const AboutStep({super.key, required this.editing, required this.onDone});

  final bool editing;
  final VoidCallback onDone;

  @override
  ConsumerState<AboutStep> createState() => _AboutStepState();
}

class _AboutStepState extends ConsumerState<AboutStep> {
  late final TextEditingController _name;
  late final TextEditingController _headline;
  late final TextEditingController _about;
  late final TextEditingController _years;
  PickedPhoto? _avatar;
  bool _busy = false;
  ProblemDetails? _problem;

  @override
  void initState() {
    super.initState();
    final profile = ref.read(providerControllerProvider).value;
    _name = TextEditingController(text: profile?.displayName ?? ref.read(sessionControllerProvider).value?.displayName);
    _headline = TextEditingController(text: profile?.headline);
    _about = TextEditingController(text: profile?.about);
    _years = TextEditingController(text: profile?.yearsOfExperience?.toString());
  }

  @override
  void dispose() {
    _name.dispose();
    _headline.dispose();
    _about.dispose();
    _years.dispose();
    super.dispose();
  }

  Future<void> _pickAvatar() async {
    final photo = await ref.read(photoPickerProvider).pick();
    if (photo != null && mounted) {
      setState(() => _avatar = photo);
    }
  }

  Future<void> _save() async {
    setState(() {
      _busy = true;
      _problem = null;
    });

    final controller = ref.read(providerControllerProvider.notifier);
    final hasShowcase = ref.read(sessionControllerProvider).value?.hasProviderProfile ?? false;
    if (!hasShowcase && !_accept(await controller.create(displayName: _name.text.trim(), headline: _headline.text.trim()))) {
      return;
    }

    final about = _about.text.trim();
    final changes = ProfileChanges(
      displayName: _name.text.trim(),
      headline: _headline.text.trim(),
      about: about.isEmpty ? null : about,
      yearsOfExperience: int.tryParse(_years.text.trim()),
    );
    if (!_accept(await controller.run((repository) => repository.update(changes)))) {
      return;
    }

    if (_avatar != null && await controller.uploadPhoto(_avatar!, avatar: true) == PhotoOutcome.failed && mounted) {
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(AppLocalizations.of(context).providerPhotoUploadFailed)));
    }

    if (mounted) {
      setState(() => _busy = false);
      widget.onDone();
    }
  }

  bool _accept(ApiResult<void> result) {
    if (result case ApiFailure(:final problem)) {
      if (mounted) {
        setState(() {
          _busy = false;
          _problem = problem;
        });
      }

      return false;
    }

    return true;
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final profile = ref.watch(providerControllerProvider).value;
    return ProviderStepPage(
      step: ProviderStep.about,
      editing: widget.editing,
      busy: _busy,
      onAction: _save,
      message: _problem == null || _problem!.isValidation ? null : problemMessage(l10n, _problem!),
      children: [
        Row(
          children: [
            VitrioAvatar(name: _name.text, imageUrl: profile?.avatarUrl, imageBytes: _avatar?.bytes, seed: profile?.id, size: 72),
            const SizedBox(width: VitrioSpace.sp16),
            Flexible(
              child: VitrioButton(
                label: profile?.avatarUrl == null && _avatar == null ? l10n.providerAvatarAdd : l10n.providerAvatarChange,
                variant: VitrioButtonVariant.secondary,
                size: VitrioButtonSize.md,
                leadingIcon: VitrioIconName.camera,
                onPressed: _busy ? null : _pickAvatar,
              ),
            ),
          ],
        ),
        const SizedBox(height: VitrioSpace.sp24),
        VitrioTextField(
          label: l10n.providerDisplayName,
          controller: _name,
          textCapitalization: TextCapitalization.words,
          error: fieldError(_problem, 'displayName'),
        ),
        const SizedBox(height: VitrioSpace.sp16),
        VitrioTextField(
          label: l10n.providerHeadline,
          controller: _headline,
          textCapitalization: TextCapitalization.sentences,
          maxLength: 60,
          placeholder: l10n.providerHeadlineHint,
          error: fieldError(_problem, 'headline'),
        ),
        const SizedBox(height: VitrioSpace.sp16),
        VitrioTextField(
          label: l10n.providerAbout,
          controller: _about,
          minLines: 4,
          maxLines: 8,
          maxLength: 600,
          textCapitalization: TextCapitalization.sentences,
          placeholder: l10n.providerAboutHint,
          error: fieldError(_problem, 'about'),
        ),
        const SizedBox(height: VitrioSpace.sp16),
        VitrioTextField(
          label: l10n.providerYears,
          controller: _years,
          keyboardType: TextInputType.number,
          error: fieldError(_problem, 'yearsOfExperience'),
        ),
      ],
    );
  }
}
