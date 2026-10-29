import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../../core/design_system/tokens/vitrio_space.dart';
import '../../../../core/design_system/widgets/vitrio_badge.dart';
import '../../../../core/design_system/widgets/vitrio_badge_tone.dart';
import '../../../../core/design_system/widgets/vitrio_chip.dart';
import '../../../../core/design_system/widgets/vitrio_chip_style.dart';
import '../../../../core/design_system/widgets/vitrio_icon.dart';
import '../../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../../core/network/api_result.dart';
import '../../../../core/network/problem_details.dart';
import '../../../../core/presentation/form_page.dart';
import '../../../../core/presentation/problem_message.dart';
import '../../../../l10n/app_localizations.dart';
import '../../domain/mei_check.dart';
import '../../domain/mei_status.dart';
import '../../domain/profile_changes.dart';
import '../../domain/service_area_draft.dart';
import '../provider_controller.dart';
import '../provider_labels.dart';
import '../provider_queries.dart';
import '../widgets/section_title.dart';
import '../widgets/step_progress.dart';

class ShowcaseStep extends ConsumerStatefulWidget {
  const ShowcaseStep({super.key, required this.editing, required this.onDone});

  final bool editing;
  final VoidCallback onDone;

  @override
  ConsumerState<ShowcaseStep> createState() => _ShowcaseStepState();
}

class _ShowcaseStepState extends ConsumerState<ShowcaseStep> {
  static const _maxCategories = 3;
  static const _maxNeighborhoods = 30;

  late final List<int> _categories;
  late final TextEditingController _cep;
  late final TextEditingController _number;
  late final TextEditingController _complement;
  late final TextEditingController _neighborhood;
  late final TextEditingController _phone;
  late final TextEditingController _cnpj;
  late final List<String> _neighborhoods;
  late double _radius;
  late bool _whatsApp;
  MeiCheck? _mei;
  bool _busy = false;
  bool _checkingMei = false;
  ProblemDetails? _problem;
  String? _message;

  @override
  void initState() {
    super.initState();
    final profile = ref.read(providerControllerProvider).value;
    final area = profile?.serviceArea;
    final primary = profile?.categories.where((category) => category.isPrimary).map((category) => category.id) ?? const <int>[];
    final others = profile?.categories.where((category) => !category.isPrimary).map((category) => category.id) ?? const <int>[];
    _categories = [...primary, ...others];
    _cep = TextEditingController(text: area?.cep);
    _number = TextEditingController(text: area?.number);
    _complement = TextEditingController(text: area?.complement);
    _neighborhood = TextEditingController();
    _phone = TextEditingController(text: phoneText(profile?.contactPhone));
    _cnpj = TextEditingController(text: profile?.mei?.cnpj);
    _neighborhoods = [...?area?.neighborhoods];
    _radius = (area?.radiusKm ?? 10).toDouble();
    _whatsApp = profile?.whatsAppEnabled ?? true;
    if (profile?.mei case final mei?) {
      _mei = MeiCheck(status: mei.status, companyName: mei.companyName, reason: mei.reason);
    }
  }

  @override
  void dispose() {
    for (final controller in [_cep, _number, _complement, _neighborhood, _phone, _cnpj]) {
      controller.dispose();
    }

    super.dispose();
  }

  void _toggleCategory(int id) => setState(() {
        if (_categories.contains(id)) {
          _categories.remove(id);
        } else if (_categories.length < _maxCategories) {
          _categories.add(id);
        }
      });

  void _addNeighborhood() {
    final name = _neighborhood.text.trim();
    if (name.isEmpty || _neighborhoods.length >= _maxNeighborhoods) {
      return;
    }

    setState(() {
      if (!_neighborhoods.any((existing) => existing.toLowerCase() == name.toLowerCase())) {
        _neighborhoods.add(name);
      }

      _neighborhood.clear();
    });
  }

