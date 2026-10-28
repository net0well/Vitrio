import 'dart:convert';

import 'package:flutter_test/flutter_test.dart';
import 'package:http/http.dart' as http;
import 'package:http/testing.dart';
import 'package:vitrio/core/network/api_client.dart';
import 'package:vitrio/core/network/api_result.dart';
import 'package:vitrio/core/network/problem_details.dart';
import 'package:vitrio/core/session/auth_tokens.dart';
import 'package:vitrio/core/session/token_store.dart';

void main() {
  final baseUrl = Uri.parse('https://api.test');

  late InMemoryTokenStore tokenStore;

  setUp(() => tokenStore = InMemoryTokenStore());

  ApiClient clientWith(MockClientHandler handler, {void Function()? onSessionExpired}) => ApiClient(
        httpClient: MockClient(handler),
        baseUrl: baseUrl,
        tokenStore: tokenStore,
        onSessionExpired: onSessionExpired,
      );

  group('successful responses', () {
    test('decodes the JSON body', () async {
      final client = clientWith((_) async => jsonResponse({'name': 'Carlos'}, 200));

      final result = await client.get('/api/v1/me', decode: (json) => (json! as Map<String, Object?>)['name']);

      expect(result, isA<ApiSuccess<Object?>>().having((s) => s.data, 'data', 'Carlos'));
    });

    test('passes null to the decoder when there is no content', () async {
      final client = clientWith((_) async => http.Response('', 204));

      final result = await client.delete('/api/v1/me/addresses/1', decode: (json) => json);

      expect(result, isA<ApiSuccess<Object?>>().having((s) => s.data, 'data', isNull));
    });

    test('sends the access token, JSON body and idempotency key', () async {
      tokenStore.tokens = const AuthTokens(accessToken: 'access-1', refreshToken: 'refresh-1');
      late http.Request sent;
      final client = clientWith((request) async {
        sent = request;
        return jsonResponse({'id': '1'}, 201);
      });

      await client.post('/api/v1/orders', body: {'message': 'Chuveiro'}, idempotencyKey: 'key-1', decode: (json) => json);

      expect(sent.headers['Authorization'], 'Bearer access-1');
      expect(sent.headers['Idempotency-Key'], 'key-1');
      expect(sent.headers['Content-Type'], startsWith('application/json'));
      expect(jsonDecode(sent.body), {'message': 'Chuveiro'});
    });
  });

  group('failures', () {
    test('converts problem details into a failure with the error code', () async {
      final client = clientWith(
        (_) async => jsonResponse(
          {
            'type': 'https://vitrio.app/problems/validation',
            'title': 'Bad Request',
            'status': 400,
            'detail': 'Um ou mais campos são inválidos.',
            'traceId': 'trace-1',
            'errors': {
              'displayName': ['Informe o nome.'],
            },
          },
          400,
          contentType: 'application/problem+json',
        ),
      );

      final result = await client.patch('/api/v1/me', body: {'displayName': ''}, decode: (json) => json);

      final problem = (result as ApiFailure<Object?>).problem;
      expect(problem.status, 400);
      expect(problem.code, 'validation');
      expect(problem.isValidation, isTrue);
      expect(problem.detail, 'Um ou mais campos são inválidos.');
      expect(problem.traceId, 'trace-1');
      expect(problem.errors, {
        'displayName': ['Informe o nome.'],
      });
    });

    test('uses the status code when the body is not a problem', () async {
      final client = clientWith((_) async => http.Response('<html>Bad Gateway</html>', 502));

      final result = await client.get('/api/v1/categories', decode: (json) => json);

      final problem = (result as ApiFailure<Object?>).problem;
      expect(problem.status, 502);
      expect(problem.code, 'http_502');
    });

    test('reports connectivity problems without throwing', () async {
      final client = clientWith((_) async => throw http.ClientException('offline'));

      final result = await client.get('/api/v1/categories', decode: (json) => json);

      final problem = (result as ApiFailure<Object?>).problem;
      expect(problem.code, ProblemDetails.networkErrorCode);
      expect(problem.isConnectivity, isTrue);
    });
  });

  group('session renewal', () {
    test('refreshes the tokens once and retries the request', () async {
      tokenStore.tokens = const AuthTokens(accessToken: 'expired', refreshToken: 'refresh-1');
      final calls = <String>[];
      final client = clientWith((request) async {
        calls.add('${request.method} ${request.url.path} ${request.headers['Authorization'] ?? ''}'.trim());
        if (request.url.path == ApiClient.refreshPath) {
          expect(jsonDecode(request.body), {'refreshToken': 'refresh-1'});
          return jsonResponse({'accessToken': 'fresh', 'expiresIn': 600, 'refreshToken': 'refresh-2'}, 200);
        }

        return request.headers['Authorization'] == 'Bearer fresh'
            ? jsonResponse({'ok': true}, 200)
            : http.Response('', 401);
      });

      final result = await client.get('/api/v1/me', decode: (json) => json);

      expect(result, isA<ApiSuccess<Object?>>());
      expect(calls, [
        'GET /api/v1/me Bearer expired',
        'POST /api/v1/auth/refresh',
        'GET /api/v1/me Bearer fresh',
      ]);
      expect(tokenStore.tokens?.refreshToken, 'refresh-2');
    });

    test('shares a single refresh between concurrent requests', () async {
      tokenStore.tokens = const AuthTokens(accessToken: 'expired', refreshToken: 'refresh-1');
      var refreshCalls = 0;
      final client = clientWith((request) async {
        if (request.url.path == ApiClient.refreshPath) {
          refreshCalls++;
          await Future<void>.delayed(const Duration(milliseconds: 10));
          return jsonResponse({'accessToken': 'fresh', 'expiresIn': 600, 'refreshToken': 'refresh-2'}, 200);
        }

        return request.headers['Authorization'] == 'Bearer fresh'
            ? jsonResponse({'ok': true}, 200)
            : http.Response('', 401);
      });

      final results = await Future.wait([
        client.get('/api/v1/me', decode: (json) => json),
        client.get('/api/v1/me/addresses', decode: (json) => json),
      ]);

      expect(refreshCalls, 1);
      expect(results, everyElement(isA<ApiSuccess<Object?>>()));
    });

    test('ends the session when the refresh token is rejected', () async {
      tokenStore.tokens = const AuthTokens(accessToken: 'expired', refreshToken: 'reused');
      var sessionExpired = false;
      final client = clientWith(
        (_) async => http.Response('', 401),
        onSessionExpired: () => sessionExpired = true,
      );

      final result = await client.get('/api/v1/me', decode: (json) => json);

      expect((result as ApiFailure<Object?>).problem.status, 401);
      expect(tokenStore.tokens, isNull);
      expect(sessionExpired, isTrue);
    });

    test('keeps the session when the refresh fails for a server error', () async {
      tokenStore.tokens = const AuthTokens(accessToken: 'expired', refreshToken: 'refresh-1');
      final client = clientWith(
        (request) async => request.url.path == ApiClient.refreshPath ? http.Response('', 503) : http.Response('', 401),
      );

      await client.get('/api/v1/me', decode: (json) => json);

      expect(tokenStore.tokens?.refreshToken, 'refresh-1');
    });

    test('does not refresh when there is no session', () async {
      final paths = <String>[];
      final client = clientWith((request) async {
        paths.add(request.url.path);
        return http.Response('', 401);
      });

      await client.get('/api/v1/me', decode: (json) => json);

      expect(paths, ['/api/v1/me']);
    });
  });
}

http.Response jsonResponse(Object body, int status, {String contentType = 'application/json; charset=utf-8'}) =>
    http.Response.bytes(utf8.encode(jsonEncode(body)), status, headers: {'content-type': contentType});

final class InMemoryTokenStore implements TokenStore {
  AuthTokens? tokens;

  @override
  Future<AuthTokens?> read() async => tokens;

  @override
  Future<void> write(AuthTokens tokens) async => this.tokens = tokens;

  @override
  Future<void> clear() async => tokens = null;
}
