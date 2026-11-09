import '../../../core/network/api_client.dart';
import '../../../core/network/api_result.dart';
import '../../../core/network/json_body.dart';
import '../domain/new_address.dart';
import '../domain/saved_address.dart';
import 'address_repository.dart';

final class HttpAddressRepository implements AddressRepository {
  const HttpAddressRepository(this._api);

  static const _addresses = '/api/v1/me/addresses';

  final ApiClient _api;

  @override
  Future<ApiResult<List<SavedAddress>>> list() =>
      _api.get(_addresses, decode: (json) => jsonObjects(json).map(SavedAddress.fromJson).toList(growable: false));

  @override
  Future<ApiResult<SavedAddress>> add(NewAddress address) =>
      _api.post(_addresses, body: address.toJson(), decode: (json) => SavedAddress.fromJson(jsonObject(json)));
}
