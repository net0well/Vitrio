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
import '../domain/cancellation_reason.dart';
import 'cancel_choice.dart';
import 'order_labels.dart';

class CancelOrderSheet extends StatefulWidget {
  const CancelOrderSheet({super.key, required this.otherFirstName, this.reasons = CancellationReason.customerReasons});

  static const maxDetails = 200;

  final String otherFirstName;
  final List<CancellationReason> reasons;

  static Future<CancelChoice?> show(
    BuildContext context, {
    required String otherFirstName,
    List<CancellationReason> reasons = CancellationReason.customerReasons,
  }) =>
      showModalBottomSheet<CancelChoice>(
        context: context,
        isScrollControlled: true,
        useSafeArea: true,
        useRootNavigator: true,
        showDragHandle: true,
        builder: (_) => CancelOrderSheet(otherFirstName: otherFirstName, reasons: reasons),
      );

  @override
  State<CancelOrderSheet> createState() => _CancelOrderSheetState();
}

class _CancelOrderSheetState extends State<CancelOrderSheet> {
  final _details = TextEditingController();
  CancellationReason? _reason;
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
      CancellationReason.other when details.isEmpty => l10n.cancelOrderDetailsRequired,
      _ => null,
    };
    if (error != null) {
      setState(() => _error = error);
      return;
    }

    Navigator.of(context).pop<CancelChoice>((reason: _reason!, details: details.isEmpty ? null : details));
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final other = _reason == CancellationReason.other;
    return SingleChildScrollView(
      padding: EdgeInsets.fromLTRB(VitrioSpace.sp20, 0, VitrioSpace.sp20, MediaQuery.viewInsetsOf(context).bottom + VitrioSpace.sp28),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Semantics(header: true, child: Text(l10n.cancelOrderTitle, style: VitrioText.title.copyWith(color: c.ink))),
          const SizedBox(height: VitrioSpace.sp6),
          Text(
            l10n.cancelOrderSubtitle(widget.otherFirstName),
            style: VitrioText.body.copyWith(height: 1.5, color: c.ink2),
          ),
          const SizedBox(height: VitrioSpace.sp16),
          VitrioChoiceList<CancellationReason>(
            label: l10n.cancelOrderReasons,
            value: _reason,
            onChanged: (reason) => setState(() {
              _reason = reason;
              _error = null;
            }),
            options: [
              for (final reason in widget.reasons)
                VitrioOption(value: reason, label: cancellationReasonLabel(l10n, reason)),
            ],
          ),
          const SizedBox(height: VitrioSpace.sp16),
          VitrioTextField(
            label: other ? l10n.cancelOrderDetailsRequired : l10n.cancelOrderDetailsOptional,
            controller: _details,
            placeholder: l10n.cancelOrderDetailsPlaceholder,
            maxLength: CancelOrderSheet.maxDetails,
            minLines: 3,
            maxLines: 4,
            textCapitalization: TextCapitalization.sentences,
            error: _error,
            onChanged: (_) => setState(() => _error = null),
          ),
          const SizedBox(height: VitrioSpace.sp12),
          VitrioButton(label: l10n.cancelOrderConfirm, variant: VitrioButtonVariant.danger, onPressed: () => _confirm(l10n)),
          const SizedBox(height: VitrioSpace.sp6),
          VitrioButton(label: l10n.commonBack, variant: VitrioButtonVariant.ghost, onPressed: () => Navigator.of(context).pop()),
        ],
      ),
    );
  }
}
