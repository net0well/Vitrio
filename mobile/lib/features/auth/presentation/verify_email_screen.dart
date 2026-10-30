import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/routes.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/design_system/widgets/vitrio_text_field.dart';
import '../../../core/network/api_result.dart';
import '../../../core/network/problem_details.dart';
import '../../../core/presentation/form_page.dart';
import '../../../core/presentation/problem_message.dart';
import '../../../l10n/app_localizations.dart';
import '../data/auth_providers.dart';
import '../domain/email_code_purpose.dart';
import 'go_after_sign_in.dart';
import 'pending_verification.dart';
import 'session_controller.dart';

class VerifyEmailScreen extends ConsumerStatefulWidget {
  const VerifyEmailScreen({super.key});

  @override
  ConsumerState<VerifyEmailScreen> createState() => _VerifyEmailScreenState();
}

class _VerifyEmailScreenState extends ConsumerState<VerifyEmailScreen> {
  static const _resendInterval = 60;

  final _code = TextEditingController();
  Timer? _timer;
  int _secondsToResend = _resendInterval;
  bool _busy = false;
  String? _notice;
  ProblemDetails? _problem;

  @override
  void initState() {
    super.initState();
    _startCountdown(_resendInterval);
  }

  @override
  void dispose() {
    _timer?.cancel();
    _code.dispose();
    super.dispose();
  }

  void _startCountdown(int seconds) {
    _timer?.cancel();
    setState(() => _secondsToResend = seconds);
    _timer = Timer.periodic(const Duration(seconds: 1), (timer) {
      if (_secondsToResend <= 1) {
        timer.cancel();
      }

      setState(() => _secondsToResend--);
    });
  }

  Future<void> _confirm() async {
    final pending = ref.read(pendingVerificationProvider);
    if (pending == null) {
      return;
    }

    setState(() {
      _busy = true;
      _problem = null;
      _notice = null;
    });

    final result = await ref.read(authRepositoryProvider).verifyEmail(
          email: pending.email,
          code: _code.text.trim(),
          password: pending.password,
        );
    if (!mounted) {
      return;
    }

    switch (result) {
      case ApiSuccess(:final data):
        await ref.read(sessionControllerProvider.notifier).start(data);
        ref.read(pendingVerificationProvider.notifier).clear();
        if (mounted) {
          goAfterSignIn(context, ref);
        }
      case ApiFailure(:final problem):
        setState(() {
          _busy = false;
          _problem = problem;
        });
    }
  }

  Future<void> _resend() async {
    final pending = ref.read(pendingVerificationProvider);
    if (pending == null) {
      return;
    }

    final result = await ref.read(authRepositoryProvider).resendCode(email: pending.email, purpose: EmailCodePurpose.verifyEmail);
    if (!mounted) {
      return;
    }

    final l10n = AppLocalizations.of(context);
    switch (result) {
      case ApiSuccess():
        setState(() {
          _notice = l10n.authCodeResent;
          _problem = null;
        });
        _startCountdown(_resendInterval);
      case ApiFailure(:final problem):
        setState(() => _problem = problem);
    }
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final pending = ref.watch(pendingVerificationProvider);
    return FormPage(
      title: l10n.authVerifyTitle,
      actionLabel: l10n.authConfirm,
      busy: _busy,
      onAction: pending == null ? null : _confirm,
      message: _problem == null || _problem!.isValidation ? null : problemMessage(l10n, _problem!),
      children: [
        Text(l10n.authVerifyMessage(pending?.email ?? ''), style: VitrioText.body),
        const SizedBox(height: VitrioSpace.sp20),
        VitrioTextField(
          label: l10n.authCode,
          controller: _code,
          keyboardType: TextInputType.number,
          maxLength: 6,
          autofillHints: const [AutofillHints.oneTimeCode],
          inputFormatters: [FilteringTextInputFormatter.digitsOnly],
          inputStyle: VitrioText.priceLg,
          textAlign: TextAlign.center,
          error: fieldError(_problem, 'code'),
        ),
        if (_notice != null) Text(_notice!, style: VitrioText.meta),
        const SizedBox(height: VitrioSpace.sp8),
        Align(
          alignment: AlignmentDirectional.centerStart,
          child: TextButton(
            onPressed: _secondsToResend > 0 ? null : _resend,
            child: Text(_secondsToResend > 0 ? l10n.authResendIn(_secondsToResend) : l10n.authResendCode),
          ),
        ),
        Align(
          alignment: AlignmentDirectional.centerStart,
          child: TextButton(
            onPressed: () {
              ref.read(pendingVerificationProvider.notifier).clear();
              const SignUpRoute().go(context);
            },
            child: Text(l10n.authChangeEmail),
          ),
        ),
      ],
    );
  }
}
