import 'package:vitrio/core/network/api_result.dart';
import 'package:vitrio/features/auth/data/auth_repository.dart';
import 'package:vitrio/features/auth/domain/auth_session.dart';
import 'package:vitrio/features/auth/domain/email_code_purpose.dart';

final class FakeAuthRepository implements AuthRepository {
  ApiResult<AuthSession>? loginResult;
  ApiResult<AuthSession>? verifyResult;
  ApiResult<void> resendResult = const ApiSuccess(null);
  final List<String> calls = [];
  String? lastVerifiedPassword;

  @override
  Future<ApiResult<void>> register({
    required String displayName,
    required String email,
    required String password,
    required String acceptedTermsVersion,
  }) async {
    calls.add('register:$email');
    return const ApiSuccess(null);
  }

  @override
  Future<ApiResult<AuthSession>> verifyEmail({required String email, required String code, required String password}) async {
    calls.add('verify:$email:$code');
    lastVerifiedPassword = password;
    return verifyResult!;
  }

  @override
  Future<ApiResult<void>> resendCode({required String email, required EmailCodePurpose purpose}) async {
    calls.add('resend:$email:${purpose.apiValue}');
    return resendResult;
  }

  @override
  Future<ApiResult<AuthSession>> login({required String email, required String password}) async {
    calls.add('login:$email');
    return loginResult!;
  }

  @override
  Future<ApiResult<void>> forgotPassword({required String email}) async => const ApiSuccess(null);

  @override
  Future<ApiResult<AuthSession>> resetPassword({required String email, required String code, required String newPassword}) async =>
      verifyResult!;

  @override
  Future<ApiResult<void>> logout({required bool allDevices}) async {
    calls.add('logout:$allDevices');
    return const ApiSuccess(null);
  }
}
