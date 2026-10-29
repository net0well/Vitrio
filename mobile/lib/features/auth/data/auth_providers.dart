import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/network/network_providers.dart';
import 'account_repository.dart';
import 'auth_repository.dart';
import 'http_account_repository.dart';
import 'http_auth_repository.dart';

part 'auth_providers.g.dart';

@Riverpod(keepAlive: true)
AuthRepository authRepository(Ref ref) => HttpAuthRepository(ref.watch(apiClientProvider));

@Riverpod(keepAlive: true)
AccountRepository accountRepository(Ref ref) => HttpAccountRepository(ref.watch(apiClientProvider));
