import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/network/api_result.dart';
import '../../auth/data/auth_providers.dart';
import '../../auth/domain/connected_session.dart';

part 'connected_devices_controller.g.dart';

@riverpod
class ConnectedDevicesController extends _$ConnectedDevicesController {
  @override
  Future<List<ConnectedSession>> build() async => switch (await ref.read(accountRepositoryProvider).sessions()) {
        ApiSuccess(:final data) => data,
        ApiFailure(:final problem) => throw problem,
      };

  Future<void> disconnect(String sessionId) async {
    await ref.read(accountRepositoryProvider).revokeSession(sessionId);
    ref.invalidateSelf();
  }
}
