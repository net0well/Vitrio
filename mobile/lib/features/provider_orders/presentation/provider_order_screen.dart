import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/routes.dart';
import '../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/design_system/widgets/vitrio_button.dart';
import '../../../core/design_system/widgets/vitrio_button_size.dart';
import '../../../core/design_system/widgets/vitrio_button_variant.dart';
import '../../../core/design_system/widgets/vitrio_empty_state.dart';
import '../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../core/design_system/widgets/vitrio_quote_card.dart';
import '../../../core/design_system/widgets/vitrio_quote_row.dart';
import '../../../core/design_system/widgets/vitrio_top_bar.dart';
import '../../../core/network/api_result.dart';
import '../../../core/presentation/clock.dart';
import '../../../core/presentation/problem_message.dart';
import '../../../l10n/app_localizations.dart';
import '../../orders/domain/cancellation_reason.dart';
import '../../orders/domain/city_time.dart';
import '../../orders/domain/order_action.dart';
import '../../orders/domain/order_party.dart';
import '../../orders/domain/order_status.dart';
import '../../orders/domain/quote_type.dart';
import '../../orders/presentation/cancel_order_sheet.dart';
import '../../orders/presentation/order_labels.dart';
import '../../orders/presentation/widgets/order_status_card.dart';
import '../domain/provider_order.dart';
import 'provider_order_controller.dart';
import 'widgets/order_contact_sections.dart';
import 'widgets/order_details_card.dart';
import 'widgets/order_request_card.dart';
import 'widgets/scheduled_service_card.dart';

class ProviderOrderScreen extends ConsumerWidget {
  const ProviderOrderScreen({super.key, required this.orderId});

  final String orderId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final order = ref.watch(providerOrderControllerProvider(orderId));
    return Scaffold(
      appBar: VitrioTopBar(title: l10n.providerOrderTitle),
      body: switch (order) {
        AsyncData(:final value) => RefreshIndicator(
          onRefresh: () => ref.refresh(providerOrderControllerProvider(orderId).future),
          child: _OrderBody(order: value),
        ),
        AsyncError() => Center(
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp20),
            child: VitrioEmptyState(
              icon: VitrioIconName.orders,
              message: l10n.orderLoadError,
              actionLabel: l10n.commonRetry,
              onAction: () => ref.invalidate(providerOrderControllerProvider(orderId)),
            ),
          ),
        ),
        _ => const Center(child: CircularProgressIndicator()),
      },
    );
  }
}

class _OrderBody extends ConsumerStatefulWidget {
  const _OrderBody({required this.order});

  final ProviderOrder order;

  @override
  ConsumerState<_OrderBody> createState() => _OrderBodyState();
}

class _OrderBodyState extends ConsumerState<_OrderBody> {
  bool _busy = false;

  ProviderOrderController get _controller => ref.read(providerOrderControllerProvider(widget.order.id).notifier);

