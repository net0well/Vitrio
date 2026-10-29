import 'package:vitrio/core/network/api_result.dart';
import 'package:vitrio/core/network/problem_details.dart';
import 'package:vitrio/features/auth/data/account_repository.dart';
import 'package:vitrio/features/auth/domain/connected_session.dart';
import 'package:vitrio/features/auth/domain/me.dart';

final class FakeAccountRepository implements AccountRepository {
  ApiResult<Me> meResult = const ApiFailure(ProblemDetails(status: 401, code: 'unauthorized'));
  List<ConnectedSession> connected = const [];
  final List<String> revoked = [];

  @override
  Future<ApiResult<Me>> me() async => meResult;

  @override
  Future<ApiResult<Me>> updateMe({String? displayName, String? phoneNumber}) async => meResult;

  @override
  Future<ApiResult<List<ConnectedSession>>> sessions() async => ApiSuccess(connected);

  @override
  Future<ApiResult<void>> revokeSession(String id) async {
    revoked.add(id);
    connected = connected.where((session) => session.id != id).toList();
    return const ApiSuccess(null);
  }

  @override
  Future<ApiResult<void>> changePassword({required String currentPassword, required String newPassword}) async =>
      const ApiSuccess(null);

  @override
  Future<ApiResult<void>> deleteAccount() async => const ApiSuccess(null);
}
