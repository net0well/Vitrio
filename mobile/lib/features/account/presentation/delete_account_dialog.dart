import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/network/api_result.dart';
import '../../../l10n/app_localizations.dart';
import '../../auth/presentation/session_controller.dart';
import '../../auth/presentation/widgets/problem_message.dart';

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
          TextField(
            controller: _confirmation,
            onChanged: (_) => setState(() {}),
            decoration: InputDecoration(hintText: l10n.accountDeleteWord, errorText: _error),
          ),
        ],
      ),
      actions: [
        TextButton(onPressed: () => Navigator.of(context).pop(), child: Text(l10n.commonCancel)),
        FilledButton(
          style: FilledButton.styleFrom(backgroundColor: Theme.of(context).colorScheme.error),
          onPressed: _busy || _confirmation.text.trim() != l10n.accountDeleteWord ? null : _delete,
          child: Text(l10n.accountDeleteConfirm),
        ),
      ],
    );
  }
}
