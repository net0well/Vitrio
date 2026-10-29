import 'package:riverpod_annotation/riverpod_annotation.dart';

part 'session_expiry.g.dart';

@Riverpod(keepAlive: true)
class SessionExpiry extends _$SessionExpiry {
  @override
  int build() => 0;

  void notify() => state++;
}
