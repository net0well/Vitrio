import 'package:flutter/material.dart';

import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/widgets/vitrio_empty_state.dart';
import '../../../core/design_system/widgets/vitrio_icon.dart';
import '../../../l10n/app_localizations.dart';

class AccountScreen extends StatelessWidget {
  const AccountScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    return Scaffold(
      appBar: AppBar(title: Text(l10n.accountTitle)),
      body: Center(
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp20),
          child: VitrioEmptyState(icon: VitrioIconName.user, message: l10n.accountEmpty),
        ),
      ),
    );
  }
}
