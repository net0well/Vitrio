import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/network/network_providers.dart';
import 'http_orders_repository.dart';
import 'orders_repository.dart';

part 'orders_providers.g.dart';

@Riverpod(keepAlive: true)
OrdersRepository ordersRepository(Ref ref) => HttpOrdersRepository(ref.watch(apiClientProvider));
