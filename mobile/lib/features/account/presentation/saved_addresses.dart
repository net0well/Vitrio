import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/network/api_result.dart';
import '../../../core/presentation/no_retry.dart';
import '../data/address_providers.dart';
import '../domain/saved_address.dart';

part 'saved_addresses.g.dart';

@Riverpod(retry: noRetry)
Future<List<SavedAddress>> savedAddresses(Ref ref) async => switch (await ref.read(addressRepositoryProvider).list()) {
      ApiSuccess(:final data) => data,
      ApiFailure(:final problem) => throw problem,
    };