  Future<void> _verifyMei() async {
    setState(() => _checkingMei = true);
    final controller = ref.read(providerControllerProvider.notifier);
    MeiCheck? check;
    final result = await controller.run((repository) async {
      final verified = await repository.verifyMei(_cnpj.text.trim());
      if (verified case ApiSuccess(:final data)) {
        check = data;
      }

      return verified;
    });
    if (!mounted) {
      return;
    }

    setState(() {
      _checkingMei = false;
      _mei = check ?? _mei;
      _problem = switch (result) {
        ApiFailure(:final problem) => problem,
        ApiSuccess() => null,
      };
    });
  }

  Future<void> _save() async {
    final l10n = AppLocalizations.of(context);
    if (_categories.isEmpty) {
      setState(() => _message = l10n.providerCategoriesRequired);
      return;
    }

    setState(() {
      _busy = true;
      _problem = null;
      _message = null;
    });

    final controller = ref.read(providerControllerProvider.notifier);
    final area = ServiceAreaDraft(
      cep: _cep.text.trim(),
      number: _number.text.trim(),
      complement: _complement.text.trim().isEmpty ? null : _complement.text.trim(),
      radiusKm: _radius.round(),
      neighborhoods: _neighborhoods,
    );
    for (final step in <Future<ApiResult<Object?>> Function()>[
      () => controller.run((repository) => repository.setCategories(_categories, _categories.first)),
      () => controller.run((repository) => repository.setServiceArea(area)),
      () => controller.run(
            (repository) => repository.update(ProfileChanges(
              contactPhone: _phone.text.trim().isEmpty ? null : _phone.text.trim(),
              whatsAppEnabled: _whatsApp,
            )),
          ),
    ]) {
      if (await step() case ApiFailure(:final problem)) {
        if (mounted) {
          setState(() {
            _busy = false;
            _problem = problem;
          });
        }

        return;
      }
    }

    if (!mounted) {
      return;
    }

    setState(() => _busy = false);
    if (ref.read(providerControllerProvider).value?.serviceArea?.locationApproximate == true) {
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(l10n.providerApproximate)));
    }

    widget.onDone();
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final categories = ref.watch(serviceCategoriesProvider);
    final mei = _mei;
    return FormPage(
      title: l10n.providerStepShowcase,
      actionLabel: widget.editing ? l10n.commonSave : l10n.providerContinue,
      busy: _busy,
      onAction: _save,
      message: _message ?? (_problem == null || _problem!.isValidation ? null : problemMessage(l10n, _problem!)),
      children: [
        if (!widget.editing) const StepProgress(step: 2, total: 4),
        SectionTitle(l10n.providerCategories, hint: l10n.providerCategoriesHint),
        switch (categories) {
          AsyncData(:final value) => Wrap(
              spacing: VitrioSpace.sp8,
              runSpacing: VitrioSpace.sp8,
              children: [
                for (final category in value)
                  VitrioChip(
                    label: _categories.isNotEmpty && _categories.first == category.id
                        ? '${category.name} · ${l10n.providerPrimary}'
                        : category.name,
                    selected: _categories.contains(category.id),
                    onSelected: (_) => _toggleCategory(category.id),
                  ),
              ],
            ),
          AsyncError() => TextButton(onPressed: () => ref.invalidate(serviceCategoriesProvider), child: Text(l10n.commonRetry)),
          _ => const Center(child: CircularProgressIndicator()),
        },
        SectionTitle(l10n.providerServiceArea, hint: l10n.providerBasePrivate),
        TextField(
          controller: _cep,
          keyboardType: TextInputType.number,
          decoration: InputDecoration(labelText: l10n.providerCep, errorText: fieldError(_problem, 'cep')),
        ),
        const SizedBox(height: VitrioSpace.sp12),
        Row(
          children: [
            Expanded(
              child: TextField(
                controller: _number,
                decoration: InputDecoration(labelText: l10n.providerNumber, errorText: fieldError(_problem, 'number')),
              ),
            ),
            const SizedBox(width: VitrioSpace.sp12),
            Expanded(
              flex: 2,
              child: TextField(controller: _complement, decoration: InputDecoration(labelText: l10n.providerComplement)),
            ),
          ],
        ),
        const SizedBox(height: VitrioSpace.sp16),
        Text(l10n.providerRadius(_radius.round())),
        Slider(
          value: _radius,
          min: 1,
          max: 50,
          divisions: 49,
          label: l10n.providerRadius(_radius.round()),
          onChanged: (value) => setState(() => _radius = value),
        ),
        Text(l10n.providerNeighborhoods, style: Theme.of(context).textTheme.titleSmall),
        const SizedBox(height: VitrioSpace.sp8),
        Row(
          children: [
            Expanded(
              child: TextField(
                controller: _neighborhood,
                textCapitalization: TextCapitalization.words,
                onSubmitted: (_) => _addNeighborhood(),
                decoration: InputDecoration(hintText: l10n.providerNeighborhoodHint),
              ),
            ),
            IconButton(tooltip: l10n.providerNeighborhoodAdd, onPressed: _addNeighborhood, icon: const VitrioIcon(VitrioIconName.plus)),
          ],
        ),
        const SizedBox(height: VitrioSpace.sp8),
        Wrap(
          spacing: VitrioSpace.sp8,
          runSpacing: VitrioSpace.sp8,
          children: [
            for (final name in _neighborhoods)
              VitrioChip(
                label: name,
                selected: true,
                style: VitrioChipStyle.soft,
                onSelected: (_) => setState(() => _neighborhoods.remove(name)),
                trailing: VitrioIcon(VitrioIconName.x, size: 14, color: c.plum),
              ),
          ],
        ),
        SectionTitle(l10n.providerContact),
        TextField(
          controller: _phone,
          keyboardType: TextInputType.phone,
          decoration: InputDecoration(labelText: l10n.providerContactPhone, errorText: fieldError(_problem, 'contactPhone')),
        ),
        SwitchListTile(
          contentPadding: EdgeInsets.zero,
          title: Text(l10n.providerWhatsApp),
          subtitle: Text(l10n.providerWhatsAppHint),
          value: _whatsApp,
          onChanged: (value) => setState(() => _whatsApp = value),
        ),
        SectionTitle(l10n.providerMei, hint: l10n.providerMeiHint),
        Row(
          children: [
            Expanded(
              child: TextField(
                controller: _cnpj,
                keyboardType: TextInputType.number,
                decoration: InputDecoration(labelText: l10n.providerCnpj, errorText: fieldError(_problem, 'cnpj')),
              ),
            ),
            const SizedBox(width: VitrioSpace.sp12),
            OutlinedButton(
              onPressed: _checkingMei ? null : _verifyMei,
              child: _checkingMei
                  ? const SizedBox.square(dimension: 18, child: CircularProgressIndicator(strokeWidth: 2))
                  : Text(l10n.providerVerify),
            ),
          ],
        ),
        if (mei != null) ...[
          const SizedBox(height: VitrioSpace.sp12),
          Align(
            alignment: AlignmentDirectional.centerStart,
            child: switch (mei.status) {
              MeiStatus.verified => VitrioBadge(
                  label: l10n.providerMeiVerified,
                  tone: VitrioBadgeTone.verified,
                  icon: const VitrioIcon(VitrioIconName.shieldCheck),
                ),
              MeiStatus.pending => VitrioBadge(label: l10n.providerMeiPending, tone: VitrioBadgeTone.warning),
              MeiStatus.lost => VitrioBadge(label: l10n.providerMeiLost, tone: VitrioBadgeTone.danger),
              MeiStatus.failed => VitrioBadge(label: mei.reason ?? l10n.errorGeneric, tone: VitrioBadgeTone.danger),
            },
          ),
        ],
      ],
    );
  }
}
