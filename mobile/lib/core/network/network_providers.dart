import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:http/http.dart' as http;
import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../app/app_config.dart';
import '../session/secure_token_store.dart';
import '../session/session_expiry.dart';
import '../session/token_store.dart';
import 'api_client.dart';

part 'network_providers.g.dart';

@Riverpod(keepAlive: true)
TokenStore tokenStore(Ref ref) => const SecureTokenStore(FlutterSecureStorage());

@Riverpod(keepAlive: true)
http.Client httpClient(Ref ref) {
  final client = http.Client();
  ref.onDispose(client.close);
  return client;
}

@Riverpod(keepAlive: true)
ApiClient apiClient(Ref ref) => ApiClient(
      httpClient: ref.watch(httpClientProvider),
      baseUrl: ref.watch(appConfigProvider).apiBaseUrl,
      tokenStore: ref.watch(tokenStoreProvider),
      onSessionExpired: () => ref.read(sessionExpiryProvider.notifier).notify(),
    );
