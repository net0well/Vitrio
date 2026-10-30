import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../data/provider_providers.dart';
import '../domain/app_mode.dart';

part 'app_mode_controller.g.dart';

@Riverpod(keepAlive: true)
class AppModeController extends _$AppModeController {
  @override
  AppMode build() => ref.read(appModeStoreProvider).mode;

  Future<void> change(AppMode mode) async {
    await ref.read(appModeStoreProvider).save(mode);
    state = mode;
  }
}
