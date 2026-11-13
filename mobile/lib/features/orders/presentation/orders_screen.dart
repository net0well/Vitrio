import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/routes.dart';
import '../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/design_system/widgets/vitrio_empty_state.dart';
import '../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../core/design_system/widgets/vitrio_option.dart';
import '../../../core/design_system/widgets/vitrio_segmented.dart';
import '../../../l10n/app_localizations.dart';
import '../../auth/presentation/session_controller.dart';
import 'customer_orders_controller.dart';
import 'widgets/order_summary_card.dart';

class OrdersScreen extends ConsumerStatefulWidget {
  const OrdersScreen({super.key});

  @override
  ConsumerState<OrdersScreen> createState() => _OrdersScreenState();
}

class _OrdersScreenState extends ConsumerState<OrdersScreen> {
  bool _open = true;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final signedIn = ref.watch(sessionControllerProvider).value != null;
    return Scaffold(
      body: SafeArea(
        bottom: false,
        child: Padding(
          padding: const EdgeInsets.fromLTRB(VitrioSpace.sp20, VitrioSpace.sp20, VitrioSpace.sp20, 0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Semantics(header: true, child: Text(l10n.ordersTitle, style: VitrioText.title.copyWith(color: c.ink))),
              if (!signedIn)
                Expanded(
                  child: Center(
                    child: VitrioEmptyState(
                      icon: VitrioIconName.orders,
                      message: l10n.ordersSignedOut,
                      actionLabel: l10n.authSignIn,
                      onAction: () => const SignInRoute().push<void>(context),
                    ),
                  ),
                )
              else ...[
                const SizedBox(height: VitrioSpace.sp16),
                VitrioSegmented<bool>(
                  label: l10n.ordersTitle,
                  value: _open,
                  onChanged: (open) => setState(() => _open = open),
                  options: [
                    VitrioOption(value: true, label: l10n.ordersOpenTab),
                    VitrioOption(value: false, label: l10n.ordersClosedTab),
                  ],
                ),
                Expanded(child: _OrdersList(open: _open)),
              ],
            ],
          ),
        ),
      ),
    );
  }
}

class _OrdersList extends ConsumerWidget {
  const _OrdersList({required this.open});

  final bool open;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final provider = customerOrdersControllerProvider(open: open);
    return switch (ref.watch(provider)) {
      AsyncData(:final value) when value.items.isEmpty => Center(
          child: open
              ? VitrioEmptyState(
                  icon: VitrioIconName.orders,
                  message: l10n.ordersEmpty,
                  actionLabel: l10n.ordersEmptyAction,
                  onAction: () => const SearchRoute().go(context),
                )
              : VitrioEmptyState(icon: VitrioIconName.orders, message: l10n.ordersClosedEmpty),
        ),
      AsyncData(:final value) => RefreshIndicator(
          onRefresh: () => ref.refresh(provider.future),
          child: NotificationListener<ScrollEndNotification>(
            onNotification: (notification) {
              if (notification.metrics.extentAfter < 200) {
                ref.read(provider.notifier).loadMore();
              }
              return false;
            },
            child: ListView.separated(
              padding: const EdgeInsets.only(top: VitrioSpace.sp12, bottom: VitrioSpace.sp24),
              itemCount: value.items.length,
              separatorBuilder: (_, _) => const SizedBox(height: VitrioSpace.sp12),
              itemBuilder: (context, index) {
                final order = value.items[index];
                return OrderSummaryCard(order: order, onTap: () => OrderRoute(id: order.id).go(context));
              },
            ),
          ),
        ),
      AsyncError() => Center(
          child: VitrioEmptyState(
            icon: VitrioIconName.orders,
            message: l10n.ordersLoadError,
            actionLabel: l10n.commonRetry,
            onAction: () => ref.invalidate(provider),
          ),
        ),
      _ => const Center(child: CircularProgressIndicator()),
    };
  }
}
