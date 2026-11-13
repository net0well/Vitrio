import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:url_launcher/url_launcher.dart';

import '../../../app/routes.dart';
import '../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../core/design_system/tokens/vitrio_radius.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/design_system/widgets/vitrio_avatar.dart';
import '../../../core/design_system/widgets/vitrio_avatar_kind.dart';
import '../../../core/design_system/widgets/vitrio_badge.dart';
import '../../../core/design_system/widgets/vitrio_badge_tone.dart';
import '../../../core/design_system/widgets/vitrio_button.dart';
import '../../../core/design_system/widgets/vitrio_button_size.dart';
import '../../../core/design_system/widgets/vitrio_button_variant.dart';
import '../../../core/design_system/widgets/vitrio_empty_state.dart';
import '../../../core/design_system/widgets/vitrio_icon.dart';
import '../../../core/design_system/widgets/vitrio_icon_button.dart';
import '../../../core/design_system/widgets/vitrio_icon_button_variant.dart';
import '../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../core/design_system/widgets/vitrio_notice.dart';
import '../../../core/design_system/widgets/vitrio_order_timeline.dart';
import '../../../core/design_system/widgets/vitrio_quote_card.dart';
import '../../../core/design_system/widgets/vitrio_quote_row.dart';
import '../../../core/design_system/widgets/vitrio_star.dart';
import '../../../core/design_system/widgets/vitrio_timeline_step.dart';
import '../../../core/design_system/widgets/vitrio_timeline_step_state.dart';
import '../../../core/design_system/widgets/vitrio_top_bar.dart';
import '../../../core/network/api_result.dart';
import '../../../core/presentation/brazilian_format.dart';
import '../../../core/presentation/clock.dart';
import '../../../core/presentation/problem_message.dart';
import '../../../l10n/app_localizations.dart';
import '../../provider/presentation/provider_queries.dart';
import '../../search/domain/provider_listing.dart';
import '../../search/presentation/search_filters_controller.dart';
import '../domain/city_time.dart';
import '../domain/customer_order.dart';
import '../domain/order_action.dart';
import '../domain/order_party.dart';
import '../domain/order_status.dart';
import '../domain/quote_type.dart';
import 'cancel_order_sheet.dart';
import 'customer_order_controller.dart';
import 'order_labels.dart';
import 'widgets/order_status_card.dart';

class OrderScreen extends ConsumerWidget {
  const OrderScreen({super.key, required this.orderId});

