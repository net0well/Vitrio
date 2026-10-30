import 'package:shared_preferences/shared_preferences.dart';

import '../domain/app_mode.dart';

final class AppModeStore {
  const AppModeStore(this._preferences);

  static const _key = 'app_mode';

  final SharedPreferences _preferences;

  AppMode get mode => _preferences.getString(_key) == AppMode.provider.name ? AppMode.provider : AppMode.client;

  Future<void> save(AppMode mode) => _preferences.setString(_key, mode.name);
}
