import 'package:flutter/widgets.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';

import '../core/storage/preferences.dart';
import 'app.dart';
import 'app_config.dart';
import 'flavor.dart';

Future<void> bootstrap(Flavor flavor) async {
  WidgetsFlutterBinding.ensureInitialized();
  final preferences = await SharedPreferences.getInstance();

  runApp(
    ProviderScope(
      overrides: [
        appConfigProvider.overrideWithValue(AppConfig.forFlavor(flavor)),
        preferencesProvider.overrideWithValue(preferences),
      ],
      child: const VitrioApp(),
    ),
  );
}
