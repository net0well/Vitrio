import '../../../core/network/api_result.dart';
import '../domain/new_address.dart';
import '../domain/saved_address.dart';

abstract interface class AddressRepository {
  Future<ApiResult<List<SavedAddress>>> list();

  Future<ApiResult<SavedAddress>> add(NewAddress address);
}
