import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

import '../core/design_system/widgets/vitrio_icon.dart';
import '../l10n/app_localizations.dart';

class ClientShell extends StatelessWidget {
  const ClientShell({super.key, required this.navigationShell});

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
          NavigationDestination(icon: const VitrioIcon(VitrioIconName.home), label: l10n.tabHome),
          NavigationDestination(icon: const VitrioIcon(VitrioIconName.search), label: l10n.tabSearch),
          NavigationDestination(icon: const VitrioIcon(VitrioIconName.orders), label: l10n.tabOrders),
          NavigationDestination(icon: const VitrioIcon(VitrioIconName.user), label: l10n.tabAccount),
        ],
      ),
    );
  }
}
