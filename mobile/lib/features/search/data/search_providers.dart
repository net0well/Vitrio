import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/network/network_providers.dart';
import 'http_search_repository.dart';
import 'search_repository.dart';

part 'search_providers.g.dart';

@Riverpod(keepAlive: true)
SearchRepository searchRepository(Ref ref) => HttpSearchRepository(ref.watch(apiClientProvider));
