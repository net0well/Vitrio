import 'dart:convert';

import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:vitrio/core/network/api_client.dart';
import 'package:vitrio/core/network/api_result.dart';
import 'package:vitrio/features/auth/data/http_auth_repository.dart';
import 'package:vitrio/features/auth/domain/auth_session.dart';

import '../../../support/memory_token_store.dart';

void main() {
  late List<http.Request> requests;

  HttpAuthRepository repositoryReplying(int status, Object? body) {
    requests = [];
    final client = MockClient((request) async {
      requests.add(request);
      return http.Response(body == null ? '' : jsonEncode(body), status, headers: {'content-type': 'application/json'});
    });
    return HttpAuthRepository(ApiClient(httpClient: client, baseUrl: Uri.parse('https://api.test'), tokenStore: MemoryTokenStore()));
  }

  const signedIn = {
    'accessToken': 'access-1',
    'expiresIn': 600,
    'refreshToken': 'refresh-1',
    'user': {
      'id': '0199a0c4-3f2e-7c1a-9d7e-2a4b6c8d0e1f',
      'displayName': 'Mariana Souza',
      'initials': 'MS',
      'email': 'mariana@exemplo.com.br',
      'phoneNumber': null,
      'roles': ['customer'],
      'hasProviderProfile': false,
    },
  };

  test('login sends the device name and reads the session', () async {
    final repository = repositoryReplying(200, signedIn);

    final result = await repository.login(email: 'mariana@exemplo.com.br', password: 'vidro azul na janela');

    final session = (result as ApiSuccess<AuthSession>).data;
    expect(session.tokens.refreshToken, 'refresh-1');
    expect(session.me.initials, 'MS');
    final body = jsonDecode(requests.single.body) as Map<String, Object?>;
    expect(requests.single.url.path, '/api/v1/auth/login');
    expect(body['deviceName'], isNotEmpty);
  });

  test('verifyEmail sends the code together with the password', () async {
    final repository = repositoryReplying(200, signedIn);

    await repository.verifyEmail(email: 'mariana@exemplo.com.br', code: '482917', password: 'vidro azul na janela');

    final body = jsonDecode(requests.single.body) as Map<String, Object?>;
    expect(body['code'], '482917');
    expect(body['password'], 'vidro azul na janela');
  });

  test('register accepts the empty 202 response', () async {
    final repository = repositoryReplying(202, null);

    final result = await repository.register(
      displayName: 'Mariana Souza',
      email: 'mariana@exemplo.com.br',
      password: 'vidro azul na janela',
      acceptedTermsVersion: '2026-10-28',
    );

    expect(result, isA<ApiSuccess<void>>());
  });

  test('errors keep the problem code from the API', () async {
    final repository = repositoryReplying(401, {
      'type': 'https://vitrio.app/problems/auth.invalid_credentials',
      'status': 401,
      'detail': 'E-mail ou senha incorretos.',
    });

    final result = await repository.login(email: 'mariana@exemplo.com.br', password: 'errada');

    expect((result as ApiFailure<AuthSession>).problem.code, 'auth.invalid_credentials');
  });
}