  final String orderId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final order = ref.watch(customerOrderControllerProvider(orderId));
    return Scaffold(
      appBar: VitrioTopBar(title: l10n.orderScreenTitle),
      body: switch (order) {
        AsyncData(:final value) => RefreshIndicator(
            onRefresh: () => ref.refresh(customerOrderControllerProvider(orderId).future),
            child: _OrderBody(order: value),
          ),
        AsyncError() => Center(
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp20),
              child: VitrioEmptyState(
                icon: VitrioIconName.orders,
                message: l10n.orderLoadError,
                actionLabel: l10n.commonRetry,
                onAction: () => ref.invalidate(customerOrderControllerProvider(orderId)),
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

  final CustomerOrder order;

  @override
  ConsumerState<_OrderBody> createState() => _OrderBodyState();
}

class _OrderBodyState extends ConsumerState<_OrderBody> {
  bool _busy = false;

  CustomerOrderController get _controller => ref.read(customerOrderControllerProvider(widget.order.id).notifier);

  Future<void> _run(Future<ApiResult<CustomerOrder>> Function() action) async {
    setState(() => _busy = true);
    final result = await action();
    if (!mounted) {
      return;
    }

    setState(() => _busy = false);
    if (result case ApiFailure(:final problem)) {
      final l10n = AppLocalizations.of(context);
      final message = problem.code == 'quote.version_mismatch'
          ? l10n.orderQuoteChanged(widget.order.provider.firstName)
          : problemMessage(l10n, problem);
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(message)));
    }
  }

  Future<void> _cancel() async {
    final choice = await CancelOrderSheet.show(context, otherFirstName: widget.order.provider.firstName);
    if (choice != null && mounted) {
      await _run(() => _controller.cancel(choice.reason, choice.details));
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
    final declined = order.status.offersSuggestions;

    return ListView(
      padding: const EdgeInsets.fromLTRB(VitrioSpace.sp20, VitrioSpace.sp16, VitrioSpace.sp20, VitrioSpace.sp24),
      children: [
        Semantics(header: true, child: Text(order.title, style: VitrioText.title.copyWith(color: c.ink))),
        const SizedBox(height: VitrioSpace.sp4),
        Text(
          l10n.orderSentLine(
            relativeMoment(l10n, order.createdAt, now),
            order.neighborhood,
            preferenceLabel(l10n, order.preferredDay, order.preferredPeriod, now),
          ),
          style: VitrioText.meta.copyWith(color: c.inkMuted),
        ),
        if (!declined) ...[
          const SizedBox(height: VitrioSpace.sp14),
          _ProviderRow(order: order),
        ],
        if (order.status.isOpen || order.status == OrderStatus.completed) ...[
          const SizedBox(height: VitrioSpace.sp16),
          _ProgressCard(order: order, now: now),
        ],
        if (order.status == OrderStatus.quoted && quote != null) ...[
          const SizedBox(height: VitrioSpace.sp16),
          VitrioQuoteCard(
            title: l10n.orderQuoteFrom(order.provider.firstName),
            price: formatQuoteAmount(quote.amount),
            validity: l10n.orderQuoteValidUntil(_moment(l10n, quote.validUntil)),
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
                VitrioQuoteRow(icon: VitrioIconName.info, text: l10n.orderQuoteMaterialNotIncluded),
            ],
            message: quote.message,
            acceptLabel: order.allows(OrderAction.acceptQuote) ? l10n.orderQuoteAccept : null,
            declineLabel: order.allows(OrderAction.declineQuote) ? l10n.orderQuoteDecline : null,
            onAccept: () => _run(() => _controller.acceptQuote(quote.version)),
            onDecline: () => _run(_controller.declineQuote),
            busy: _busy,
          ),
          const SizedBox(height: VitrioSpace.sp10),
          Text(
            l10n.orderQuoteAddressNote(order.provider.firstName),
            style: VitrioText.weighted(VitrioText.caption, 600).copyWith(color: c.inkMuted),
          ),
        ],
        if (order.status == OrderStatus.scheduled && schedule != null) ...[
          const SizedBox(height: VitrioSpace.sp16),
          _ScheduledCard(order: order, now: now, busy: _busy, onCancel: _cancel, onComplete: () => _run(_controller.complete)),
        ],
        if (order.rescheduleCount > 0 && order.status == OrderStatus.scheduled && schedule != null) ...[
          const SizedBox(height: VitrioSpace.sp14),
          VitrioNotice(
            message: l10n.orderRescheduled(order.provider.firstName, serviceWhen(l10n, schedule, now)),
            tone: VitrioBadgeTone.info,
          ),
        ],
        if (declined) ...[
          const SizedBox(height: VitrioSpace.sp16),
          _DeclinedCard(order: order),
          _Suggestions(order: order),
        ],
        if (_closingTitle(l10n, order, now) case (final String title, final String? body, final bool positive)) ...[
          const SizedBox(height: VitrioSpace.sp16),
          OrderStatusCard(title: title, body: body, positive: positive),
        ],
        if (!declined) ...[
          const SizedBox(height: VitrioSpace.sp14),
          VitrioNotice(message: l10n.orderPaymentNote, tone: VitrioBadgeTone.brand, icon: VitrioIconName.info),
        ],
        if (order.allows(OrderAction.cancel) && order.status == OrderStatus.sent) ...[
          const SizedBox(height: VitrioSpace.sp12),
          VitrioButton(label: l10n.cancelOrderConfirm, variant: VitrioButtonVariant.ghost, onPressed: _busy ? null : _cancel),
        ],
      ],
    );
  }

  String _moment(AppLocalizations l10n, DateTime cityTime) => l10n.orderDayAndTime(shortDate(l10n, cityTime), formatTime(cityTime));

  (String, String?, bool)? _closingTitle(AppLocalizations l10n, CustomerOrder order, DateTime now) {
    final cancellation = order.cancellation;
    return switch (order.status) {
      OrderStatus.cancelled when cancellation != null => (
          l10n.orderCancelledTitle,
          switch (cancellation.by) {
            OrderParty.customer => l10n.orderCancelledByYou(cancellationReasonLabel(l10n, cancellation.reason)),
            OrderParty.provider => l10n.orderCancelledByProvider(order.provider.firstName, cancellationReasonLabel(l10n, cancellation.reason)),
            OrderParty.system => l10n.orderCancelledBySystem(cancellationReasonLabel(l10n, cancellation.reason)),
          },
          false,
        ),
      OrderStatus.quoteDeclined => (l10n.orderQuoteDeclinedTitle, null, false),
      OrderStatus.quoteExpired => (l10n.orderQuoteExpiredTitle, null, false),
      OrderStatus.completed || OrderStatus.paymentConfirmed || OrderStatus.paymentDisputed || OrderStatus.paymentVoided => (
          l10n.orderCompletedTitle,
          order.completedAt == null ? null : l10n.orderCompletedOn(relativeMoment(l10n, order.completedAt!, now)),
          true,
        ),
      _ => null,
    };
  }
}

class _ProviderRow extends StatelessWidget {
  const _ProviderRow({required this.order});

