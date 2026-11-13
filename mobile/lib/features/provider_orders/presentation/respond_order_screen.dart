import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/routes.dart';
import '../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/design_system/widgets/vitrio_action_bar.dart';
import '../../../core/design_system/widgets/vitrio_badge_tone.dart';
import '../../../core/design_system/widgets/vitrio_button.dart';
import '../../../core/design_system/widgets/vitrio_button_variant.dart';
import '../../../core/design_system/widgets/vitrio_empty_state.dart';
import '../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../core/design_system/widgets/vitrio_notice.dart';
import '../../../core/design_system/widgets/vitrio_option.dart';
import '../../../core/design_system/widgets/vitrio_segmented.dart';
import '../../../core/design_system/widgets/vitrio_switch_row.dart';
import '../../../core/design_system/widgets/vitrio_text_field.dart';
import '../../../core/design_system/widgets/vitrio_top_bar.dart';
import '../../../core/network/api_client.dart';
import '../../../core/network/api_result.dart';
import '../../../core/network/problem_details.dart';
import '../../../core/presentation/clock.dart';
import '../../../core/presentation/problem_message.dart';
import '../../../l10n/app_localizations.dart';
import '../../orders/domain/city_time.dart';
import '../../orders/domain/order_action.dart';
import '../../orders/domain/preferred_period.dart';
import '../../orders/domain/quote_type.dart';
import '../../orders/presentation/order_labels.dart';
import '../../provider/presentation/provider_controller.dart';
import '../domain/provider_order.dart';
import '../domain/quote_draft.dart';
import 'decline_order_sheet.dart';
import 'provider_agenda_queries.dart';
import 'provider_order_controller.dart';
import 'schedule_slots.dart';
import 'widgets/order_request_card.dart';
import 'widgets/schedule_fields.dart';

class RespondOrderScreen extends ConsumerWidget {
  const RespondOrderScreen({super.key, required this.orderId});

  final String orderId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final order = ref.watch(providerOrderControllerProvider(orderId));
    return switch (order) {
      AsyncData(:final value) => _RespondForm(order: value),
      AsyncError() => Scaffold(
        appBar: VitrioTopBar(title: l10n.respondTitle),
        body: Center(
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
      ),
      _ => Scaffold(
        appBar: VitrioTopBar(title: l10n.respondTitle),
        body: const Center(child: CircularProgressIndicator()),
      ),
    };
  }
}

class _RespondForm extends ConsumerStatefulWidget {
  const _RespondForm({required this.order});

  final ProviderOrder order;

  @override
  ConsumerState<_RespondForm> createState() => _RespondFormState();
}

class _RespondFormState extends ConsumerState<_RespondForm> {
  static const _maxMaterial = 80;
  static const _maxMessage = 500;
  static const _amount = (min: 1.0, max: 100000.0);
  static const _defaultDuration = 60;
  static const _declineWidth = 110.0;

  final _price = TextEditingController();
  final _material = TextEditingController();
  final _message = TextEditingController();
  late final String _idempotencyKey = ApiClient.newIdempotencyKey();

  QuoteType _type = QuoteType.fixedPrice;
  late DateTime _day;
  late int _start;
  int _duration = _defaultDuration;
  bool _materialIncluded = true;
  bool _showErrors = false;
  bool _busy = false;
  ProblemDetails? _problem;

  DateTime get _now => cityNow(ref.read(clockProvider)(), widget.order.cityOffset);

  @override
  void initState() {
    super.initState();
    final quote = widget.order.quote;
    final days = scheduleDays(_now);
    if (quote != null) {
      _type = quote.type;
      _price.text = quote.amount.toStringAsFixed(2).replaceAll('.', ',');
      _materialIncluded = quote.materialIncluded;
      _material.text = quote.materialDetails ?? '';
      _message.text = quote.message ?? '';
      _duration = quote.time.durationMinutes;
    }

    final wantedDay = quote?.time.date ?? widget.order.preferredDay;
    _day = days.firstWhere((day) => !day.isBefore(dayOf(wantedDay)), orElse: () => days.first);
    _start = closestSlot(startSlots(_day, _now), quote == null ? _periodStart(widget.order.preferredPeriod) : parseClock(quote.time.start));
    _duration = closestSlot(durationSlots(_start), _duration);
  }

