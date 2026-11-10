import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../../../core/network/api_result.dart';
import '../../../core/presentation/no_retry.dart';
import '../data/provider_orders_providers.dart';
import '../domain/agenda_item.dart';

part 'provider_agenda_queries.g.dart';

@Riverpod(retry: noRetry)
Future<List<AgendaItem>> providerAgenda(Ref ref, DateTime from, DateTime to) async =>
    switch (await ref.read(providerOrdersRepositoryProvider).agenda(from: from, to: to)) {
      ApiSuccess(:final data) => data,
      ApiFailure(:final problem) => throw problem,
    };