  final CustomerOrder order;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final provider = order.provider;
    final rating = provider.ratingScore;
    final whatsApp = provider.whatsAppPhone;
    return Container(
      padding: const EdgeInsets.fromLTRB(VitrioSpace.sp14, VitrioSpace.sp12, VitrioSpace.sp12, VitrioSpace.sp12),
      decoration: BoxDecoration(
        color: c.surfaceRaised,
        borderRadius: const BorderRadius.all(VitrioRadius.xxl),
        border: Border.all(color: c.lineSoft),
      ),
      child: Row(
        children: [
          VitrioAvatar(
            name: provider.displayName,
            initials: provider.initials,
            imageUrl: provider.avatarUrl,
            size: VitrioSpace.tap,
            kind: VitrioAvatarKind.provider,
            seed: provider.id,
          ),
          const SizedBox(width: VitrioSpace.sp12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(provider.displayName, style: VitrioText.bodyStrong.copyWith(color: c.ink)),
                const SizedBox(height: 2),
                Row(
                  children: [
                    Flexible(
                      child: Text(
                        rating == null ? provider.headline : '${provider.headline} · ',
                        overflow: TextOverflow.ellipsis,
                        style: VitrioText.weighted(VitrioText.meta, 400).copyWith(color: c.inkMuted),
                      ),
                    ),
                    if (rating != null) ...[
                      const VitrioStar(size: 13),
                      const SizedBox(width: VitrioSpace.sp4),
                      Text(formatRating(rating), style: VitrioText.weighted(VitrioText.meta, 800).copyWith(color: c.ink)),
                    ],
                  ],
                ),
              ],
            ),
          ),
          if (whatsApp != null)
            VitrioIconButton(
              icon: VitrioIconName.message,
              label: l10n.orderChatWith(provider.firstName),
              variant: VitrioIconButtonVariant.soft,
              onPressed: () => launchUrl(
                Uri.parse('https://wa.me/${whatsApp.replaceAll(RegExp(r'\D'), '')}'),
                mode: LaunchMode.externalApplication,
              ),
            ),
        ],
      ),
    );
  }
}

class _ProgressCard extends StatelessWidget {
  const _ProgressCard({required this.order, required this.now});

  final CustomerOrder order;
  final DateTime now;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final quote = order.quote;
    final schedule = order.schedule;
    final completed = order.status == OrderStatus.completed;
    final reached = switch (order.status) {
      OrderStatus.sent => 1,
      OrderStatus.quoted => 2,
      OrderStatus.scheduled => 3,
      _ => 4,
    };
    VitrioTimelineStepState state(int step) => switch (step.compareTo(reached)) {
          < 0 => VitrioTimelineStepState.done,
          0 when completed => VitrioTimelineStepState.done,
          0 => VitrioTimelineStepState.current,
          _ => VitrioTimelineStepState.pending,
        };
    String stateLabel(int step) => switch (state(step)) {
          VitrioTimelineStepState.done => l10n.orderStepDone,
          VitrioTimelineStepState.current => l10n.orderStepCurrent,
          VitrioTimelineStepState.pending => l10n.orderStepPending,
        };
    final subtitles = [
      capitalized(relativeMoment(l10n, order.createdAt, now)),
      quote != null ? capitalized(relativeMoment(l10n, quote.sentAt, now)) : l10n.orderStepWaitingQuote(relativeMoment(l10n, order.respondBy, now)),
      switch (order.status) {
        OrderStatus.quoted => l10n.orderStepWaitingYou,
        _ when schedule != null => capitalized(serviceWhen(l10n, schedule, now)),
        _ => null,
      },
      switch (order.status) {
        OrderStatus.scheduled => l10n.orderStepConfirmWhenDone,
        _ when order.completedAt != null => capitalized(relativeMoment(l10n, order.completedAt!, now)),
        _ => null,
      },
    ];
    final titles = [l10n.orderStepSent, l10n.orderStepQuoted, l10n.orderStepScheduled, l10n.orderStepCompleted];
    return Container(
      padding: const EdgeInsets.all(VitrioSpace.sp16),
      decoration: BoxDecoration(
        color: c.surfaceRaised,
        borderRadius: const BorderRadius.all(VitrioRadius.card),
        border: Border.all(color: c.lineSoft),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Semantics(
            header: true,
            child: Text(
              l10n.orderProgress.toUpperCase(),
              style: VitrioText.weighted(VitrioText.meta, 800).copyWith(color: c.inkMuted, letterSpacing: 0.4),
            ),
          ),
          const SizedBox(height: VitrioSpace.sp14),
          VitrioOrderTimeline(
            steps: [
              for (var step = 0; step < titles.length; step++)
                VitrioTimelineStep(
                  title: titles[step],
                  subtitle: subtitles[step],
                  state: state(step),
                  stateLabel: stateLabel(step),
                ),
            ],
          ),
        ],
      ),
    );
  }
}

