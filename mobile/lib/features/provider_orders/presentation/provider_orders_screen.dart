import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
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
import '../../provider/presentation/provider_queries.dart';
import '../domain/provider_order_tab.dart';
import 'decline_order_flow.dart';
import 'provider_orders_controller.dart';
import 'widgets/new_order_card.dart';
import 'widgets/provider_order_card.dart';

class ProviderOrdersScreen extends ConsumerStatefulWidget {
  const ProviderOrdersScreen({super.key});

  @override
  ConsumerState<ProviderOrdersScreen> createState() => _ProviderOrdersScreenState();
}

class _ProviderOrdersScreenState extends ConsumerState<ProviderOrdersScreen> {
  ProviderOrderTab _tab = ProviderOrderTab.fresh;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final fresh = ref.watch(providerDashboardProvider).value?.newOrders ?? 0;
    return AnnotatedRegion<SystemUiOverlayStyle>(
      value: Theme.of(context).brightness == Brightness.dark ? SystemUiOverlayStyle.light : SystemUiOverlayStyle.dark,
      child: Scaffold(
        body: SafeArea(
          bottom: false,
          child: Padding(
            padding: const EdgeInsets.fromLTRB(VitrioSpace.sp20, VitrioSpace.sp20, VitrioSpace.sp20, 0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Semantics(
                  header: true,
                  child: Text(l10n.tabOrders, style: VitrioText.title.copyWith(color: c.ink)),
                ),
                const SizedBox(height: VitrioSpace.sp16),
                VitrioSegmented<ProviderOrderTab>(
                  label: l10n.tabOrders,
                  value: _tab,
                  onChanged: (tab) => setState(() => _tab = tab),
                  options: [
                    VitrioOption(
                      value: ProviderOrderTab.fresh,
                      label: fresh > 0 ? l10n.providerOrdersNewTabCount(fresh) : l10n.providerOrdersNewTab,
                    ),
                    VitrioOption(value: ProviderOrderTab.ongoing, label: l10n.ordersOpenTab),
                    VitrioOption(value: ProviderOrderTab.closed, label: l10n.ordersClosedTab),
                  ],
                ),
                Expanded(child: _OrdersList(tab: _tab)),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class _OrdersList extends ConsumerWidget {
  const _OrdersList({required this.tab});

  final ProviderOrderTab tab;

  String _empty(AppLocalizations l10n) => switch (tab) {
    ProviderOrderTab.fresh => l10n.providerNoOrders,
    ProviderOrderTab.ongoing => l10n.providerOrdersOngoingEmpty,
    ProviderOrderTab.closed => l10n.providerOrdersClosedEmpty,
  };

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final provider = providerOrdersControllerProvider(tab);
    return switch (ref.watch(provider)) {
      AsyncData(:final value) when value.items.isEmpty => Center(
        child: VitrioEmptyState(icon: VitrioIconName.orders, message: _empty(l10n)),
      ),
      AsyncData(:final value) => RefreshIndicator(
        onRefresh: () {
          ref.invalidate(providerDashboardProvider);
          return ref.refresh(provider.future);
        },
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
              return tab == ProviderOrderTab.fresh
                  ? NewOrderCard(
                      order: order,
                      onDecline: () => declineFromList(context, ref, order),
                      onRespond: () => RespondOrderRoute(id: order.id).push<void>(context),
                    )
                  : ProviderOrderCard(
                      order: order,
                      onTap: () => ProviderOrderRoute(id: order.id).go(context),
                    );
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