  @override
  void dispose() {
    _price.dispose();
    _material.dispose();
    _message.dispose();
    super.dispose();
  }

  static int _periodStart(PreferredPeriod period) => switch (period) {
    PreferredPeriod.morning => 8 * 60,
    PreferredPeriod.afternoon => 14 * 60,
    PreferredPeriod.evening => 19 * 60,
  };

  double? get _amountValue => double.tryParse(_price.text.trim().replaceAll('.', '').replaceAll(',', '.'));

  String? _priceError(AppLocalizations l10n) {
    final amount = _amountValue;
    return amount == null || amount < _amount.min || amount > _amount.max ? l10n.respondPriceInvalid : null;
  }

  void _selectDay(DateTime day) => setState(() {
    _day = day;
    _start = closestSlot(startSlots(day, _now), _start);
    _duration = closestSlot(durationSlots(_start), _duration);
  });

  void _selectStart(int start) => setState(() {
    _start = start;
    _duration = closestSlot(durationSlots(start), _duration);
  });

  Future<void> _send() async {
    final l10n = AppLocalizations.of(context);
    if (_priceError(l10n) != null) {
      setState(() => _showErrors = true);
      return;
    }

    setState(() {
      _busy = true;
      _problem = null;
    });
    final material = _material.text.trim();
    final message = _message.text.trim();
    final result = await ref
        .read(providerOrderControllerProvider(widget.order.id).notifier)
        .sendQuote(
          QuoteDraft(
            type: _type,
            amount: _amountValue!,
            date: _day,
            start: clockLabel(_start),
            durationMinutes: _duration,
            materialIncluded: _materialIncluded,
            materialDetails: _materialIncluded && material.isNotEmpty ? material : null,
            message: message.isEmpty ? null : message,
          ),
          idempotencyKey: _idempotencyKey,
        );
    if (!mounted) {
      return;
    }

    switch (result) {
      case ApiSuccess():
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(l10n.providerQuoteSentTitle)));
        _leave();
      case ApiFailure(:final problem):
        setState(() {
          _busy = false;
          _problem = problem;
        });
    }
  }

  Future<void> _decline() async {
    final l10n = AppLocalizations.of(context);
    final provider = ref.read(providerControllerProvider).value;
    final choice = await DeclineOrderSheet.show(
      context,
      customerFirstName: widget.order.customer.firstName,
      providerFirstName: provider?.displayName.split(' ').first ?? '',
    );
    if (choice == null || !mounted) {
      return;
    }

    setState(() {
      _busy = true;
      _problem = null;
    });
    final result = await ref.read(providerOrderControllerProvider(widget.order.id).notifier).decline(choice.reason, choice.details);
    if (!mounted) {
      return;
    }

    switch (result) {
      case ApiSuccess():
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(l10n.providerDeclinedTitle)));
        _leave();
      case ApiFailure(:final problem):
        setState(() {
          _busy = false;
          _problem = problem;
        });
    }
  }

  void _leave() {
    final navigator = Navigator.of(context);
    if (navigator.canPop()) {
      navigator.pop();
    } else {
      ProviderOrderRoute(id: widget.order.id).go(context);
    }
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final order = widget.order;
    final now = cityNow(ref.watch(clockProvider)(), order.cityOffset);
    final name = order.customer.firstName;
    final replacing = order.quote != null;
    final agenda = ref.watch(providerAgendaProvider(_day, _day)).value ?? const [];
    final conflict = overlapping(agenda, day: _day, start: _start, end: _start + _duration, orderId: order.id);
    final canAnswer = order.allows(OrderAction.sendQuote) || order.allows(OrderAction.replaceQuote);
    final serverMessage = _problem == null ? null : problemMessage(l10n, _problem!);
    final days = scheduleDays(now);
    final starts = startSlots(_day, now);

    return Scaffold(
      appBar: VitrioTopBar(title: l10n.respondTitle),
      body: ListView(
        padding: const EdgeInsets.fromLTRB(VitrioSpace.sp20, VitrioSpace.sp16, VitrioSpace.sp20, VitrioSpace.sp24),
        children: [
          OrderRequestCard(order: order),
          const SizedBox(height: VitrioSpace.sp24),
          Semantics(
            header: true,
            child: Text(l10n.respondYourQuote, style: VitrioText.section.copyWith(color: c.ink)),
          ),
          if (replacing) ...[
            const SizedBox(height: VitrioSpace.sp12),
            VitrioNotice(message: l10n.respondReplaceNote(formatQuoteAmount(order.quote!.amount)), tone: VitrioBadgeTone.info),
          ],
          const SizedBox(height: VitrioSpace.sp12),
          VitrioSegmented<QuoteType>(
            label: l10n.respondQuoteType,
            value: _type,
            onChanged: (type) => setState(() => _type = type),
            options: [
              VitrioOption(value: QuoteType.fixedPrice, label: l10n.quoteTypeFixedPrice),
              VitrioOption(value: QuoteType.technicalVisit, label: l10n.quoteTypeTechnicalVisit),
            ],
          ),
          const SizedBox(height: VitrioSpace.sp14),
          VitrioTextField(
            label: _type == QuoteType.fixedPrice ? l10n.respondPriceFixed : l10n.respondPriceVisit,
            controller: _price,
            prefix: r'R$',
            keyboardType: const TextInputType.numberWithOptions(decimal: true),
            inputFormatters: [FilteringTextInputFormatter.allow(RegExp(r'[0-9.,]'))],
            error: _showErrors ? _priceError(l10n) : null,
            onChanged: (_) => setState(() {}),
          ),
          const SizedBox(height: VitrioSpace.sp14),
          ScheduleFields(
            dayLabel: l10n.respondDay,
            days: days,
            today: now,
            day: _day,
            starts: starts,
            start: _start,
            durations: durationSlots(_start),
            duration: _duration,
            onDay: _selectDay,
            onStart: _selectStart,
            onDuration: (duration) => setState(() => _duration = duration),
            overlap: conflict,
          ),
          const SizedBox(height: VitrioSpace.sp14),
          VitrioSwitchRow(
            label: l10n.respondMaterial,
            description: _materialIncluded ? l10n.respondMaterialYours : l10n.respondMaterialCustomer(name),
            value: _materialIncluded,
            onChanged: (value) => setState(() => _materialIncluded = value),
          ),
          if (_materialIncluded) ...[
            const SizedBox(height: VitrioSpace.sp12),
            Text.rich(
              TextSpan(
                text: l10n.respondMaterialWhat,
                style: VitrioText.label.copyWith(color: c.ink),
                children: [
                  TextSpan(
                    text: ' ${l10n.requestQuoteOptional}',
                    style: VitrioText.meta.copyWith(color: c.inkMuted),
                  ),
                ],
              ),
            ),
            const SizedBox(height: VitrioSpace.sp6),
            VitrioTextField(
              label: l10n.respondMaterialWhat,
              showLabel: false,
              controller: _material,
              placeholder: l10n.respondMaterialPlaceholder,
              maxLength: _maxMaterial,
              textCapitalization: TextCapitalization.sentences,
            ),
          ],
          const SizedBox(height: VitrioSpace.sp14),
          VitrioTextField(
            label: l10n.respondMessage(name),
            controller: _message,
            placeholder: l10n.respondMessagePlaceholder,
            maxLength: _maxMessage,
            minLines: 4,
            maxLines: 6,
            textCapitalization: TextCapitalization.sentences,
          ),
        ],
      ),
      bottomNavigationBar: VitrioActionBar(
        note: l10n.respondNote(name),
        top: serverMessage == null ? null : VitrioNotice(message: serverMessage),
        children: [
          if (order.allows(OrderAction.decline))
            SizedBox(
              width: _declineWidth,
              child: VitrioButton(
                label: l10n.orderQuoteDecline,
                variant: VitrioButtonVariant.secondary,
                onPressed: _busy ? null : _decline,
              ),
            ),
          VitrioButton(
            label: replacing ? l10n.providerReplaceQuote : l10n.providerSendQuote,
            trailingIcon: VitrioIconName.send,
            busy: _busy,
            onPressed: canAnswer && starts.isNotEmpty ? _send : null,
          ),
        ],
      ),
    );
  }
}
