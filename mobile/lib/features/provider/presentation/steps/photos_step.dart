import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../../core/design_system/tokens/vitrio_radius.dart';
import '../../../../core/design_system/tokens/vitrio_space.dart';
import '../../../../core/design_system/widgets/vitrio_badge.dart';
import '../../../../core/design_system/widgets/vitrio_badge_tone.dart';
import '../../../../core/design_system/widgets/vitrio_icon.dart';
import '../../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../../core/network/api_result.dart';
import '../../../../core/network/problem_details.dart';
import '../../../../core/presentation/form_page.dart';
import '../../../../core/presentation/problem_message.dart';
import '../../../../l10n/app_localizations.dart';
import '../../domain/photo_status.dart';
import '../../domain/provider_photo.dart';
import '../../domain/provider_service.dart';
import '../photo_outcome.dart';
import '../provider_controller.dart';
import '../provider_labels.dart';
import '../provider_queries.dart';
import '../widgets/section_title.dart';
import '../widgets/service_editor_sheet.dart';
import '../widgets/step_progress.dart';

class PhotosStep extends ConsumerStatefulWidget {
  const PhotosStep({super.key, required this.editing, required this.onDone});

  final bool editing;
  final VoidCallback onDone;

  @override
  ConsumerState<PhotosStep> createState() => _PhotosStepState();
}

class _PhotosStepState extends ConsumerState<PhotosStep> {
  late final List<ProviderService> _services = [...?ref.read(providerControllerProvider).value?.services];
  Timer? _refresh;
  bool _uploading = false;
  bool _busy = false;
  ProblemDetails? _problem;
  String? _message;

  @override
  void dispose() {
    _refresh?.cancel();
    super.dispose();
  }

  Future<void> _addPhoto() async {
    setState(() => _uploading = true);
    final outcome = await ref.read(providerControllerProvider.notifier).addPhoto(avatar: false);
    if (!mounted) {
      return;
    }

    setState(() => _uploading = false);
    if (outcome == PhotoOutcome.failed) {
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(AppLocalizations.of(context).providerPhotoUploadFailed)));
    }

    ref.invalidate(providerPhotosProvider);
  }

  Future<void> _deletePhoto(ProviderPhoto photo) async {
    await ref.read(providerControllerProvider.notifier).run((repository) => repository.deletePhoto(photo.id));
    ref.invalidate(providerPhotosProvider);
  }

  Future<void> _editService([int? index]) async {
    final service = await ServiceEditorSheet.show(context, service: index == null ? null : _services[index]);
    if (service == null || !mounted) {
      return;
    }

    setState(() => index == null ? _services.add(service) : _services[index] = service);
  }

  Future<void> _save() async {
    if (_services.isEmpty) {
      setState(() => _message = AppLocalizations.of(context).providerServicesEmpty);
      return;
    }

    setState(() {
      _busy = true;
      _problem = null;
      _message = null;
    });
    final result = await ref.read(providerControllerProvider.notifier).run((repository) => repository.setServices(_services));
    if (!mounted) {
      return;
    }

    setState(() => _busy = false);
    switch (result) {
      case ApiSuccess():
        widget.onDone();
      case ApiFailure(:final problem):
        setState(() => _problem = problem);
    }
  }

  void _scheduleRefresh(List<ProviderPhoto> photos) {
    _refresh?.cancel();
    if (photos.any((photo) => photo.status == PhotoStatus.processing)) {
      _refresh = Timer(const Duration(seconds: 2), () => ref.invalidate(providerPhotosProvider));
    }
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final photos = ref.watch(providerPhotosProvider);
    if (photos case AsyncData(:final value)) {
      _scheduleRefresh(value);
    }

    return FormPage(
      title: l10n.providerStepPhotos,
      actionLabel: widget.editing ? l10n.commonSave : l10n.providerContinue,
      busy: _busy,
      onAction: _save,
      message: _message ?? (_problem == null ? null : problemMessage(l10n, _problem!)),
      children: [
        if (!widget.editing) const StepProgress(step: 3, total: 4),
        SectionTitle(l10n.providerPhotos, hint: l10n.providerPhotosHint),
        GridView.count(
          crossAxisCount: 3,
          shrinkWrap: true,
          physics: const NeverScrollableScrollPhysics(),
          mainAxisSpacing: VitrioSpace.sp8,
          crossAxisSpacing: VitrioSpace.sp8,
          children: [
            for (final photo in photos.value ?? const <ProviderPhoto>[]) _PhotoTile(photo: photo, onDelete: () => _deletePhoto(photo)),
            Semantics(
              button: true,
              label: l10n.providerPhotoAdd,
              child: InkWell(
                borderRadius: const BorderRadius.all(VitrioRadius.lg),
                onTap: _uploading ? null : _addPhoto,
                child: DecoratedBox(
                  decoration: BoxDecoration(
                    color: c.surfaceInset,
                    borderRadius: const BorderRadius.all(VitrioRadius.lg),
                    border: Border.all(color: c.line),
                  ),
                  child: Center(
                    child: _uploading ? const CircularProgressIndicator() : VitrioIcon(VitrioIconName.camera, color: c.plum),
                  ),
                ),
              ),
            ),
          ],
        ),
        SectionTitle(l10n.providerServices),
        for (final (index, service) in _services.indexed)
          ListTile(
            contentPadding: EdgeInsets.zero,
            title: Text(service.title),
            subtitle: Text(priceText(l10n, service)),
            onTap: () => _editService(index),
            trailing: IconButton(
              tooltip: l10n.providerServiceRemove,
              icon: const VitrioIcon(VitrioIconName.x),
              onPressed: () => setState(() => _services.removeAt(index)),
            ),
          ),
        Align(
          alignment: AlignmentDirectional.centerStart,
          child: TextButton.icon(
            onPressed: _services.length >= 20 ? null : _editService,
            icon: const VitrioIcon(VitrioIconName.plus),
            label: Text(l10n.providerServiceAdd),
          ),
        ),
      ],
    );
  }
}

class _PhotoTile extends StatelessWidget {
  const _PhotoTile({required this.photo, required this.onDelete});

  final ProviderPhoto photo;
  final VoidCallback onDelete;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    return ClipRRect(
      borderRadius: const BorderRadius.all(VitrioRadius.lg),
      child: Stack(
        fit: StackFit.expand,
        children: [
          if (photo.thumbnailUrl != null)
            Image.network(photo.thumbnailUrl!, fit: BoxFit.cover, errorBuilder: (_, _, _) => ColoredBox(color: c.surfaceInset))
          else
            ColoredBox(color: c.surfaceInset),
          if (photo.status != PhotoStatus.active)
            Center(
              child: VitrioBadge(
                label: photo.status == PhotoStatus.processing ? l10n.providerPhotoProcessing : l10n.providerPhotoRejected,
                tone: photo.status == PhotoStatus.processing ? VitrioBadgeTone.info : VitrioBadgeTone.danger,
              ),
            ),
          Positioned(
            top: 0,
            right: 0,
            child: IconButton(
              tooltip: l10n.providerPhotoDelete,
              onPressed: onDelete,
              icon: VitrioIcon(VitrioIconName.x, color: c.onPlum),
              style: IconButton.styleFrom(backgroundColor: c.plum.withValues(alpha: 0.6)),
            ),
          ),
        ],
      ),
    );
  }
}