class _ScheduledCard extends StatelessWidget {
  const _ScheduledCard({required this.order, required this.now, required this.busy, required this.onCancel, required this.onComplete});

  static const _check = 32.0;
  static const _cancelWidth = 120.0;

  final CustomerOrder order;
  final DateTime now;
  final bool busy;
  final VoidCallback onCancel;
  final VoidCallback onComplete;

  String _day(AppLocalizations l10n, DateTime date) {
    final relative = relativeDay(l10n, date, now);
    final exact = shortDate(l10n, date);
    return capitalized(relative == exact ? exact : '$relative, $exact');
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final schedule = order.schedule!;
    final canComplete = order.allows(OrderAction.complete);
    return Container(
      padding: const EdgeInsets.all(VitrioSpace.sp20),
      decoration: BoxDecoration(
        color: c.surfaceRaised,
        borderRadius: const BorderRadius.all(VitrioRadius.hero),
        border: Border.all(color: c.green, width: 1.5),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            children: [
              Container(
                width: _check,
                height: _check,
                alignment: Alignment.center,
                decoration: BoxDecoration(color: c.greenSoft, shape: BoxShape.circle),
                child: VitrioIcon(VitrioIconName.check, size: 16, color: c.green, strokeWidth: 3),
              ),
              const SizedBox(width: VitrioSpace.sp10),
              Text(l10n.orderScheduledTitle, style: VitrioText.bodyStrong.copyWith(color: c.green)),
            ],
          ),
          const SizedBox(height: VitrioSpace.sp12),
          Text(
            l10n.orderScheduledWhen(_day(l10n, schedule.date), serviceRange(schedule)),
            style: VitrioText.title.copyWith(color: c.ink),
          ),
          const SizedBox(height: VitrioSpace.sp8),
          Text(
            l10n.orderScheduledBody(order.provider.firstName, order.quote == null ? '' : formatQuoteAmount(order.quote!.amount)),
            style: VitrioText.body.copyWith(height: 1.5, color: c.ink2),
          ),
          const SizedBox(height: VitrioSpace.sp16),
          Row(
            children: [
              if (order.allows(OrderAction.cancel)) ...[
                SizedBox(
                  width: _cancelWidth,
                  child: VitrioButton(
                    label: l10n.orderScheduledCancel,
                    variant: VitrioButtonVariant.danger,
                    size: VitrioButtonSize.decision,
                    onPressed: busy ? null : onCancel,
                  ),
                ),
                const SizedBox(width: VitrioSpace.sp10),
              ],
              Expanded(
                child: VitrioButton(
                  label: l10n.orderComplete,
                  size: VitrioButtonSize.decision,
                  busy: busy,
                  onPressed: canComplete ? onComplete : null,
                ),
              ),
            ],
          ),
          if (!canComplete) ...[
            const SizedBox(height: VitrioSpace.sp10),
            Text(
              l10n.orderCompleteFrom(schedule.start),
              style: VitrioText.weighted(VitrioText.caption, 600).copyWith(height: 1.5, color: c.inkMuted),
            ),
          ],
        ],
      ),
    );
  }
}

class _DeclinedCard extends StatelessWidget {
  const _DeclinedCard({required this.order});

