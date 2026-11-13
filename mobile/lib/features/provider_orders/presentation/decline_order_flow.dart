import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/network/api_result.dart';
import '../../../core/presentation/problem_message.dart';
import '../../../l10n/app_localizations.dart';
import '../../provider/presentation/provider_controller.dart';
import '../../provider/presentation/provider_queries.dart';
import '../data/provider_orders_providers.dart';
import '../domain/provider_order_summary.dart';
import 'decline_order_sheet.dart';
import 'provider_orders_controller.dart';

Future<void> declineFromList(BuildContext context, WidgetRef ref, ProviderOrderSummary order) async {
  final l10n = AppLocalizations.of(context);
  final messenger = ScaffoldMessenger.of(context);
  final provider = ref.read(providerControllerProvider).value;
  final choice = await DeclineOrderSheet.show(
    context,
    customerFirstName: order.customerName.split(' ').first,
    providerFirstName: provider?.displayName.split(' ').first ?? '',
  );
  if (choice == null) {
    return;
  }

  final result = await ref.read(providerOrdersRepositoryProvider).decline(order.id, reason: choice.reason, details: choice.details);
  ref.invalidate(providerOrdersControllerProvider);
  ref.invalidate(providerDashboardProvider);
  messenger.showSnackBar(
    SnackBar(
      content: Text(switch (result) {
        ApiSuccess() => l10n.providerDeclinedTitle,
        ApiFailure(:final problem) => problemMessage(l10n, problem),
      }),
    ),
  );
}
