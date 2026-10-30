import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../../core/design_system/tokens/vitrio_radius.dart';
import '../../../../core/design_system/tokens/vitrio_space.dart';
import '../../../../core/design_system/widgets/vitrio_add_photo_tile.dart';
import '../../../../core/design_system/widgets/vitrio_badge.dart';
import '../../../../core/design_system/widgets/vitrio_badge_tone.dart';
import '../../../../core/design_system/widgets/vitrio_button.dart';
import '../../../../core/design_system/widgets/vitrio_button_size.dart';
import '../../../../core/design_system/widgets/vitrio_button_variant.dart';
import '../../../../core/design_system/widgets/vitrio_icon.dart';
import '../../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../../core/design_system/widgets/vitrio_list_card.dart';
import '../../../../core/design_system/widgets/vitrio_section_header.dart';
import '../../../../core/network/api_result.dart';
import '../../../../core/network/problem_details.dart';
import '../../../../core/presentation/problem_message.dart';
import '../../../../l10n/app_localizations.dart';
import '../../domain/photo_status.dart';
import '../../domain/provider_photo.dart';
import '../../domain/provider_service.dart';
import '../photo_outcome.dart';
import '../provider_controller.dart';
import '../provider_queries.dart';
import '../provider_step.dart';
import '../widgets/provider_step_page.dart';
import '../widgets/service_editor_sheet.dart';
import '../widgets/service_price_row.dart';

class PhotosStep extends ConsumerStatefulWidget {
  const PhotosStep({super.key, required this.editing, required this.onDone});

  final bool editing;
  final VoidCallback onDone;

  @override
  ConsumerState<PhotosStep> createState() => _PhotosStepState();
}

class _PhotosStepState extends ConsumerState<PhotosStep> {
  static const _maxServices = 20;

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
    final edit = await ServiceEditorSheet.show(context, service: index == null ? null : _services[index]);
    if (edit == null || !mounted) {
      return;
    }

    setState(() {
      switch ((edit.service, index)) {
        case (null, final int index):
          _services.removeAt(index);
        case (final ProviderService service, final int index):
          _services[index] = service;
        case (final ProviderService service, null):
          _services.add(service);
        case (null, null):
          break;
      }
    });
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
    final photos = ref.watch(providerPhotosProvider);
    if (photos case AsyncData(:final value)) {
      _scheduleRefresh(value);
    }

    return ProviderStepPage(
      step: ProviderStep.photos,
      editing: widget.editing,
      busy: _busy,
      onAction: _save,
      message: _message ?? (_problem == null ? null : problemMessage(l10n, _problem!)),
      children: [
        VitrioSectionHeader(l10n.providerPhotos, hint: l10n.providerPhotosHint),
        const SizedBox(height: VitrioSpace.sp12),
        GridView.count(
          crossAxisCount: 3,
          shrinkWrap: true,
          physics: const NeverScrollableScrollPhysics(),
          mainAxisSpacing: VitrioSpace.sp8,
          crossAxisSpacing: VitrioSpace.sp8,
          children: [
            for (final photo in photos.value ?? const <ProviderPhoto>[]) _PhotoTile(photo: photo, onDelete: () => _deletePhoto(photo)),
            VitrioAddPhotoTile(label: l10n.providerPhotoAdd, busy: _uploading, onTap: _addPhoto),
          ],
        ),
        const SizedBox(height: VitrioSpace.sp24),
        VitrioSectionHeader(l10n.providerServices),
        const SizedBox(height: VitrioSpace.sp12),
        if (_services.isNotEmpty) ...[
          VitrioListCard(
            children: [
              for (final (index, service) in _services.indexed) ServicePriceRow(service: service, onTap: () => _editService(index)),
            ],
          ),
          const SizedBox(height: VitrioSpace.sp12),
        ],
        Align(
          alignment: AlignmentDirectional.centerStart,
          child: VitrioButton(
            label: l10n.providerServiceAdd,
            variant: VitrioButtonVariant.secondary,
            size: VitrioButtonSize.md,
            leadingIcon: VitrioIconName.plus,
            onPressed: _services.length >= _maxServices ? null : _editService,
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
    final placeholder = ColoredBox(
      color: c.plum,
      child: Center(child: Opacity(opacity: 0.45, child: VitrioIcon(VitrioIconName.image, size: 30, color: c.onPlum))),
    );
    return ClipRRect(
      borderRadius: const BorderRadius.all(VitrioRadius.xl),
      child: Stack(
        fit: StackFit.expand,
        children: [
          if (photo.thumbnailUrl != null)
            Image.network(photo.thumbnailUrl!, fit: BoxFit.cover, errorBuilder: (_, _, _) => placeholder)
          else
            placeholder,
          if (photo.status != PhotoStatus.active)
            Center(
              child: VitrioBadge(
                label: photo.status == PhotoStatus.processing ? l10n.providerPhotoProcessing : l10n.providerPhotoRejected,
                tone: photo.status == PhotoStatus.processing ? VitrioBadgeTone.info : VitrioBadgeTone.danger,
              ),
            ),
          PositionedDirectional(
            top: 0,
            end: 0,
            child: Semantics(
              button: true,
              label: l10n.providerPhotoDelete,
              excludeSemantics: true,
              child: InkResponse(
                onTap: onDelete,
                radius: VitrioSpace.tap / 2,
                child: Container(
                  width: VitrioSpace.tap,
                  height: VitrioSpace.tap,
                  alignment: AlignmentDirectional.topEnd,
                  padding: const EdgeInsets.all(VitrioSpace.sp6),
                  child: Container(
                  width: 24,
                  height: 24,
                  decoration: BoxDecoration(
                    color: c.ink,
                    shape: BoxShape.circle,
                    border: Border.all(color: c.surface, width: 2),
                  ),
                  alignment: Alignment.center,
                  child: VitrioIcon(VitrioIconName.x, size: 11, color: c.surfaceRaised, strokeWidth: 3),
                  ),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}
