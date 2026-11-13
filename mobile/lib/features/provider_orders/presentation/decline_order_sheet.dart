import 'package:flutter/material.dart';

import '../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/design_system/widgets/vitrio_button.dart';
import '../../../core/design_system/widgets/vitrio_button_variant.dart';
import '../../../core/design_system/widgets/vitrio_choice_list.dart';
import '../../../core/design_system/widgets/vitrio_option.dart';
import '../../../core/design_system/widgets/vitrio_text_field.dart';
import '../../../l10n/app_localizations.dart';
import '../../orders/domain/decline_reason.dart';
import '../../orders/presentation/order_labels.dart';
import 'decline_choice.dart';

class DeclineOrderSheet extends StatefulWidget {
  const DeclineOrderSheet({super.key, required this.customerFirstName, required this.providerFirstName});

  static const maxDetails = 200;

  final String customerFirstName;
  final String providerFirstName;

  static Future<DeclineChoice?> show(BuildContext context, {required String customerFirstName, required String providerFirstName}) =>
      showModalBottomSheet<DeclineChoice>(
        context: context,
        isScrollControlled: true,
        useSafeArea: true,
        useRootNavigator: true,
        showDragHandle: true,
        builder: (_) => DeclineOrderSheet(customerFirstName: customerFirstName, providerFirstName: providerFirstName),
      );

  @override
  State<DeclineOrderSheet> createState() => _DeclineOrderSheetState();
}

class _DeclineOrderSheetState extends State<DeclineOrderSheet> {
  final _details = TextEditingController();
  DeclineReason? _reason;
  String? _error;

  @override
  void dispose() {
    _details.dispose();
    super.dispose();
  }

  void _confirm(AppLocalizations l10n) {
    final details = _details.text.trim();
    final error = switch (_reason) {
      null => l10n.cancelOrderChooseReason,
      DeclineReason.other when details.isEmpty => l10n.declineOrderDetailsRequired,
      _ => null,
    };
    if (error != null) {
      setState(() => _error = error);
      return;
    }

    final reason = _reason!;
    Navigator.of(context).pop<DeclineChoice>((reason: reason, details: reason == DeclineReason.other ? details : null));
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final other = _reason == DeclineReason.other;
    return SingleChildScrollView(
      padding: EdgeInsets.fromLTRB(VitrioSpace.sp20, 0, VitrioSpace.sp20, MediaQuery.viewInsetsOf(context).bottom + VitrioSpace.sp28),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Semantics(
            header: true,
            child: Text(l10n.declineOrderTitle, style: VitrioText.title.copyWith(color: c.ink)),
          ),
          const SizedBox(height: VitrioSpace.sp6),
          Text(
            l10n.declineOrderSubtitle(widget.customerFirstName, widget.providerFirstName),
            style: VitrioText.body.copyWith(height: 1.5, color: c.ink2),
          ),
          const SizedBox(height: VitrioSpace.sp16),
          VitrioChoiceList<DeclineReason>(
            label: l10n.cancelOrderReasons,
            value: _reason,
            onChanged: (reason) => setState(() {
              _reason = reason;
              _error = null;
            }),
            options: [for (final reason in DeclineReason.values) VitrioOption(value: reason, label: declineReasonLabel(l10n, reason))],
          ),
          if (other) ...[
            const SizedBox(height: VitrioSpace.sp16),
            VitrioTextField(
              label: l10n.declineOrderDetails,
              controller: _details,
              placeholder: l10n.declineOrderPlaceholder(widget.customerFirstName),
              hint: l10n.declineOrderHint,
              maxLength: DeclineOrderSheet.maxDetails,
              minLines: 3,
              maxLines: 4,
              textCapitalization: TextCapitalization.sentences,
              error: _error,
              onChanged: (_) => setState(() => _error = null),
            ),
          ] else if (_error != null) ...[
            const SizedBox(height: VitrioSpace.sp8),
            Semantics(
              liveRegion: true,
              child: Text(_error!, style: VitrioText.weighted(VitrioText.caption, 600).copyWith(color: c.danger)),
            ),
          ],
          const SizedBox(height: VitrioSpace.sp16),
          VitrioButton(label: l10n.declineOrderConfirm, variant: VitrioButtonVariant.danger, onPressed: () => _confirm(l10n)),
          const SizedBox(height: VitrioSpace.sp6),
          VitrioButton(label: l10n.commonBack, variant: VitrioButtonVariant.ghost, onPressed: () => Navigator.of(context).pop()),
        ],
      ),
    );
  }
}
