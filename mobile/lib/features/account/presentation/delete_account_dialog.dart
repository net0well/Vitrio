import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/widgets/vitrio_button.dart';
import '../../../core/design_system/widgets/vitrio_button_size.dart';
import '../../../core/design_system/widgets/vitrio_button_variant.dart';
import '../../../core/design_system/widgets/vitrio_text_field.dart';
import '../../../core/network/api_result.dart';
import '../../../core/presentation/problem_message.dart';
import '../../../l10n/app_localizations.dart';
import '../../auth/presentation/session_controller.dart';

class DeleteAccountDialog extends ConsumerStatefulWidget {
  const DeleteAccountDialog({super.key});

  @override
  ConsumerState<DeleteAccountDialog> createState() => _DeleteAccountDialogState();
}

class _DeleteAccountDialogState extends ConsumerState<DeleteAccountDialog> {
  final _confirmation = TextEditingController();
  bool _busy = false;
  String? _error;

  @override
  void dispose() {
    _confirmation.dispose();
    super.dispose();
  }

  Future<void> _delete() async {
    setState(() => _busy = true);
    final result = await ref.read(sessionControllerProvider.notifier).deleteAccount();
    if (!mounted) {
      return;
    }

    switch (result) {
      case ApiSuccess():
        Navigator.of(context).pop();
      case ApiFailure(:final problem):
        setState(() {
          _busy = false;
          _error = problemMessage(AppLocalizations.of(context), problem);
        });
    }
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    return AlertDialog(
      title: Text(l10n.accountDeleteTitle),
      content: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Text(l10n.accountDeleteMessage),
          const SizedBox(height: VitrioSpace.sp16),
          VitrioTextField(
            label: l10n.accountDeleteWord,
            showLabel: false,
            controller: _confirmation,
            placeholder: l10n.accountDeleteWord,
            onChanged: (_) => setState(() {}),
            error: _error,
          ),
        ],
      ),
      actions: [
        VitrioButton(
          label: l10n.commonCancel,
          variant: VitrioButtonVariant.ghost,
          size: VitrioButtonSize.md,
          onPressed: () => Navigator.of(context).pop(),
        ),
        VitrioButton(
          label: l10n.accountDeleteConfirm,
          variant: VitrioButtonVariant.danger,
          size: VitrioButtonSize.md,
          busy: _busy,
          onPressed: _confirmation.text.trim() != l10n.accountDeleteWord ? null : _delete,
        ),
      ],
    );
  }
}
