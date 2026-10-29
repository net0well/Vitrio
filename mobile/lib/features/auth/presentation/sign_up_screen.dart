import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/routes.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/network/api_result.dart';
import '../../../core/network/problem_details.dart';
import '../../../l10n/app_localizations.dart';
import '../data/auth_providers.dart';
import '../domain/pending_credentials.dart';
import '../domain/terms.dart';
import 'pending_verification.dart';
import 'widgets/form_page.dart';
import 'widgets/password_field.dart';
import 'widgets/problem_message.dart';

class SignUpScreen extends ConsumerStatefulWidget {
  const SignUpScreen({super.key});

  @override
  ConsumerState<SignUpScreen> createState() => _SignUpScreenState();
}

class _SignUpScreenState extends ConsumerState<SignUpScreen> {
  final _name = TextEditingController();
  final _email = TextEditingController();
  final _password = TextEditingController();
  bool _acceptedTerms = false;
  bool _busy = false;
  bool _termsMissing = false;
  ProblemDetails? _problem;

  @override
  void dispose() {
    _name.dispose();
    _email.dispose();
    _password.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_acceptedTerms) {
      setState(() => _termsMissing = true);
      return;
    }

    setState(() {
      _busy = true;
      _problem = null;
    });

    final email = _email.text.trim();
    final result = await ref.read(authRepositoryProvider).register(
          displayName: _name.text.trim(),
          email: email,
          password: _password.text,
          acceptedTermsVersion: Terms.currentVersion,
        );
    if (!mounted) {
      return;
    }

    switch (result) {
      case ApiSuccess():
        ref.read(pendingVerificationProvider.notifier).remember(PendingCredentials(email: email, password: _password.text));
        setState(() => _busy = false);
        await const VerifyEmailRoute().push<void>(context);
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
      title: l10n.authCreateAccount,
      actionLabel: l10n.authCreateAccount,
      busy: _busy,
      onAction: _submit,
      message: _termsMissing && !_acceptedTerms
          ? l10n.authAcceptTermsRequired
          : _problem == null || _problem!.isValidation
              ? null
              : problemMessage(l10n, _problem!),
      children: [
        TextField(
          controller: _name,
          textCapitalization: TextCapitalization.words,
          autofillHints: const [AutofillHints.name],
          decoration: InputDecoration(labelText: l10n.authName, errorText: fieldError(_problem, 'displayName')),
        ),
        const SizedBox(height: VitrioSpace.sp16),
        TextField(
          controller: _email,
          keyboardType: TextInputType.emailAddress,
          autocorrect: false,
          autofillHints: const [AutofillHints.email],
          decoration: InputDecoration(labelText: l10n.authEmail, errorText: fieldError(_problem, 'email')),
        ),
        const SizedBox(height: VitrioSpace.sp16),
        PasswordField(
          controller: _password,
          label: l10n.authPassword,
          newPassword: true,
          helperText: l10n.authPasswordRule,
          errorText: fieldError(_problem, 'password'),
        ),
        const SizedBox(height: VitrioSpace.sp8),
        CheckboxListTile(
          value: _acceptedTerms,
          onChanged: (value) => setState(() => _acceptedTerms = value ?? false),
          controlAffinity: ListTileControlAffinity.leading,
          contentPadding: EdgeInsets.zero,
          title: Text(l10n.authAcceptTerms),
        ),
      ],
    );
  }
}
