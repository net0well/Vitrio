import '../../../core/network/api_result.dart';
import '../domain/auth_session.dart';
import '../domain/email_code_purpose.dart';

abstract interface class AuthRepository {
  Future<ApiResult<void>> register({
    required String displayName,
    required String email,
    required String password,
    required String acceptedTermsVersion,
  });

  Future<ApiResult<AuthSession>> verifyEmail({required String email, required String code, required String password});

  Future<ApiResult<void>> resendCode({required String email, required EmailCodePurpose purpose});

  Future<ApiResult<AuthSession>> login({required String email, required String password});

  Future<ApiResult<void>> forgotPassword({required String email});

  Future<ApiResult<AuthSession>> resetPassword({required String email, required String code, required String newPassword});

  Future<ApiResult<void>> logout({required bool allDevices});
}
