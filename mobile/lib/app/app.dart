import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../core/design_system/vitrio_theme.dart';
import '../l10n/app_localizations.dart';
import 'router.dart';

final _lightTheme = vitrioTheme(Brightness.light);
final _darkTheme = vitrioTheme(Brightness.dark);

class VitrioApp extends ConsumerWidget {
  const VitrioApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return MaterialApp.router(
      onGenerateTitle: (context) => AppLocalizations.of(context).appTitle,
      debugShowCheckedModeBanner: false,
      theme: _lightTheme,
      darkTheme: _darkTheme,
      localizationsDelegates: AppLocalizations.localizationsDelegates,
      supportedLocales: AppLocalizations.supportedLocales,
      routerConfig: ref.watch(routerProvider),
    );
  }
}
