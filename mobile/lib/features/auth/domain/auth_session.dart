import '../../../core/session/auth_tokens.dart';
import 'me.dart';

final class AuthSession {
  const AuthSession({required this.tokens, required this.me});

  factory AuthSession.fromJson(Map<String, Object?> json) {
    final user = json['user'];
    if (user is! Map<String, Object?>) {
      throw const FormatException('Sign-in response without user.');
    }

    return AuthSession(tokens: AuthTokens.fromJson(json), me: Me.fromJson(user));
  }

  final AuthTokens tokens;
  final Me me;
}
