import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/network/network_providers.dart';
import 'address_repository.dart';
import 'http_address_repository.dart';

part 'address_providers.g.dart';

@Riverpod(keepAlive: true)
AddressRepository addressRepository(Ref ref) => HttpAddressRepository(ref.watch(apiClientProvider));
