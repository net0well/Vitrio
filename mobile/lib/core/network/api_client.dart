import 'dart:async';
import 'dart:convert';

import 'package:http/http.dart' as http;
import 'package:uuid/uuid.dart';

import '../session/auth_tokens.dart';
import '../session/token_store.dart';
import 'api_result.dart';
import 'problem_details.dart';

typedef JsonDecoder<T> = T Function(Object? json);

class ApiClient {
  ApiClient({
    required http.Client httpClient,
    required this._baseUrl,
    required this._tokenStore,
    this._onSessionExpired,
    this._timeout = const Duration(seconds: 15),
  }) : _http = httpClient;

  static const refreshPath = '/api/v1/auth/refresh';

  final http.Client _http;
  final Uri _baseUrl;
  final TokenStore _tokenStore;
  final void Function()? _onSessionExpired;
  final Duration _timeout;

  Future<AuthTokens?>? _refreshing;

  static String newIdempotencyKey() => const Uuid().v4();

  Future<ApiResult<T>> get<T>(String path, {Map<String, String>? query, required JsonDecoder<T> decode}) =>
      send('GET', path, query: query, decode: decode);

  Future<ApiResult<T>> post<T>(String path, {Object? body, String? idempotencyKey, required JsonDecoder<T> decode}) =>
      send('POST', path, body: body, idempotencyKey: idempotencyKey, decode: decode);

  Future<ApiResult<T>> put<T>(String path, {Object? body, required JsonDecoder<T> decode}) =>
      send('PUT', path, body: body, decode: decode);

  Future<ApiResult<T>> patch<T>(String path, {Object? body, required JsonDecoder<T> decode}) =>
      send('PATCH', path, body: body, decode: decode);

  Future<ApiResult<T>> delete<T>(String path, {required JsonDecoder<T> decode}) =>
      send('DELETE', path, decode: decode);

  Future<ApiResult<T>> send<T>(
    String method,
    String path, {
    Map<String, String>? query,
    Object? body,
    String? idempotencyKey,
    required JsonDecoder<T> decode,
  }) async {
    final uri = _resolve(path, query);
    try {
      final tokens = await _tokenStore.read();
      var response = await _send(
        method,
        uri,
        body: body,
        idempotencyKey: idempotencyKey,
        accessToken: tokens?.accessToken,
      );

      if (response.statusCode == 401 && tokens != null) {
        final accessToken = await _renewAccessToken(tokens.accessToken);
        if (accessToken != null) {
          response = await _send(method, uri, body: body, idempotencyKey: idempotencyKey, accessToken: accessToken);
        }
      }

      return _toResult(response, decode);
    } on TimeoutException {
      return ApiFailure(ProblemDetails.timeout());
    } on http.ClientException {
      return ApiFailure(ProblemDetails.network());
    }
  }

  Future<String?> _renewAccessToken(String rejectedAccessToken) async {
    final current = await _tokenStore.read();
    if (current == null) {
      return null;
    }

    if (current.accessToken != rejectedAccessToken) {
      return current.accessToken;
    }

    final refreshed = await (_refreshing ??= _refresh(current.refreshToken).whenComplete(() => _refreshing = null));
    return refreshed?.accessToken;
  }

  Future<AuthTokens?> _refresh(String refreshToken) async {
    final http.Response response;
    try {
      response = await _send('POST', _resolve(refreshPath, null), body: {'refreshToken': refreshToken});
    } on TimeoutException {
      return null;
    } on http.ClientException {
      return null;
    }

    if (response.statusCode == 200) {
      try {
        final json = jsonDecode(utf8.decode(response.bodyBytes));
        if (json is! Map<String, Object?>) {
          return null;
        }

        final tokens = AuthTokens.fromJson(json);
        await _tokenStore.write(tokens);
        return tokens;
      } on FormatException {
        return null;
      }
    }

    if (response.statusCode >= 400 && response.statusCode < 500) {
      await _tokenStore.clear();
      _onSessionExpired?.call();
    }

    return null;
  }

  Future<http.Response> _send(
    String method,
    Uri uri, {
    Object? body,
    String? idempotencyKey,
    String? accessToken,
  }) async {
    final request = http.Request(method, uri)..headers['Accept'] = 'application/json';
    if (accessToken != null) {
      request.headers['Authorization'] = 'Bearer $accessToken';
    }

    if (idempotencyKey != null) {
      request.headers['Idempotency-Key'] = idempotencyKey;
    }

    if (body != null) {
      request.headers['Content-Type'] = 'application/json';
      request.body = jsonEncode(body);
    }

    final streamed = await _http.send(request).timeout(_timeout);
    return http.Response.fromStream(streamed).timeout(_timeout);
  }

  ApiResult<T> _toResult<T>(http.Response response, JsonDecoder<T> decode) {
    final status = response.statusCode;
    if (status < 200 || status >= 300) {
      return ApiFailure(ProblemDetails.fromResponse(status, response.headers['content-type'], response.bodyBytes));
    }

    try {
      final json = response.bodyBytes.isEmpty ? null : jsonDecode(utf8.decode(response.bodyBytes));
      return ApiSuccess(decode(json));
    } on FormatException {
      return ApiFailure(ProblemDetails.invalidResponse(status));
    }
  }

  Uri _resolve(String path, Map<String, String>? query) {
    final resolved = _baseUrl.resolve(path);
    return query == null || query.isEmpty ? resolved : resolved.replace(queryParameters: query);
  }
}
