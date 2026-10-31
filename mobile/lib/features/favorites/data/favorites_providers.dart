import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/network/network_providers.dart';
import 'favorites_repository.dart';
import 'http_favorites_repository.dart';

part 'favorites_providers.g.dart';

@Riverpod(keepAlive: true)
FavoritesRepository favoritesRepository(Ref ref) => HttpFavoritesRepository(ref.watch(apiClientProvider));