  Future<void> _run(Future<ApiResult<ProviderOrder>> Function() action, String done) async {
    setState(() => _busy = true);
    final result = await action();
    if (!mounted) {
      return;
    }

    setState(() => _busy = false);
    final l10n = AppLocalizations.of(context);
    final message = switch (result) {
      ApiSuccess() => done,
      ApiFailure(:final problem) => problemMessage(l10n, problem),
    };
    ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(message)));
  }

  Future<void> _cancel() async {
    final l10n = AppLocalizations.of(context);
    final choice = await CancelOrderSheet.show(
      context,
      otherFirstName: widget.order.customer.firstName,
      reasons: CancellationReason.providerReasons,
    );
    if (choice != null && mounted) {
      await _run(() => _controller.cancel(choice.reason, choice.details), l10n.providerOrderCancelled);
    }
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final order = widget.order;
    final now = cityNow(ref.watch(clockProvider)(), order.cityOffset);
    final quote = order.quote;
    final schedule = order.schedule;
    final beforeAcceptance = order.status == OrderStatus.sent || order.status == OrderStatus.quoted;
    final closing = _closing(l10n, order, now);

    return ListView(
      padding: const EdgeInsets.fromLTRB(VitrioSpace.sp20, VitrioSpace.sp16, VitrioSpace.sp20, VitrioSpace.sp24),
      children: [
        Semantics(
          header: true,
          child: Text(order.title, style: VitrioText.title.copyWith(color: c.ink)),
        ),
        const SizedBox(height: VitrioSpace.sp4),
        Text(
          l10n.providerOrderMeta(order.serviceTitle, l10n.orderDayAndTime(shortDate(l10n, order.createdAt), formatTime(order.createdAt))),
          style: VitrioText.meta.copyWith(color: c.inkMuted),
        ),
        if (order.status == OrderStatus.scheduled && schedule != null) ...[
          const SizedBox(height: VitrioSpace.sp16),
          ScheduledServiceCard(
            order: order,
            now: now,
            busy: _busy,
            onReschedule: () => RescheduleOrderRoute(id: order.id).push<void>(context),
            onComplete: () => _run(_controller.complete, l10n.providerOrderCompleted),
          ),
        ],
        if (order.status == OrderStatus.quoted && quote != null) ...[
          const SizedBox(height: VitrioSpace.sp16),
          VitrioQuoteCard(
            title: l10n.providerQuoteSentTitle,
            price: formatQuoteAmount(quote.amount),
            validity: l10n.orderQuoteValidUntil(l10n.orderDayAndTime(shortDate(l10n, quote.validUntil), formatTime(quote.validUntil))),
            rows: [
              VitrioQuoteRow(icon: VitrioIconName.calendar, text: l10n.orderQuoteVisit(serviceWhen(l10n, quote.time, now))),
              if (quote.type == QuoteType.technicalVisit) VitrioQuoteRow(icon: VitrioIconName.info, text: l10n.orderQuoteTechnicalVisit),
              if (quote.materialIncluded)
                VitrioQuoteRow(
                  icon: VitrioIconName.check,
                  positive: true,
                  text: quote.materialDetails == null
                      ? l10n.orderQuoteMaterialIncluded
                      : l10n.orderQuoteMaterialIncludedWhat(quote.materialDetails!),
                )
              else
                VitrioQuoteRow(icon: VitrioIconName.info, text: l10n.respondMaterialCustomer(order.customer.firstName)),
            ],
            message: quote.message,
          ),
          const SizedBox(height: VitrioSpace.sp10),
          Text(
            l10n.providerQuoteWaiting(order.customer.firstName, relativeMoment(l10n, quote.validUntil, now)),
            style: VitrioText.weighted(VitrioText.caption, 600).copyWith(color: c.inkMuted),
          ),
        ],
        if (closing != null) ...[
          const SizedBox(height: VitrioSpace.sp16),
          OrderStatusCard(title: closing.title, body: closing.body, positive: closing.positive),
        ],
        if (beforeAcceptance) ...[
          const SizedBox(height: VitrioSpace.sp16),
          OrderRequestCard(order: order),
          if (order.allows(OrderAction.sendQuote) || order.allows(OrderAction.replaceQuote)) ...[
            const SizedBox(height: VitrioSpace.sp16),
            VitrioButton(
              label: order.allows(OrderAction.replaceQuote) ? l10n.providerReplaceQuote : l10n.providerSendQuote,
              variant: order.allows(OrderAction.replaceQuote) ? VitrioButtonVariant.secondary : VitrioButtonVariant.primary,
              size: VitrioButtonSize.decision,
              onPressed: _busy ? null : () => RespondOrderRoute(id: order.id).push<void>(context),
            ),
          ],
        ] else ...[
          OrderContactSections(order: order),
          const SizedBox(height: 22),
          OrderDetailsCard(order: order, now: now),
        ],
        if (order.allows(OrderAction.cancel)) ...[
          const SizedBox(height: 22),
          VitrioButton(
            label: l10n.cancelOrderConfirm,
            variant: VitrioButtonVariant.danger,
            size: VitrioButtonSize.decision,
            onPressed: _busy ? null : _cancel,
          ),
        ],
      ],
    );
  }

  ({String title, String? body, bool positive})? _closing(AppLocalizations l10n, ProviderOrder order, DateTime now) {
    final name = order.customer.firstName;
    final decline = order.decline;
    final cancellation = order.cancellation;
    return switch (order.status) {
      OrderStatus.declined => (
        title: l10n.providerDeclinedTitle,
        body: decline == null ? null : l10n.orderDeclinedReason(decline.details ?? declineReasonLabel(l10n, decline.reason)),
        positive: false,
      ),
      OrderStatus.expired => (title: l10n.providerExpiredTitle, body: null, positive: false),
      OrderStatus.quoteDeclined => (title: l10n.providerQuoteDeclinedTitle(name), body: null, positive: false),
      OrderStatus.quoteExpired => (title: l10n.providerQuoteExpiredTitle, body: null, positive: false),
      OrderStatus.cancelled when cancellation != null => (
        title: l10n.orderCancelledTitle,
        body: switch (cancellation.by) {
          OrderParty.provider => l10n.orderCancelledByYou(cancellationReasonLabel(l10n, cancellation.reason)),
          OrderParty.customer => l10n.providerCancelledByCustomer(name, cancellationReasonLabel(l10n, cancellation.reason)),
          OrderParty.system => l10n.orderCancelledBySystem(cancellationReasonLabel(l10n, cancellation.reason)),
        },
        positive: false,
      ),
      OrderStatus.completed || OrderStatus.paymentConfirmed || OrderStatus.paymentDisputed || OrderStatus.paymentVoided => (
        title: l10n.orderCompletedTitle,
        body: order.completedAt == null ? null : l10n.orderCompletedOn(relativeMoment(l10n, order.completedAt!, now)),
        positive: true,
      ),
      _ => null,
    };
  }
}
