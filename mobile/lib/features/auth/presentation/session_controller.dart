import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/network/api_result.dart';
import '../../../core/network/network_providers.dart';
import '../../../core/session/session_expiry.dart';
import '../data/auth_providers.dart';
import '../domain/auth_session.dart';
import '../domain/me.dart';

part 'session_controller.g.dart';

@Riverpod(keepAlive: true)
class SessionController extends _$SessionController {
  @override
  Future<Me?> build() async {
    ref.listen(sessionExpiryProvider, (_, _) => state = const AsyncData(null));

    final tokens = await ref.read(tokenStoreProvider).read();
    if (tokens == null) {
      return null;
    }

    return switch (await ref.read(accountRepositoryProvider).me()) {
      ApiSuccess(:final data) => data,
      ApiFailure(:final problem) when problem.isConnectivity => throw problem,
      ApiFailure() => await _forget(),
    };
  }

  Future<void> start(AuthSession session) async {
    await ref.read(tokenStoreProvider).write(session.tokens);
    state = AsyncData(session.me);
  }

  void replaceProfile(Me me) => state = AsyncData(me);

  Future<void> signOut({required bool allDevices}) async {
    await ref.read(authRepositoryProvider).logout(allDevices: allDevices);
    state = AsyncData(await _forget());
  }

  Future<ApiResult<void>> deleteAccount() async {
    final result = await ref.read(accountRepositoryProvider).deleteAccount();
    if (result is ApiSuccess<void>) {
      state = AsyncData(await _forget());
    }

    return result;
  }

  Future<Me?> _forget() async {
    await ref.read(tokenStoreProvider).clear();
    return null;
  }
}
