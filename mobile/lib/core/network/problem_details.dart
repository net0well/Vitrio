import 'dart:convert';

final class ProblemDetails {
  const ProblemDetails({
    required this.status,
    required this.code,
    this.title,
    this.detail,
    this.traceId,
    this.errors = const {},
  });

  factory ProblemDetails.network() => const ProblemDetails(status: 0, code: networkErrorCode);

  factory ProblemDetails.timeout() => const ProblemDetails(status: 0, code: timeoutCode);

  factory ProblemDetails.invalidResponse(int status) => ProblemDetails(status: status, code: invalidResponseCode);

  factory ProblemDetails.fromResponse(int status, String? contentType, List<int> bodyBytes) {
    if (contentType != null && contentType.contains('json') && bodyBytes.isNotEmpty) {
      try {
        final json = jsonDecode(utf8.decode(bodyBytes));
        if (json is Map<String, Object?>) {
          return ProblemDetails.fromJson(json, status);
        }
      } on FormatException {
        return ProblemDetails.invalidResponse(status);
      }
    }

    return ProblemDetails(status: status, code: 'http_$status');
  }

  factory ProblemDetails.fromJson(Map<String, Object?> json, int fallbackStatus) {
    final status = json['status'];
    final type = json['type'];
    return ProblemDetails(
      status: status is int ? status : fallbackStatus,
      code: type is String && type.startsWith(typePrefix) ? type.substring(typePrefix.length) : 'http_$fallbackStatus',
      title: _string(json['title']),
      detail: _string(json['detail']),
      traceId: _string(json['traceId']),
      errors: _errors(json['errors']),
    );
  }

  static const typePrefix = 'https://vitrio.app/problems/';
  static const validationCode = 'validation';
  static const networkErrorCode = 'network_error';
  static const timeoutCode = 'timeout';
  static const invalidResponseCode = 'invalid_response';

  final int status;
  final String code;
  final String? title;
  final String? detail;
  final String? traceId;
  final Map<String, List<String>> errors;

  bool get isValidation => code == validationCode;

  bool get isConnectivity => code == networkErrorCode || code == timeoutCode;

  static String? _string(Object? value) => value is String ? value : null;

  static Map<String, List<String>> _errors(Object? value) {
    if (value is! Map<String, Object?>) {
      return const {};
    }

    return {
      for (final MapEntry(:key, :value) in value.entries)
        if (value is List<Object?>) key: value.whereType<String>().toList(growable: false),
    };
  }
}
