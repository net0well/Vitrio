import 'package:riverpod_annotation/riverpod_annotation.dart';

part 'pending_favorite.g.dart';

@Riverpod(keepAlive: true)
class PendingFavorite extends _$PendingFavorite {
  @override
  String? build() => null;

  void remember(String providerId) => state = providerId;

  String? take() {
    final providerId = state;
    state = null;
    return providerId;
  }
}