  final CustomerOrder order;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final decline = order.decline;
    final name = order.provider.firstName;
    final reason = decline == null ? null : decline.details ?? declineReasonLabel(l10n, decline.reason);
    return OrderStatusCard(
      title: order.status == OrderStatus.expired ? l10n.orderExpiredTitle(name) : l10n.orderDeclinedTitle(name),
      body: reason == null ? null : l10n.orderDeclinedReason(reason),
      footnote: l10n.orderKeptForOthers,
      positive: false,
    );
  }
}

class _Suggestions extends ConsumerWidget {
  const _Suggestions({required this.order});

  final CustomerOrder order;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final suggestions = ref.watch(orderSuggestionsProvider(order.id)).value ?? const <ProviderListing>[];
    final category = ref.watch(serviceCategoriesProvider).value?.where((candidate) => candidate.id == order.categoryId).firstOrNull;
    final professionals = category?.professionalPlural?.toLowerCase() ?? l10n.orderSuggestionsFallback;
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        if (suggestions.isNotEmpty) ...[
          const SizedBox(height: VitrioSpace.sp24),
          Semantics(header: true, child: Text(l10n.orderSuggestionsTitle(professionals), style: VitrioText.section.copyWith(color: c.ink))),
          for (final provider in suggestions) ...[
            const SizedBox(height: VitrioSpace.sp12),
            _SuggestionCard(
              provider: provider,
              onOpen: () => PublicProviderRoute(id: provider.id).push<void>(context),
              onRequest: () => RequestQuoteRoute(providerId: provider.id, from: order.id).push<void>(context),
            ),
          ],
        ],
        const SizedBox(height: VitrioSpace.sp14),
        Center(
          child: VitrioButton(
            label: l10n.orderSuggestionsMore(professionals),
            variant: VitrioButtonVariant.ghost,
            trailingIcon: VitrioIconName.arrowRight,
            onPressed: () {
              final filters = ref.read(searchFiltersControllerProvider.notifier);
              if (category != null) {
                filters.startWithCategory(category);
              }
              const SearchRoute().go(context);
            },
          ),
        ),
      ],
    );
  }
}

class _SuggestionCard extends StatelessWidget {
  const _SuggestionCard({required this.provider, required this.onOpen, required this.onRequest});

  static const _avatar = 48.0;

  final ProviderListing provider;
  final VoidCallback onOpen;
  final VoidCallback onRequest;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final rating = provider.ratingScore;
    final distance = provider.distanceKm;
    return Container(
      padding: const EdgeInsets.all(VitrioSpace.sp16),
      decoration: BoxDecoration(
        color: c.surfaceRaised,
        borderRadius: const BorderRadius.all(VitrioRadius.xxl),
        border: Border.all(color: c.lineSoft),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          InkWell(
            onTap: onOpen,
            child: Row(
              children: [
                VitrioAvatar(
                  name: provider.displayName,
                  initials: provider.initials,
                  imageUrl: provider.avatarUrl,
                  size: _avatar,
                  kind: VitrioAvatarKind.provider,
                  seed: provider.id,
                ),
                const SizedBox(width: VitrioSpace.sp12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(provider.displayName, style: VitrioText.bodyStrong.copyWith(color: c.ink)),
                      const SizedBox(height: 3),
                      Wrap(
                        crossAxisAlignment: WrapCrossAlignment.center,
                        spacing: VitrioSpace.sp4,
                        children: [
                          Text('${provider.headline} ·', style: VitrioText.weighted(VitrioText.meta, 400).copyWith(color: c.inkMuted)),
                          if (rating != null && !provider.isNew) ...[
                            const VitrioStar(size: 13),
                            Text(formatRating(rating), style: VitrioText.weighted(VitrioText.meta, 800).copyWith(color: c.ink)),
                            Text('(${provider.reviewCount ?? 0})', style: VitrioText.weighted(VitrioText.meta, 400).copyWith(color: c.inkMuted)),
                          ] else
                            VitrioBadge(label: l10n.providerNew, tone: VitrioBadgeTone.warning),
                          if (distance != null)
                            Text('· ${l10n.providerDistance(formatDistance(distance))}', style: VitrioText.weighted(VitrioText.meta, 400).copyWith(color: c.inkMuted)),
                        ],
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(height: VitrioSpace.sp12),
          VitrioButton(label: l10n.orderRequestThis, variant: VitrioButtonVariant.secondary, size: VitrioButtonSize.md, onPressed: onRequest),
        ],
      ),
    );
  }
}
