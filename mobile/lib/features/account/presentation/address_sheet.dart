import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../core/design_system/tokens/vitrio_radius.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/design_system/widgets/vitrio_button.dart';
import '../../../core/design_system/widgets/vitrio_button_variant.dart';
import '../../../core/design_system/widgets/vitrio_icon.dart';
import '../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../core/design_system/widgets/vitrio_notice.dart';
import '../../../core/design_system/widgets/vitrio_text_field.dart';
import '../../../core/network/api_result.dart';
import '../../../core/network/problem_details.dart';
import '../../../core/presentation/brazilian_format.dart';
import '../../../core/presentation/mask_input_formatter.dart';
import '../../../core/presentation/problem_message.dart';
import '../../../l10n/app_localizations.dart';
import '../../location/data/location_providers.dart';
import '../../location/domain/cep_address.dart';
import '../data/address_providers.dart';
import '../domain/new_address.dart';
import '../domain/saved_address.dart';
import 'saved_addresses.dart';

class AddressSheet extends ConsumerStatefulWidget {
  const AddressSheet({super.key, required this.addresses, this.selectedId});

  final List<SavedAddress> addresses;
  final String? selectedId;

  static Future<SavedAddress?> show(BuildContext context, {required List<SavedAddress> addresses, String? selectedId}) =>
      showModalBottomSheet<SavedAddress>(
        context: context,
        isScrollControlled: true,
        useSafeArea: true,
        useRootNavigator: true,
        showDragHandle: true,
        builder: (_) => AddressSheet(addresses: addresses, selectedId: selectedId),
      );

  @override
  ConsumerState<AddressSheet> createState() => _AddressSheetState();
}

class _AddressSheetState extends ConsumerState<AddressSheet> {
  static const _cepDigits = 8;

