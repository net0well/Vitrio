import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
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
import '../../../core/presentation/problem_message.dart';
import '../../../l10n/app_localizations.dart';
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
  late bool _adding = widget.addresses.isEmpty;
  late final TextEditingController _label = TextEditingController();
  final _cep = TextEditingController();
  final _number = TextEditingController();
  final _complement = TextEditingController();
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
    _number.dispose();
    _complement.dispose();
    super.dispose();
  }

  Future<void> _save() async {
    setState(() {
      _busy = true;
      _problem = null;
    });

    final complement = _complement.text.trim();
    final result = await ref.read(addressRepositoryProvider).add(
          NewAddress(
            label: _label.text.trim(),
            cep: _cep.text.trim(),
            number: _number.text.trim(),
            complement: complement.isEmpty ? null : complement,
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
              inputFormatters: [FilteringTextInputFormatter.allow(RegExp(r'[0-9-]')), LengthLimitingTextInputFormatter(9)],
              error: fieldError(_problem, 'cep'),
              autofillHints: const [AutofillHints.postalCode],
            ),
            const SizedBox(height: VitrioSpace.sp14),
            VitrioTextField(
              label: l10n.addressNumber,
              controller: _number,
              keyboardType: TextInputType.text,
              error: fieldError(_problem, 'number'),
            ),
            const SizedBox(height: VitrioSpace.sp14),
            VitrioTextField(label: l10n.addressComplement, controller: _complement, error: fieldError(_problem, 'complement')),
            const SizedBox(height: VitrioSpace.sp14),
            VitrioTextField(
              label: l10n.addressLabel,
              controller: _label,
              placeholder: l10n.addressLabelPlaceholder,
              error: fieldError(_problem, 'label'),
            ),
            if (_problem != null && !_problem!.isValidation) ...[
              const SizedBox(height: VitrioSpace.sp14),
              VitrioNotice(message: problemMessage(l10n, _problem!)),
            ],
            const SizedBox(height: VitrioSpace.sp20),
            VitrioButton(label: l10n.addressSave, onPressed: _save, busy: _busy),
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
