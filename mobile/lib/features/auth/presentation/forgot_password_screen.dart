import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/routes.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/network/api_result.dart';
import '../../../core/network/problem_details.dart';
import '../../../l10n/app_localizations.dart';
import '../data/auth_providers.dart';
import 'session_controller.dart';
import 'widgets/form_page.dart';
import 'widgets/password_field.dart';
import 'widgets/problem_message.dart';

class ForgotPasswordScreen extends ConsumerStatefulWidget {
  const ForgotPasswordScreen({super.key});

  @override
  ConsumerState<ForgotPasswordScreen> createState() => _ForgotPasswordScreenState();
}

class _ForgotPasswordScreenState extends ConsumerState<ForgotPasswordScreen> {
  final _email = TextEditingController();
  final _code = TextEditingController();
  final _password = TextEditingController();
  bool _codeSent = false;
  bool _busy = false;
  ProblemDetails? _problem;

  @override
  void dispose() {
    _email.dispose();
    _code.dispose();
    _password.dispose();
    super.dispose();
  }

  Future<void> _sendCode() async {
    setState(() {
      _busy = true;
      _problem = null;
    });

    final result = await ref.read(authRepositoryProvider).forgotPassword(email: _email.text.trim());
    if (!mounted) {
      return;
    }

    setState(() {
      _busy = false;
      switch (result) {
        case ApiSuccess():
          _codeSent = true;
        case ApiFailure(:final problem):
          _problem = problem;
      }
    });
  }

  Future<void> _reset() async {
    setState(() {
      _busy = true;
      _problem = null;
    });

    final result = await ref.read(authRepositoryProvider).resetPassword(
          email: _email.text.trim(),
          code: _code.text.trim(),
          newPassword: _password.text,
        );
    if (!mounted) {
      return;
    }

    switch (result) {
      case ApiSuccess(:final data):
        await ref.read(sessionControllerProvider.notifier).start(data);
        if (mounted) {
          const AccountRoute().go(context);
        }
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
      title: l10n.authForgotTitle,
      actionLabel: _codeSent ? l10n.authSaveNewPassword : l10n.authSendCode,
      busy: _busy,
      onAction: _codeSent ? _reset : _sendCode,
      message: _problem == null || _problem!.isValidation ? null : problemMessage(l10n, _problem!),
      children: [
        Text(_codeSent ? l10n.authCodeSentIfExists : l10n.authForgotMessage, style: VitrioText.body),
        const SizedBox(height: VitrioSpace.sp20),
        TextField(
          controller: _email,
          enabled: !_codeSent,
          keyboardType: TextInputType.emailAddress,
          autocorrect: false,
          autofillHints: const [AutofillHints.email],
          decoration: InputDecoration(labelText: l10n.authEmail, errorText: fieldError(_problem, 'email')),
        ),
        if (_codeSent) ...[
          const SizedBox(height: VitrioSpace.sp16),
          TextField(
            controller: _code,
            keyboardType: TextInputType.number,
            maxLength: 6,
            autofillHints: const [AutofillHints.oneTimeCode],
            inputFormatters: [FilteringTextInputFormatter.digitsOnly],
            decoration: InputDecoration(labelText: l10n.authCode, errorText: fieldError(_problem, 'code')),
          ),
          const SizedBox(height: VitrioSpace.sp8),
          PasswordField(
            controller: _password,
            label: l10n.authNewPassword,
            newPassword: true,
            helperText: l10n.authPasswordRule,
            errorText: fieldError(_problem, 'newPassword'),
          ),
        ],
      ],
    );
  }
}