  late bool _adding = widget.addresses.isEmpty;
  late final TextEditingController _label = TextEditingController();
  late final TextEditingController _cep = TextEditingController()..addListener(_lookUpCep);
  final _street = TextEditingController();
  final _number = TextEditingController();
  final _complement = TextEditingController();
  final _neighborhood = TextEditingController();
  final _city = TextEditingController();
  CepAddress? _found;
  String? _lookedUpCep;
  bool _searching = false;
  bool _cepNotFound = false;
  bool _showErrors = false;
  bool _busy = false;
  ProblemDetails? _problem;

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (_label.text.isEmpty) {
      _label.text = AppLocalizations.of(context).addressLabelDefault;
    }
  }

  @override
  void dispose() {
    _label.dispose();
    _cep.dispose();
    _street.dispose();
    _number.dispose();
    _complement.dispose();
    _neighborhood.dispose();
    _city.dispose();
    super.dispose();
  }

  static String _digits(String text) => text.replaceAll(RegExp(r'\D'), '');

  Future<void> _lookUpCep() async {
    final cep = _digits(_cep.text);
    if (cep.length != _cepDigits) {
      if (_found != null || _cepNotFound || _lookedUpCep != null) {
        setState(() {
          _found = null;
          _lookedUpCep = null;
          _cepNotFound = false;
        });
      }

      return;
    }

    if (cep == _lookedUpCep) {
      return;
    }

    setState(() {
      _lookedUpCep = cep;
      _searching = true;
      _cepNotFound = false;
    });
    final result = await ref.read(citiesRepositoryProvider).lookupCep(cep);
    if (!mounted || _digits(_cep.text) != cep) {
      return;
    }

    final l10n = AppLocalizations.of(context);
    setState(() {
      _searching = false;
      switch (result) {
        case ApiSuccess(:final data):
          _found = data;
          _street.text = data.street ?? '';
          _neighborhood.text = data.neighborhood ?? '';
          _city.text = l10n.addressCity(data.city.name, data.city.state);
        case ApiFailure(:final problem):
          _found = null;
          _cepNotFound = problem.status == 404;
          _problem = problem.status == 404 ? null : problem;
      }
    });
  }

  Map<String, String> _validate(AppLocalizations l10n) => {
        if (_street.text.trim().isEmpty) 'street': l10n.addressStreetRequired,
        if (_number.text.trim().isEmpty) 'number': l10n.addressNumberRequired,
        if (_neighborhood.text.trim().isEmpty) 'neighborhood': l10n.addressNeighborhoodRequired,
      };

  Future<void> _save() async {
    final found = _found;
    if (found == null || _validate(AppLocalizations.of(context)).isNotEmpty) {
      setState(() => _showErrors = true);
      return;
    }

    setState(() {
      _busy = true;
      _problem = null;
    });

    final complement = _complement.text.trim();
    final result = await ref.read(addressRepositoryProvider).add(
          NewAddress(
            label: _label.text.trim(),
            cep: _digits(_cep.text),
            number: _number.text.trim(),
            complement: complement.isEmpty ? null : complement,
            street: _street.text.trim(),
            neighborhood: _neighborhood.text.trim(),
            state: found.city.state,
            cityIbgeCode: found.city.ibgeCode,
          ),
        );
    if (!mounted) {
      return;
    }

    switch (result) {
      case ApiSuccess(:final data):
        ref.invalidate(savedAddressesProvider);
        Navigator.of(context).pop(data);
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
    final found = _found;
    final errors = _showErrors ? _validate(l10n) : const <String, String>{};
    return SingleChildScrollView(
      padding: EdgeInsets.fromLTRB(VitrioSpace.sp20, 0, VitrioSpace.sp20, MediaQuery.viewInsetsOf(context).bottom + VitrioSpace.sp24),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Semantics(
            header: true,
            child: Text(_adding ? l10n.addressNew : l10n.addressSheetTitle, style: VitrioText.title.copyWith(color: c.ink)),
          ),
          const SizedBox(height: VitrioSpace.sp16),
          if (!_adding) ...[
            for (final address in widget.addresses) ...[
              _AddressOption(
                address: address,
                selected: address.id == widget.selectedId,
                onTap: () => Navigator.of(context).pop(address),
              ),
              const SizedBox(height: VitrioSpace.sp8),
            ],
            const SizedBox(height: VitrioSpace.sp8),
            VitrioButton(
              label: l10n.addressNew,
              variant: VitrioButtonVariant.secondary,
              leadingIcon: VitrioIconName.plus,
              onPressed: () => setState(() => _adding = true),
            ),
          ] else ...[
            VitrioTextField(
              label: l10n.addressCep,
              controller: _cep,
              keyboardType: TextInputType.number,
              inputFormatters: const [MaskInputFormatter(cepText)],
              placeholder: l10n.providerCepPlaceholder,
              hint: _searching ? l10n.addressCepSearching : (found == null ? l10n.addressCepHint : null),
              error: fieldError(_problem, 'cep') ?? (_cepNotFound ? l10n.addressCepNotFound : null),
              autofillHints: const [AutofillHints.postalCode],
            ),
            if (found != null) ...[
              const SizedBox(height: VitrioSpace.sp14),
              VitrioTextField(
                label: l10n.addressStreetLabel,
                controller: _street,
                enabled: (found.street ?? '').isEmpty,
                textCapitalization: TextCapitalization.words,
                error: errors['street'] ?? fieldError(_problem, 'street'),
                onChanged: (_) => setState(() {}),
              ),
              const SizedBox(height: VitrioSpace.sp14),
              Row(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Expanded(
                    child: VitrioTextField(
                      label: l10n.addressNumber,
                      controller: _number,
                      keyboardType: TextInputType.text,
                      error: errors['number'] ?? fieldError(_problem, 'number'),
                      onChanged: (_) => setState(() {}),
                    ),
                  ),
                  const SizedBox(width: VitrioSpace.sp12),
                  Expanded(
                    flex: 2,
                    child: VitrioTextField(
                      label: l10n.addressComplement,
                      controller: _complement,
                      error: fieldError(_problem, 'complement'),
                    ),
                  ),
                ],
              ),
              const SizedBox(height: VitrioSpace.sp14),
              VitrioTextField(
                label: l10n.addressNeighborhoodLabel,
                controller: _neighborhood,
                enabled: (found.neighborhood ?? '').isEmpty,
                textCapitalization: TextCapitalization.words,
                error: errors['neighborhood'] ?? fieldError(_problem, 'neighborhood'),
                onChanged: (_) => setState(() {}),
              ),
              const SizedBox(height: VitrioSpace.sp14),
              VitrioTextField(label: l10n.addressCityLabel, controller: _city, enabled: false),
              const SizedBox(height: VitrioSpace.sp14),
              VitrioTextField(
                label: l10n.addressLabel,
                controller: _label,
                placeholder: l10n.addressLabelPlaceholder,
                error: fieldError(_problem, 'label'),
              ),
            ],
            if (_problem != null && !_problem!.isValidation) ...[
              const SizedBox(height: VitrioSpace.sp14),
              VitrioNotice(message: problemMessage(l10n, _problem!)),
            ],
            const SizedBox(height: VitrioSpace.sp20),
            VitrioButton(label: l10n.addressSave, onPressed: found == null ? null : _save, busy: _busy),
          ],
        ],
      ),
    );
  }
}

class _AddressOption extends StatelessWidget {
  const _AddressOption({required this.address, required this.selected, required this.onTap});

  final SavedAddress address;
  final bool selected;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    return Semantics(
      inMutuallyExclusiveGroup: true,
      checked: selected,
      button: true,
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          borderRadius: const BorderRadius.all(VitrioRadius.lg),
          onTap: onTap,
          child: Container(
            padding: const EdgeInsets.all(VitrioSpace.sp14),
            decoration: BoxDecoration(
              color: selected ? c.plumSoft : c.surfaceRaised,
              borderRadius: const BorderRadius.all(VitrioRadius.lg),
              border: Border.all(color: selected ? c.plum : c.line, width: selected ? 1.5 : 1),
            ),
            child: Row(
              children: [
                VitrioIcon(VitrioIconName.pin, color: c.plum),
                const SizedBox(width: VitrioSpace.sp12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        l10n.addressSummary(address.label, address.neighborhood),
                        style: VitrioText.bodyStrong.copyWith(color: selected ? c.plum : c.ink),
                      ),
                      const SizedBox(height: 2),
                      Text(
                        '${address.street}, ${address.number} · ${l10n.addressCity(address.city, address.state)}',
                        style: VitrioText.weighted(VitrioText.meta, 400).copyWith(color: c.inkMuted),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
