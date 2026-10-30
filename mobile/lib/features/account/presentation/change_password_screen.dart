import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/network/api_result.dart';
import '../../../core/network/problem_details.dart';
import '../../../core/presentation/form_page.dart';
import '../../../core/presentation/problem_message.dart';
import '../../../l10n/app_localizations.dart';
import '../../auth/data/auth_providers.dart';
import '../../auth/presentation/widgets/password_field.dart';

class ChangePasswordScreen extends ConsumerStatefulWidget {
  const ChangePasswordScreen({super.key});

  @override
  ConsumerState<ChangePasswordScreen> createState() => _ChangePasswordScreenState();
}

class _ChangePasswordScreenState extends ConsumerState<ChangePasswordScreen> {
  final _current = TextEditingController();
  final _new = TextEditingController();
  bool _busy = false;
  ProblemDetails? _problem;

  @override
  void dispose() {
    _current.dispose();
    _new.dispose();
    super.dispose();
  }

  Future<void> _save() async {
    setState(() {
      _busy = true;
      _problem = null;
    });

    final result = await ref.read(accountRepositoryProvider).changePassword(currentPassword: _current.text, newPassword: _new.text);
    if (!mounted) {
      return;
    }

    switch (result) {
      case ApiSuccess():
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(AppLocalizations.of(context).passwordChanged)));
        Navigator.of(context).pop();
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
    return FormPage(
      title: l10n.accountChangePassword,
      actionLabel: l10n.commonSave,
      busy: _busy,
      onAction: _save,
      message: _problem == null || _problem!.isValidation ? null : problemMessage(l10n, _problem!),
      children: [
        PasswordField(controller: _current, label: l10n.passwordCurrent, errorText: fieldError(_problem, 'currentPassword')),
        const SizedBox(height: VitrioSpace.sp16),
        PasswordField(
          controller: _new,
          label: l10n.authNewPassword,
          newPassword: true,
          helperText: l10n.authPasswordRule,
          errorText: fieldError(_problem, 'newPassword'),
        ),
      ],
    );
  }
}
