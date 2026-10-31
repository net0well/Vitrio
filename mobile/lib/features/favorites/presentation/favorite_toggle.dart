import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/network/api_result.dart';
import '../../provider/presentation/provider_queries.dart';
import '../data/favorites_providers.dart';
import 'favorites_controller.dart';

part 'favorite_toggle.g.dart';

@riverpod
class FavoriteToggle extends _$FavoriteToggle {
  @override
  bool build(String providerId) => false;

  Future<bool> toggle({required bool saved}) async {
    if (state) {
      return true;
    }

    state = true;
    final repository = ref.read(favoritesRepositoryProvider);
    final result = saved ? await repository.remove(providerId) : await repository.save(providerId);
    if (!ref.mounted) {
      return result is ApiSuccess;
    }

    state = false;
    if (result is! ApiSuccess) {
      return false;
    }

    ref
      ..invalidate(publicProviderProvider(providerId))
      ..invalidate(favoritesControllerProvider);
    return true;
  }
}
