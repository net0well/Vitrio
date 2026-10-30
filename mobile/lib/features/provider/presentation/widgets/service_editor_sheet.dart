import 'package:flutter/material.dart';

import '../../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../../core/design_system/tokens/vitrio_space.dart';
import '../../../../core/design_system/tokens/vitrio_text.dart';
import '../../../../core/design_system/widgets/vitrio_button.dart';
import '../../../../core/design_system/widgets/vitrio_button_variant.dart';
import '../../../../core/design_system/widgets/vitrio_chip.dart';
import '../../../../core/design_system/widgets/vitrio_chip_style.dart';
import '../../../../core/design_system/widgets/vitrio_switch_row.dart';
import '../../../../core/design_system/widgets/vitrio_text_field.dart';
import '../../../../l10n/app_localizations.dart';
import '../../domain/price_unit.dart';
import '../../domain/provider_service.dart';
import '../provider_labels.dart';

class ServiceEditorSheet extends StatefulWidget {
  const ServiceEditorSheet({super.key, this.service});

  final ProviderService? service;

  static Future<({ProviderService? service})?> show(BuildContext context, {ProviderService? service}) =>
      showModalBottomSheet<({ProviderService? service})>(
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
    Navigator.of(context).pop((
      service: ProviderService(
        title: title,
        description: description.isEmpty ? null : description,
        priceFrom: _onQuote ? null : price,
        unit: _unit,
        onQuote: _onQuote,
      ),
    ));
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    return SingleChildScrollView(
      padding: EdgeInsets.fromLTRB(VitrioSpace.sp20, 0, VitrioSpace.sp20, MediaQuery.viewInsetsOf(context).bottom + VitrioSpace.sp20),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Text(
            widget.service == null ? l10n.providerServiceAdd : l10n.providerServiceEdit,
            textAlign: TextAlign.center,
            style: VitrioText.section.copyWith(color: c.ink),
          ),
          const SizedBox(height: VitrioSpace.sp20),
          VitrioTextField(
            label: l10n.providerServiceTitle,
            controller: _title,
            autofocus: true,
            maxLength: 60,
            textCapitalization: TextCapitalization.sentences,
          ),
          const SizedBox(height: VitrioSpace.sp16),
          VitrioTextField(
            label: l10n.providerServiceDescription,
            controller: _description,
            maxLength: 140,
            textCapitalization: TextCapitalization.sentences,
          ),
          const SizedBox(height: VitrioSpace.sp16),
          VitrioSwitchRow(
            label: l10n.providerServiceOnQuote,
            description: l10n.providerServiceOnQuoteHint,
            value: _onQuote,
            onChanged: (value) => setState(() => _onQuote = value),
          ),
          if (!_onQuote) ...[
            const SizedBox(height: VitrioSpace.sp16),
            VitrioTextField(
              label: l10n.providerServicePrice,
              controller: _price,
              prefix: r'R$',
              keyboardType: const TextInputType.numberWithOptions(decimal: true),
            ),
            const SizedBox(height: VitrioSpace.sp16),
            Text(l10n.providerServiceUnit, style: VitrioText.label.copyWith(color: c.ink)),
            const SizedBox(height: VitrioSpace.sp8),
            Wrap(
              spacing: VitrioSpace.sp8,
              runSpacing: VitrioSpace.sp8,
              children: [
                for (final unit in PriceUnit.values)
                  VitrioChip(
                    label: unitLabel(l10n, unit),
                    selected: _unit == unit,
                    style: VitrioChipStyle.soft,
                    onSelected: (_) => setState(() => _unit = unit),
                  ),
              ],
            ),
          ],
          const SizedBox(height: VitrioSpace.sp24),
          VitrioButton(label: l10n.commonSave, onPressed: _done),
          if (widget.service != null) ...[
            const SizedBox(height: VitrioSpace.sp8),
            VitrioButton(
              label: l10n.providerServiceRemove,
              variant: VitrioButtonVariant.danger,
              onPressed: () => Navigator.of(context).pop((service: null)),
            ),
          ],
        ],
      ),
    );
  }
}
