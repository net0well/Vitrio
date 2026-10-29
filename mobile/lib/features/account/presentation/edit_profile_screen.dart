import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/network/api_result.dart';
import '../../../core/network/problem_details.dart';
import '../../../core/presentation/form_page.dart';
import '../../../core/presentation/problem_message.dart';
import '../../../l10n/app_localizations.dart';
import '../../auth/data/auth_providers.dart';
import '../../auth/presentation/session_controller.dart';

class EditProfileScreen extends ConsumerStatefulWidget {
  const EditProfileScreen({super.key});

  @override
  ConsumerState<EditProfileScreen> createState() => _EditProfileScreenState();
}

class _EditProfileScreenState extends ConsumerState<EditProfileScreen> {
  late final TextEditingController _name;
  late final TextEditingController _phone;
  bool _busy = false;
  ProblemDetails? _problem;

  @override
  void initState() {
    super.initState();
    final me = ref.read(sessionControllerProvider).value;
    _name = TextEditingController(text: me?.displayName);
    _phone = TextEditingController(text: me?.phoneNumber);
  }

  @override
  void dispose() {
    _name.dispose();
    _phone.dispose();
    super.dispose();
  }

  Future<void> _save() async {
    setState(() {
      _busy = true;
      _problem = null;
    });

    final result = await ref.read(accountRepositoryProvider).updateMe(displayName: _name.text.trim(), phoneNumber: _phone.text.trim());
    if (!mounted) {
      return;
    }

    switch (result) {
      case ApiSuccess(:final data):
        ref.read(sessionControllerProvider.notifier).replaceProfile(data);
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(AppLocalizations.of(context).profileSaved)));
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
      title: l10n.accountEditProfile,
      actionLabel: l10n.commonSave,
      busy: _busy,
      onAction: _save,
      message: _problem == null || _problem!.isValidation ? null : problemMessage(l10n, _problem!),
      children: [
        TextField(
          controller: _name,
          textCapitalization: TextCapitalization.words,
          decoration: InputDecoration(labelText: l10n.authName, errorText: fieldError(_problem, 'displayName')),
        ),
        const SizedBox(height: VitrioSpace.sp16),
        TextField(
          controller: _phone,
          keyboardType: TextInputType.phone,
          autofillHints: const [AutofillHints.telephoneNumber],
          decoration: InputDecoration(
            labelText: l10n.profilePhone,
            hintText: l10n.profilePhoneHint,
            errorText: fieldError(_problem, 'phoneNumber'),
          ),
        ),
      ],
    );
  }
}
