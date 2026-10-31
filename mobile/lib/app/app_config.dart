import 'package:flutter/foundation.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';
import 'flavor.dart';

part 'app_config.g.dart';

final class AppConfig {
  const AppConfig({required this.flavor, required this.apiBaseUrl});

  factory AppConfig.forFlavor(Flavor flavor) {
    const override = String.fromEnvironment('API_BASE_URL');
    final apiBaseUrl = override.isNotEmpty
        ? override
        : switch (flavor) {
            Flavor.dev => _localApiUrl(),
            Flavor.staging => 'https://api-staging.vitrio.app',
            Flavor.prod => 'https://api.vitrio.app',
          };

    return AppConfig(flavor: flavor, apiBaseUrl: Uri.parse(apiBaseUrl));
  }

  static const playStoreUrl = 'https://play.google.com/store/apps/details?id=app.vitrio';

  final Flavor flavor;
  final Uri apiBaseUrl;

  // The Android emulator reaches the host machine through 10.0.2.2, not localhost.
  static String _localApiUrl() =>
      defaultTargetPlatform == TargetPlatform.android ? 'http://10.0.2.2:5080' : 'http://localhost:5080';
}

@Riverpod(keepAlive: true)
AppConfig appConfig(Ref ref) => throw UnimplementedError('appConfigProvider must be overridden at startup.');
