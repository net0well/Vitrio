import 'package:flutter/material.dart';

import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/widgets/vitrio_empty_state.dart';
import '../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../core/design_system/widgets/vitrio_top_bar.dart';
import '../../../l10n/app_localizations.dart';

class SearchScreen extends StatelessWidget {
  const SearchScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    return Scaffold(
      appBar: VitrioTopBar(title: l10n.searchTitle),
      body: Center(
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp20),
          child: VitrioEmptyState(icon: VitrioIconName.search, message: l10n.searchEmpty),
        ),
      ),
    );
  }
}
