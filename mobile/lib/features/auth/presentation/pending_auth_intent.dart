import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../domain/auth_intent.dart';

part 'pending_auth_intent.g.dart';

@Riverpod(keepAlive: true)
class PendingAuthIntent extends _$PendingAuthIntent {
  @override
  AuthIntent? build() => null;

  void remember(AuthIntent intent) => state = intent;

  AuthIntent? take() {
    final intent = state;
    state = null;
    return intent;
  }
}
