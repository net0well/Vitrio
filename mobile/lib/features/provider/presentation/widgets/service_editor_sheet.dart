import 'package:flutter/material.dart';

import '../../../../core/design_system/tokens/vitrio_space.dart';
import '../../../../l10n/app_localizations.dart';
import '../../domain/price_unit.dart';
import '../../domain/provider_service.dart';
import '../provider_labels.dart';

class ServiceEditorSheet extends StatefulWidget {
  const ServiceEditorSheet({super.key, this.service});

  final ProviderService? service;

  static Future<ProviderService?> show(BuildContext context, {ProviderService? service}) => showModalBottomSheet<ProviderService>(
        context: context,
        isScrollControlled: true,
        useSafeArea: true,
        useRootNavigator: true,
        showDragHandle: true,
        builder: (_) => ServiceEditorSheet(service: service),
      );

  @override
  State<ServiceEditorSheet> createState() => _ServiceEditorSheetState();
}

class _ServiceEditorSheetState extends State<ServiceEditorSheet> {
  late final TextEditingController _title = TextEditingController(text: widget.service?.title);
  late final TextEditingController _description = TextEditingController(text: widget.service?.description);
  late final TextEditingController _price = TextEditingController(
    text: widget.service?.priceFrom?.toStringAsFixed(2).replaceAll('.', ','),
  );
  late bool _onQuote = widget.service?.onQuote ?? false;
  late PriceUnit _unit = widget.service?.unit ?? PriceUnit.perService;

  @override
  void dispose() {
    _title.dispose();
    _description.dispose();
    _price.dispose();
    super.dispose();
  }

  void _done() {
    final title = _title.text.trim();
    final price = double.tryParse(_price.text.trim().replaceAll('.', '').replaceAll(',', '.'));
    if (title.isEmpty || (!_onQuote && price == null)) {
      return;
    }

    final description = _description.text.trim();
    Navigator.of(context).pop(ProviderService(
      title: title,
      description: description.isEmpty ? null : description,
      priceFrom: _onQuote ? null : price,
      unit: _unit,
      onQuote: _onQuote,
    ));
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    return Padding(
      padding: EdgeInsets.fromLTRB(VitrioSpace.sp20, 0, VitrioSpace.sp20, MediaQuery.viewInsetsOf(context).bottom + VitrioSpace.sp20),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          TextField(
            controller: _title,
            autofocus: true,
            maxLength: 60,
            textCapitalization: TextCapitalization.sentences,
            decoration: InputDecoration(labelText: l10n.providerServiceTitle),
          ),
          TextField(
            controller: _description,
            maxLength: 140,
            textCapitalization: TextCapitalization.sentences,
            decoration: InputDecoration(labelText: l10n.providerServiceDescription),
          ),
          SwitchListTile(
            contentPadding: EdgeInsets.zero,
            title: Text(l10n.providerServiceOnQuote),
            value: _onQuote,
            onChanged: (value) => setState(() => _onQuote = value),
          ),
          if (!_onQuote) ...[
            TextField(
              controller: _price,
              keyboardType: const TextInputType.numberWithOptions(decimal: true),
              decoration: InputDecoration(labelText: l10n.providerServicePrice),
            ),
            const SizedBox(height: VitrioSpace.sp12),
            DropdownButtonFormField<PriceUnit>(
              initialValue: _unit,
              decoration: InputDecoration(labelText: l10n.providerServiceUnit),
              items: [
                for (final unit in PriceUnit.values) DropdownMenuItem(value: unit, child: Text(unitLabel(l10n, unit))),
              ],
              onChanged: (value) => setState(() => _unit = value ?? _unit),
            ),
          ],
          const SizedBox(height: VitrioSpace.sp20),
          SizedBox(height: VitrioSpace.cta, child: FilledButton(onPressed: _done, child: Text(l10n.commonSave))),
        ],
      ),
    );
  }
}
