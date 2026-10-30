import '../../../core/network/api_client.dart';
import '../../../core/network/api_result.dart';
import '../../../core/network/json_body.dart';
import '../domain/connected_session.dart';
import '../domain/me.dart';
import 'account_repository.dart';

final class HttpAccountRepository implements AccountRepository {
  const HttpAccountRepository(this._api);

  final ApiClient _api;

  @override
  Future<ApiResult<Me>> me() => _api.get('/api/v1/me', decode: _me);

  @override
  Future<ApiResult<Me>> updateMe({String? displayName, String? phoneNumber}) => _api.patch(
        '/api/v1/me',
        body: {'displayName': ?displayName, 'phoneNumber': ?phoneNumber},
        decode: _me,
      );

  @override
  Future<ApiResult<List<ConnectedSession>>> sessions() => _api.get(
        '/api/v1/me/sessions',
        decode: (json) => jsonObjects(json).map(ConnectedSession.fromJson).toList(growable: false),
      );

  @override
  Future<ApiResult<void>> revokeSession(String id) => _api.delete('/api/v1/me/sessions/$id', decode: _ignore);

  @override
  Future<ApiResult<void>> changePassword({required String currentPassword, required String newPassword}) => _api.put(
        '/api/v1/me/password',
        body: {'currentPassword': currentPassword, 'newPassword': newPassword},
        decode: _ignore,
      );

  @override
  Future<ApiResult<void>> deleteAccount() => _api.delete('/api/v1/me', decode: _ignore);

  static Me _me(Object? json) => Me.fromJson(jsonObject(json));

  static void _ignore(Object? json) {}
}
