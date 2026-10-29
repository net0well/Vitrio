import '../../../core/network/api_result.dart';
import '../domain/connected_session.dart';
import '../domain/me.dart';

abstract interface class AccountRepository {
  Future<ApiResult<Me>> me();

  Future<ApiResult<Me>> updateMe({String? displayName, String? phoneNumber});

  Future<ApiResult<List<ConnectedSession>>> sessions();

  Future<ApiResult<void>> revokeSession(String id);

  Future<ApiResult<void>> changePassword({required String currentPassword, required String newPassword});

  Future<ApiResult<void>> deleteAccount();
}
