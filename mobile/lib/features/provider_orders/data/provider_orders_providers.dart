import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/network/network_providers.dart';
import 'http_provider_orders_repository.dart';
import 'provider_orders_repository.dart';

part 'provider_orders_providers.g.dart';

@Riverpod(keepAlive: true)
ProviderOrdersRepository providerOrdersRepository(Ref ref) => HttpProviderOrdersRepository(ref.watch(apiClientProvider));
