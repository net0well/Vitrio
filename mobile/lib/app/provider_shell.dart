import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../core/design_system/widgets/vitrio_icon.dart';
import '../core/design_system/widgets/vitrio_icon_name.dart';
import '../l10n/app_localizations.dart';

class ProviderShell extends StatelessWidget {
  const ProviderShell({super.key, required this.navigationShell});

  final StatefulNavigationShell navigationShell;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    return Scaffold(
      body: navigationShell,
      bottomNavigationBar: NavigationBar(
        selectedIndex: navigationShell.currentIndex,
        onDestinationSelected: (index) =>
            navigationShell.goBranch(index, initialLocation: index == navigationShell.currentIndex),
        destinations: [
          NavigationDestination(icon: const VitrioIcon(VitrioIconName.chart), label: l10n.tabDashboard),
          NavigationDestination(icon: const VitrioIcon(VitrioIconName.grid), label: l10n.tabMyProfile),
          NavigationDestination(icon: const VitrioIcon(VitrioIconName.user), label: l10n.tabAccount),
        ],
      ),
    );
  }
}
