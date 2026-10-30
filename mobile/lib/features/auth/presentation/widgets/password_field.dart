import 'package:flutter/material.dart';

import '../../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../../core/design_system/tokens/vitrio_space.dart';
import '../../../../core/design_system/tokens/vitrio_text.dart';
import '../../../../core/design_system/widgets/vitrio_text_field.dart';
import '../../../../l10n/app_localizations.dart';

class PasswordField extends StatefulWidget {
  const PasswordField({
    super.key,
    required this.controller,
    required this.label,
    this.errorText,
    this.helperText,
    this.newPassword = false,
  });

  final TextEditingController controller;
  final String label;
  final String? errorText;
  final String? helperText;
  final bool newPassword;

  @override
  State<PasswordField> createState() => _PasswordFieldState();
}

class _PasswordFieldState extends State<PasswordField> {
  bool _visible = false;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    return VitrioTextField(
      label: widget.label,
      controller: widget.controller,
      obscureText: !_visible,
      autocorrect: false,
      enableSuggestions: false,
      autofillHints: [widget.newPassword ? AutofillHints.newPassword : AutofillHints.password],
      error: widget.errorText,
      hint: widget.helperText,
      suffix: Padding(
        padding: const EdgeInsetsDirectional.only(end: VitrioSpace.sp6),
        child: Semantics(
          label: _visible ? l10n.authHidePassword : l10n.authShowPassword,
          excludeSemantics: true,
          button: true,
          child: TextButton(
            onPressed: () => setState(() => _visible = !_visible),
            child: Text(_visible ? l10n.authHide : l10n.authShow, style: VitrioText.label.copyWith(color: c.plum)),
          ),
        ),
      ),
    );
  }
}
