import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/routes.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/network/api_result.dart';
import '../../../core/network/problem_details.dart';
import '../../../core/presentation/form_page.dart';
import '../../../core/presentation/problem_message.dart';
import '../../../l10n/app_localizations.dart';
import '../data/auth_providers.dart';
import '../domain/email_code_purpose.dart';
import '../domain/pending_credentials.dart';
import 'pending_verification.dart';
import 'session_controller.dart';
import 'widgets/password_field.dart';

class SignInScreen extends ConsumerStatefulWidget {
  const SignInScreen({super.key});

  @override
  ConsumerState<SignInScreen> createState() => _SignInScreenState();
}

class _SignInScreenState extends ConsumerState<SignInScreen> {
  static const _emailNotVerified = 'account.email_not_verified';

  final _email = TextEditingController();
  final _password = TextEditingController();
  bool _busy = false;
  ProblemDetails? _problem;

  @override
  void dispose() {
    _email.dispose();
    _password.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    setState(() {
      _busy = true;
      _problem = null;
    });

    final email = _email.text.trim();
    final result = await ref.read(authRepositoryProvider).login(email: email, password: _password.text);
    if (!mounted) {
      return;
    }

    switch (result) {
      case ApiSuccess(:final data):
        await ref.read(sessionControllerProvider.notifier).start(data);
        if (mounted) {
          const AccountRoute().go(context);
        }
      case ApiFailure(:final problem) when problem.code == _emailNotVerified:
        ref.read(pendingVerificationProvider.notifier).remember(PendingCredentials(email: email, password: _password.text));
        await ref.read(authRepositoryProvider).resendCode(email: email, purpose: EmailCodePurpose.verifyEmail);
        if (mounted) {
          setState(() => _busy = false);
          await const VerifyEmailRoute().push<void>(context);
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
      title: l10n.authSignIn,
      actionLabel: l10n.authSignIn,
      busy: _busy,
      onAction: _submit,
      message: _problem == null || _problem!.isValidation ? null : problemMessage(l10n, _problem!),
      children: [
        TextField(
          controller: _email,
          keyboardType: TextInputType.emailAddress,
          autocorrect: false,
          autofillHints: const [AutofillHints.email],
          decoration: InputDecoration(labelText: l10n.authEmail, errorText: fieldError(_problem, 'email')),
        ),
        const SizedBox(height: VitrioSpace.sp16),
        PasswordField(controller: _password, label: l10n.authPassword, errorText: fieldError(_problem, 'password')),
        const SizedBox(height: VitrioSpace.sp8),
        Align(
          alignment: AlignmentDirectional.centerStart,
          child: TextButton(onPressed: () => const ForgotPasswordRoute().push<void>(context), child: Text(l10n.authForgotPassword)),
        ),
        Align(
          alignment: AlignmentDirectional.centerStart,
          child: TextButton(onPressed: () => const SignUpRoute().push<void>(context), child: Text(l10n.authCreateAccount)),
        ),
      ],
    );
  }
}
