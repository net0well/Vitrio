import '../../../core/network/api_client.dart';
import '../../../core/network/api_result.dart';
import '../../../core/network/json_body.dart';
import '../domain/auth_session.dart';
import '../domain/email_code_purpose.dart';
import 'auth_repository.dart';
import 'device_name.dart';

final class HttpAuthRepository implements AuthRepository {
  const HttpAuthRepository(this._api);

  final ApiClient _api;

  @override
  Future<ApiResult<void>> register({
    required String displayName,
    required String email,
    required String password,
    required String acceptedTermsVersion,
  }) =>
      _api.post(
        '/api/v1/auth/register',
        body: {'displayName': displayName, 'email': email, 'password': password, 'acceptedTermsVersion': acceptedTermsVersion},
        decode: _ignore,
      );

  @override
  Future<ApiResult<AuthSession>> verifyEmail({required String email, required String code, required String password}) =>
      _api.post(
        '/api/v1/auth/verify-email',
        body: {'email': email, 'code': code, 'password': password, 'deviceName': currentDeviceName()},
        decode: _session,
      );

  @override
  Future<ApiResult<void>> resendCode({required String email, required EmailCodePurpose purpose}) =>
      _api.post('/api/v1/auth/resend-code', body: {'email': email, 'purpose': purpose.apiValue}, decode: _ignore);

  @override
  Future<ApiResult<AuthSession>> login({required String email, required String password}) => _api.post(
        '/api/v1/auth/login',
        body: {'email': email, 'password': password, 'deviceName': currentDeviceName()},
        decode: _session,
      );

  @override
  Future<ApiResult<void>> forgotPassword({required String email}) =>
      _api.post('/api/v1/auth/password/forgot', body: {'email': email}, decode: _ignore);

  @override
  Future<ApiResult<AuthSession>> resetPassword({required String email, required String code, required String newPassword}) =>
      _api.post(
        '/api/v1/auth/password/reset',
        body: {'email': email, 'code': code, 'newPassword': newPassword, 'deviceName': currentDeviceName()},
        decode: _session,
      );

  @override
  Future<ApiResult<void>> logout({required bool allDevices}) =>
      _api.post('/api/v1/auth/logout', body: {'allDevices': allDevices}, decode: _ignore);

  static AuthSession _session(Object? json) => AuthSession.fromJson(jsonObject(json));

  static void _ignore(Object? json) {}
}
