import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../../core/design_system/tokens/vitrio_radius.dart';
import '../../../../core/design_system/tokens/vitrio_space.dart';
import '../../../../core/design_system/tokens/vitrio_text.dart';
import '../../../../core/design_system/widgets/vitrio_badge_tone.dart';
import '../../../../core/design_system/widgets/vitrio_button.dart';
import '../../../../core/design_system/widgets/vitrio_button_size.dart';
import '../../../../core/design_system/widgets/vitrio_button_variant.dart';
import '../../../../core/design_system/widgets/vitrio_chip.dart';
import '../../../../core/design_system/widgets/vitrio_chip_style.dart';
import '../../../../core/design_system/widgets/vitrio_icon.dart';
import '../../../../core/design_system/widgets/vitrio_icon_button.dart';
import '../../../../core/design_system/widgets/vitrio_icon_button_variant.dart';
import '../../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../../core/design_system/widgets/vitrio_notice.dart';
import '../../../../core/design_system/widgets/vitrio_section_header.dart';
import '../../../../core/design_system/widgets/vitrio_switch_row.dart';
import '../../../../core/design_system/widgets/vitrio_text_field.dart';
import '../../../../core/network/api_result.dart';
import '../../../../core/network/problem_details.dart';
import '../../../../core/presentation/problem_message.dart';
import '../../../../l10n/app_localizations.dart';
import '../../../location/data/location_providers.dart';
import '../../../location/domain/cep_address.dart';
import '../../domain/mei_check.dart';
import '../../domain/mei_status.dart';
import '../../domain/profile_changes.dart';
import '../../domain/service_area_draft.dart';
import '../provider_controller.dart';
import '../provider_labels.dart';
import '../provider_queries.dart';
import '../provider_step.dart';
import '../widgets/provider_step_page.dart';

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
  CepAddress? _cepAddress;
  String? _lookedUpCep;
  bool _cepNotFound = false;
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
    _cep = TextEditingController(text: area?.cep)..addListener(_lookUpCep);
    _lookedUpCep = _digits(area?.cep ?? '');
    if (area != null) {
      _cepAddress = CepAddress(cep: area.cep, street: area.street, neighborhood: area.neighborhood, city: area.city);
    }

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

  static String _digits(String text) => text.replaceAll(RegExp(r'\D'), '');

  Future<void> _lookUpCep() async {
    final cep = _digits(_cep.text);
    if (cep.length != 8 || cep == _lookedUpCep) {
      return;
    }

    _lookedUpCep = cep;
    final result = await ref.read(citiesRepositoryProvider).lookupCep(cep);
    if (!mounted || _digits(_cep.text) != cep) {
      return;
    }

    setState(() {
      switch (result) {
        case ApiSuccess(:final data):
          _cepAddress = data;
          _cepNotFound = false;
          final neighborhood = data.neighborhood;
          if (neighborhood != null &&
              _neighborhoods.length < _maxNeighborhoods &&
              !_neighborhoods.any((existing) => existing.toLowerCase() == neighborhood.toLowerCase())) {
            _neighborhoods.insert(0, neighborhood);
          }
        case ApiFailure(:final problem):
          _cepAddress = null;
          _cepNotFound = problem.status == 404;
      }
    });
  }

  String? _cepHint() {
    final address = _cepAddress;
    if (address == null || _digits(address.cep) != _digits(_cep.text)) {
      return null;
    }

    final place = [?address.street, ?address.neighborhood].join(', ');
    final city = '${address.city.name}, ${address.city.state}';
    return place.isEmpty ? city : '$place · $city';
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
    return ProviderStepPage(
      step: ProviderStep.showcase,
      editing: widget.editing,
      busy: _busy,
      onAction: _save,
      message: _message ?? (_problem == null || _problem!.isValidation ? null : problemMessage(l10n, _problem!)),
      children: [
        VitrioSectionHeader(l10n.providerCategories, hint: l10n.providerCategoriesHint),
        const SizedBox(height: VitrioSpace.sp10),
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
          AsyncError() => Align(
              alignment: AlignmentDirectional.centerStart,
              child: VitrioButton(
                label: l10n.commonRetry,
                variant: VitrioButtonVariant.ghost,
                size: VitrioButtonSize.sm,
                onPressed: () => ref.invalidate(serviceCategoriesProvider),
              ),
            ),
          _ => const Center(child: CircularProgressIndicator()),
        },
        const SizedBox(height: VitrioSpace.sp24),
        VitrioSectionHeader(l10n.providerServiceArea),
        const SizedBox(height: VitrioSpace.sp10),
        VitrioTextField(
          label: l10n.providerCep,
          controller: _cep,
          keyboardType: TextInputType.number,
          placeholder: l10n.providerCepPlaceholder,
          hint: _cepHint(),
          error: fieldError(_problem, 'cep') ?? (_cepNotFound ? l10n.providerCepNotFound : null),
        ),
        const SizedBox(height: VitrioSpace.sp12),
        Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Expanded(
              child: VitrioTextField(label: l10n.providerNumber, controller: _number, error: fieldError(_problem, 'number')),
            ),
            const SizedBox(width: VitrioSpace.sp12),
            Expanded(
              flex: 2,
              child: VitrioTextField(label: l10n.providerComplement, controller: _complement),
            ),
          ],
        ),
        const SizedBox(height: VitrioSpace.sp10),
        Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Padding(
              padding: const EdgeInsets.only(top: 1),
              child: VitrioIcon(VitrioIconName.lock, size: 16, color: c.inkMuted),
            ),
            const SizedBox(width: VitrioSpace.sp8),
            Expanded(
              child: Text(l10n.providerBasePrivate, style: VitrioText.weighted(VitrioText.caption, 600).copyWith(color: c.inkMuted)),
            ),
          ],
        ),
        const SizedBox(height: VitrioSpace.sp16),
        if (_neighborhoods.isNotEmpty) ...[
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
          const SizedBox(height: VitrioSpace.sp10),
        ],
        Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Expanded(
              child: VitrioTextField(
                label: l10n.providerNeighborhoods,
                showLabel: false,
                controller: _neighborhood,
                textCapitalization: TextCapitalization.words,
                textInputAction: TextInputAction.done,
                placeholder: l10n.providerNeighborhoodHint,
                onSubmitted: (_) => _addNeighborhood(),
              ),
            ),
            const SizedBox(width: VitrioSpace.sp8),
            Padding(
              padding: const EdgeInsets.only(top: 5),
              child: VitrioIconButton(
                icon: VitrioIconName.plus,
                label: l10n.providerNeighborhoodAdd,
                variant: VitrioIconButtonVariant.soft,
                onPressed: _addNeighborhood,
              ),
            ),
          ],
        ),
        const SizedBox(height: VitrioSpace.sp14),
        Row(
          children: [
            Expanded(child: Text(l10n.providerRadiusLabel, style: VitrioText.label.copyWith(color: c.ink))),
            Text(l10n.providerRadius(_radius.round()), style: VitrioText.label.copyWith(color: c.plum)),
          ],
        ),
        Slider(
          value: _radius,
          min: 1,
          max: 50,
          divisions: 49,
          label: l10n.providerRadius(_radius.round()),
          semanticFormatterCallback: (value) => l10n.providerRadius(value.round()),
          onChanged: (value) => setState(() => _radius = value),
        ),
        const SizedBox(height: VitrioSpace.sp16),
        VitrioSectionHeader(l10n.providerContactPhone),
        const SizedBox(height: VitrioSpace.sp8),
        VitrioTextField(
          label: l10n.providerContactPhone,
          showLabel: false,
          controller: _phone,
          icon: VitrioIconName.message,
          keyboardType: TextInputType.phone,
          placeholder: l10n.providerPhonePlaceholder,
          hint: l10n.providerContactPhoneHint,
          error: fieldError(_problem, 'contactPhone'),
        ),
        const SizedBox(height: VitrioSpace.sp12),
        VitrioSwitchRow(
          label: l10n.providerWhatsApp,
          description: l10n.providerWhatsAppHint,
          value: _whatsApp,
          onChanged: (value) => setState(() => _whatsApp = value),
        ),
        const SizedBox(height: VitrioSpace.sp24),
        Container(
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
                children: [
                  Container(
                    width: VitrioSpace.tap,
                    height: VitrioSpace.tap,
                    decoration: BoxDecoration(color: c.greenSoft, borderRadius: const BorderRadius.all(VitrioRadius.lg)),
                    alignment: Alignment.center,
                    child: VitrioIcon(VitrioIconName.shieldCheck, color: c.green),
                  ),
                  const SizedBox(width: VitrioSpace.sp12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(l10n.providerMei, style: VitrioText.bodyStrong.copyWith(color: c.ink)),
                        const SizedBox(height: 2),
                        Text(l10n.providerMeiHint, style: VitrioText.weighted(VitrioText.caption, 600).copyWith(color: c.inkMuted)),
                      ],
                    ),
                  ),
                ],
              ),
              const SizedBox(height: VitrioSpace.sp12),
              Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Expanded(
                    child: VitrioTextField(
                      label: l10n.providerCnpj,
                      showLabel: false,
                      controller: _cnpj,
                      filledInset: true,
                      keyboardType: TextInputType.number,
                      placeholder: l10n.providerCnpjPlaceholder,
                      error: fieldError(_problem, 'cnpj'),
                    ),
                  ),
                  const SizedBox(width: VitrioSpace.sp8),
                  Padding(
                    padding: const EdgeInsets.only(top: 5),
                    child: VitrioButton(
                      label: l10n.providerVerify,
                      size: VitrioButtonSize.md,
                      busy: _checkingMei,
                      onPressed: _verifyMei,
                    ),
                  ),
                ],
              ),
              if (mei != null) ...[
                const SizedBox(height: VitrioSpace.sp10),
                switch (mei.status) {
                  MeiStatus.verified => VitrioNotice(
                      message: l10n.providerMeiVerified,
                      tone: VitrioBadgeTone.verified,
                      icon: VitrioIconName.shieldCheck,
                    ),
                  MeiStatus.pending => VitrioNotice(message: l10n.providerMeiPending, tone: VitrioBadgeTone.warning),
                  MeiStatus.lost => VitrioNotice(message: l10n.providerMeiLost),
                  MeiStatus.failed => VitrioNotice(message: mei.reason ?? l10n.errorGeneric),
                },
              ],
            ],
          ),
        ),
      ],
    );
  }
}
