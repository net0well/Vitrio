import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/storage/preferences.dart';

part 'welcome_controller.g.dart';

@Riverpod(keepAlive: true)
class WelcomeController extends _$WelcomeController {
  static const _seenKey = 'welcome_seen';

  @override
  bool build() => ref.watch(preferencesProvider).getBool(_seenKey) ?? false;

  Future<void> complete() async {
    await ref.read(preferencesProvider).setBool(_seenKey, true);
    state = true;
  }
}
