import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/design_system/tokens/vitrio_space.dart';
import '../../../../core/design_system/widgets/vitrio_avatar.dart';
import '../../../../core/network/api_result.dart';
import '../../../../core/network/problem_details.dart';
import '../../../../core/presentation/form_page.dart';
import '../../../../core/presentation/problem_message.dart';
import '../../../../l10n/app_localizations.dart';
import '../../../auth/presentation/session_controller.dart';
import '../../data/provider_providers.dart';
import '../../domain/picked_photo.dart';
import '../../domain/profile_changes.dart';
import '../photo_outcome.dart';
import '../provider_controller.dart';
import '../widgets/step_progress.dart';

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
    if (ref.read(providerControllerProvider).value == null &&
        !_accept(await controller.create(displayName: _name.text.trim(), headline: _headline.text.trim()))) {
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
    return FormPage(
      title: l10n.providerStepAbout,
      actionLabel: widget.editing ? l10n.commonSave : l10n.providerContinue,
      busy: _busy,
      onAction: _save,
      message: _problem == null || _problem!.isValidation ? null : problemMessage(l10n, _problem!),
      children: [
        if (!widget.editing) const StepProgress(step: 1, total: 4),
        Row(
          children: [
            VitrioAvatar(name: _name.text, imageUrl: profile?.avatarUrl, imageBytes: _avatar?.bytes, size: 72),
            const SizedBox(width: VitrioSpace.sp16),
            Flexible(
              child: TextButton(
                onPressed: _busy ? null : _pickAvatar,
                child: Text(profile?.avatarUrl == null && _avatar == null ? l10n.providerAvatarAdd : l10n.providerAvatarChange),
              ),
            ),
          ],
        ),
        const SizedBox(height: VitrioSpace.sp20),
        TextField(
          controller: _name,
          textCapitalization: TextCapitalization.words,
          decoration: InputDecoration(labelText: l10n.providerDisplayName, errorText: fieldError(_problem, 'displayName')),
        ),
        const SizedBox(height: VitrioSpace.sp16),
        TextField(
          controller: _headline,
          textCapitalization: TextCapitalization.sentences,
          maxLength: 60,
          decoration: InputDecoration(
            labelText: l10n.providerHeadline,
            hintText: l10n.providerHeadlineHint,
            errorText: fieldError(_problem, 'headline'),
          ),
        ),
        const SizedBox(height: VitrioSpace.sp8),
        TextField(
          controller: _about,
          minLines: 4,
          maxLines: 8,
          maxLength: 600,
          textCapitalization: TextCapitalization.sentences,
          decoration: InputDecoration(
            labelText: l10n.providerAbout,
            hintText: l10n.providerAboutHint,
            alignLabelWithHint: true,
            errorText: fieldError(_problem, 'about'),
          ),
        ),
        const SizedBox(height: VitrioSpace.sp8),
        TextField(
          controller: _years,
          keyboardType: TextInputType.number,
          decoration: InputDecoration(labelText: l10n.providerYears, errorText: fieldError(_problem, 'yearsOfExperience')),
        ),
      ],
    );
  }
}
