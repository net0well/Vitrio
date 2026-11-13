import 'package:vitrio/core/network/api_result.dart';
import 'package:vitrio/features/account/data/address_repository.dart';
import 'package:vitrio/features/account/domain/new_address.dart';
import 'package:vitrio/features/account/domain/saved_address.dart';

final class FakeAddressRepository implements AddressRepository {
  final List<SavedAddress> addresses = [];
  final List<NewAddress> added = [];

  @override
  Future<ApiResult<List<SavedAddress>>> list() async => ApiSuccess(List.of(addresses));

  @override
  Future<ApiResult<SavedAddress>> add(NewAddress address) async {
    added.add(address);
    final saved = SavedAddress(
      id: 'address-${addresses.length + 1}',
      label: address.label,
      cep: address.cep,
      street: 'Rua Rui Barbosa',
      number: address.number,
      complement: address.complement,
      neighborhood: 'Centro',
      city: 'Jaboticabal',
      state: 'SP',
    );
    addresses.add(saved);
    return ApiSuccess(saved);
  }
}
