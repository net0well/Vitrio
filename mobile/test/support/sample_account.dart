import 'package:vitrio/core/session/auth_tokens.dart';
import 'package:vitrio/features/auth/domain/auth_session.dart';
import 'package:vitrio/features/auth/domain/me.dart';

const sampleMe = Me(
  id: '0199a0c4-3f2e-7c1a-9d7e-2a4b6c8d0e1f',
  displayName: 'Mariana Souza',
  initials: 'MS',
  email: 'mariana@exemplo.com.br',
  phoneNumber: null,
  roles: ['customer'],
);

const sampleSession = AuthSession(
  tokens: AuthTokens(accessToken: 'access-1', refreshToken: 'refresh-1'),
  me: sampleMe,
);
