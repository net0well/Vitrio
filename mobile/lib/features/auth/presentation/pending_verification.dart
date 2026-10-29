import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../domain/pending_credentials.dart';

part 'pending_verification.g.dart';

@Riverpod(keepAlive: true)
class PendingVerification extends _$PendingVerification {
  @override
  PendingCredentials? build() => null;

  void remember(PendingCredentials credentials) => state = credentials;

  void clear() => state = null;
}
