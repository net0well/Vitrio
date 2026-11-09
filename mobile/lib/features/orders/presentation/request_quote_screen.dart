import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/routes.dart';
import '../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../core/design_system/tokens/vitrio_radius.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/design_system/widgets/vitrio_action_bar.dart';
import '../../../core/design_system/widgets/vitrio_add_photo_tile.dart';
import '../../../core/design_system/widgets/vitrio_avatar.dart';
import '../../../core/design_system/widgets/vitrio_avatar_kind.dart';
import '../../../core/design_system/widgets/vitrio_badge.dart';
import '../../../core/design_system/widgets/vitrio_badge_tone.dart';
import '../../../core/design_system/widgets/vitrio_button.dart';
import '../../../core/design_system/widgets/vitrio_button_size.dart';
import '../../../core/design_system/widgets/vitrio_button_variant.dart';
import '../../../core/design_system/widgets/vitrio_chip.dart';
import '../../../core/design_system/widgets/vitrio_icon.dart';
import '../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../core/design_system/widgets/vitrio_notice.dart';
import '../../../core/design_system/widgets/vitrio_option.dart';
import '../../../core/design_system/widgets/vitrio_segmented.dart';
import '../../../core/design_system/widgets/vitrio_star.dart';
import '../../../core/design_system/widgets/vitrio_text_field.dart';
import '../../../core/design_system/widgets/vitrio_top_bar.dart';
import '../../../core/network/api_client.dart';
import '../../../core/network/api_result.dart';
import '../../../core/network/problem_details.dart';
import '../../../core/presentation/brazilian_format.dart';
import '../../../core/presentation/clock.dart';
import '../../../core/presentation/problem_message.dart';
import '../../../l10n/app_localizations.dart';
import '../../account/domain/saved_address.dart';
import '../../account/presentation/address_sheet.dart';
import '../../account/presentation/saved_addresses.dart';
import '../../auth/data/auth_providers.dart';
import '../../auth/presentation/session_controller.dart';
import '../../provider/data/provider_providers.dart';
import '../../provider/domain/photo_upload.dart';
import '../../provider/domain/public_provider.dart';
import '../../provider/presentation/provider_queries.dart';
import '../data/orders_providers.dart';
import '../domain/customer_order.dart';
import '../domain/new_order.dart';
import '../domain/preferred_period.dart';
import 'customer_orders_controller.dart';
import 'order_labels.dart';
import 'request_day.dart';
import 'request_photo.dart';

class RequestQuoteScreen extends ConsumerStatefulWidget {
  const RequestQuoteScreen({super.key, required this.providerId, this.fromOrderId});

  final String providerId;
  final String? fromOrderId;

  @override
  ConsumerState<RequestQuoteScreen> createState() => _RequestQuoteScreenState();
}

class _RequestQuoteScreenState extends ConsumerState<RequestQuoteScreen> {
  static const _maxPhotos = 5;
  static const _maxDaysAhead = 30;
  static const _titleLength = (min: 5, max: 80);
  static const _descriptionLength = (min: 20, max: 500);
  static const _minPhoneDigits = 10;
  static const _otherService = '';

  final _title = TextEditingController();
  final _description = TextEditingController();
  final _phone = TextEditingController();
  final _photos = <RequestPhoto>[];
  late final String _idempotencyKey = ApiClient.newIdempotencyKey();

  String? _service;
  RequestDay _day = RequestDay.tomorrow;
  DateTime? _pickedDate;
  PreferredPeriod? _period;
  SavedAddress? _address;
  bool _uploading = false;
  bool _busy = false;
  bool _showErrors = false;
  ProblemDetails? _problem;

  @override
  void initState() {
    super.initState();
    if (widget.fromOrderId case final String orderId) {
      _prefill(orderId);
    }
  }

  @override
  void dispose() {
    _title.dispose();
    _description.dispose();
    _phone.dispose();
    super.dispose();
  }

  Future<void> _prefill(String orderId) async {
    final repository = ref.read(ordersRepositoryProvider);
    final previous = await repository.get(orderId);
    if (!mounted || previous is! ApiSuccess<CustomerOrder>) {
      return;
    }

    final order = previous.data;
    _title.text = order.title;
    _description.text = order.description;
    final copies = order.photos.isEmpty || order.photosRemoved ? null : await repository.copyPhotos(orderId);
    if (!mounted) {
      return;
    }

    setState(() {
      if (copies case ApiSuccess(:final data)) {
        _photos.addAll(data.map((copy) => RequestPhoto(objectKey: copy.objectKey, url: copy.previewUrl)));
      }
    });
  }

  DateTime get _today {
    final now = ref.read(clockProvider)();
    return DateTime(now.year, now.month, now.day);
  }

  DateTime? get _preferredDay => switch (_day) {
        RequestDay.today => _today,
        RequestDay.tomorrow => _today.add(const Duration(days: 1)),
        RequestDay.pick => _pickedDate,
      };

  bool _isOver(PreferredPeriod period) {
    if (_preferredDay != _today) {
      return false;
    }

    final ends = switch (period) {
      PreferredPeriod.morning => 12,
      PreferredPeriod.afternoon => 18,
      PreferredPeriod.evening => 22,
    };
    return ref.read(clockProvider)().hour >= ends;
  }

  Future<void> _pickDate() async {
    final picked = await showDatePicker(
      context: context,
      initialDate: _pickedDate ?? _today.add(const Duration(days: 2)),
      firstDate: _today,
      lastDate: _today.add(const Duration(days: _maxDaysAhead)),
    );
    if (picked != null && mounted) {
      setState(() {
        _day = RequestDay.pick;
        _pickedDate = picked;
        if (_period != null && _isOver(_period!)) {
          _period = null;
        }
      });
    }
  }

  void _selectDay(RequestDay day) {
    if (day == RequestDay.pick) {
      _pickDate();
      return;
    }

    setState(() {
      _day = day;
      if (_period != null && _isOver(_period!)) {
        _period = null;
      }
    });
  }

  Future<void> _addPhoto() async {
    final picked = await ref.read(photoPickerProvider).pick();
    if (picked == null || !mounted) {
      return;
    }

    setState(() => _uploading = true);
    final upload = await ref.read(ordersRepositoryProvider).photoUpload(contentType: picked.contentType, contentLength: picked.bytes.length);
    final stored = upload is ApiSuccess<PhotoUpload> && await ref.read(photoStorageProvider).upload(upload.data, picked);
    if (!mounted) {
      return;
    }

    setState(() {
      _uploading = false;
      if (upload case ApiSuccess(:final data) when stored) {
        _photos.add(RequestPhoto(objectKey: data.objectKey, bytes: picked.bytes));
      }
    });
    if (!stored) {
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(AppLocalizations.of(context).requestQuotePhotoFailed)));
    }
  }

  Future<void> _changeAddress(List<SavedAddress> addresses, SavedAddress? current) async {
    final chosen = await AddressSheet.show(context, addresses: addresses, selectedId: current?.id);
    if (chosen != null && mounted) {
      setState(() => _address = chosen);
    }
  }

  bool get _needsPhone => ref.read(sessionControllerProvider).value?.phoneNumber == null;

  Map<String, String> _validate(AppLocalizations l10n, SavedAddress? address) {
    final title = _title.text.trim().length;
    final description = _description.text.trim().length;
    return {
      if (_service == null) 'service': l10n.requestQuoteChooseService,
      if (title < _titleLength.min || title > _titleLength.max) 'title': l10n.requestQuoteSummaryInvalid,
      if (description < _descriptionLength.min || description > _descriptionLength.max) 'description': l10n.requestQuoteDescriptionInvalid,
      if (_period == null || _preferredDay == null || _isOver(_period!)) 'period': l10n.requestQuoteChoosePeriod,
      if (address == null) 'address': l10n.requestQuoteAddressRequired,
      if (_needsPhone && _phone.text.replaceAll(RegExp(r'\D'), '').length < _minPhoneDigits) 'phone': l10n.requestQuotePhoneInvalid,
    };
  }

  Future<void> _send(SavedAddress? address) async {
    final l10n = AppLocalizations.of(context);
    if (_validate(l10n, address).isNotEmpty) {
      setState(() => _showErrors = true);
      return;
    }

    setState(() {
      _busy = true;
      _problem = null;
    });

    if (_needsPhone) {
      final saved = await ref.read(accountRepositoryProvider).updateMe(phoneNumber: _phone.text.trim());
      if (!mounted) {
        return;
      }

      switch (saved) {
        case ApiSuccess(:final data):
          ref.read(sessionControllerProvider.notifier).replaceProfile(data);
        case ApiFailure(:final problem):
          setState(() {
            _busy = false;
            _problem = problem;
          });
          return;
      }
    }

    final result = await ref.read(ordersRepositoryProvider).create(
          NewOrder(
            providerId: widget.providerId,
            serviceId: _service == _otherService ? null : _service,
            title: _title.text.trim(),
            description: _description.text.trim(),
            preferredDay: _preferredDay!,
            preferredPeriod: _period!,
            addressId: address!.id,
            photoKeys: [for (final photo in _photos) photo.objectKey],
          ),
          idempotencyKey: _idempotencyKey,
        );
    if (!mounted) {
      return;
    }

    switch (result) {
      case ApiSuccess(:final data):
        ref.invalidate(customerOrdersControllerProvider);
        ref.invalidate(latestOpenOrderProvider);
        OrderRoute(id: data.id).go(context);
      case ApiFailure(:final problem):
        setState(() {
          _busy = false;
          _problem = problem;
        });
    }
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final provider = ref.watch(publicProviderProvider(widget.providerId));
    final addresses = ref.watch(savedAddressesProvider);
    final address = _address ?? addresses.value?.firstOrNull;
    final errors = _showErrors ? _validate(l10n, address) : const <String, String>{};
    final serverMessage = _problem == null || _problem!.isValidation ? null : problemMessage(l10n, _problem!);

    return Scaffold(
      appBar: VitrioTopBar(title: l10n.requestQuoteTitle),
      body: ListView(
        padding: const EdgeInsets.fromLTRB(VitrioSpace.sp20, VitrioSpace.sp16, VitrioSpace.sp20, VitrioSpace.sp24),
        children: [
          if (provider.value case final PublicProvider value) _ProviderSummary(provider: value),
          const SizedBox(height: 22),
          _SectionTitle(l10n.requestQuoteService),
          const SizedBox(height: VitrioSpace.sp10),
          Wrap(
            spacing: VitrioSpace.sp8,
            runSpacing: VitrioSpace.sp8,
            children: [
              for (final service in [...?provider.value?.services])
                if (service.id case final String id)
                  VitrioChip(label: service.title, selected: _service == id, onSelected: (_) => setState(() => _service = id)),
              VitrioChip(
                label: l10n.orderReasonOther,
                selected: _service == _otherService,
                onSelected: (_) => setState(() => _service = _otherService),
              ),
            ],
          ),
          if (errors['service'] case final String error) _FieldError(error),
          const SizedBox(height: 18),
          _SectionTitle(l10n.requestQuoteSummary),
          const SizedBox(height: VitrioSpace.sp10),
          VitrioTextField(
            label: l10n.requestQuoteSummary,
            showLabel: false,
            controller: _title,
            placeholder: l10n.requestQuoteSummaryPlaceholder,
            maxLength: _titleLength.max,
            textCapitalization: TextCapitalization.sentences,
            error: errors['title'] ?? fieldError(_problem, 'title'),
            onChanged: (_) => setState(() {}),
          ),
          const SizedBox(height: VitrioSpace.sp14),
          _SectionTitle(l10n.requestQuoteDescription),
          const SizedBox(height: VitrioSpace.sp10),
          VitrioTextField(
            label: l10n.requestQuoteDescription,
            showLabel: false,
            controller: _description,
            placeholder: l10n.requestQuoteDescriptionPlaceholder,
            hint: l10n.requestQuoteDescriptionHint,
            maxLength: _descriptionLength.max,
            minLines: 4,
            maxLines: 6,
            textCapitalization: TextCapitalization.sentences,
            error: errors['description'] ?? fieldError(_problem, 'description'),
            onChanged: (_) => setState(() {}),
          ),
          const SizedBox(height: 18),
          _SectionTitle(l10n.requestQuotePhotos, optional: l10n.requestQuoteOptional),
          const SizedBox(height: VitrioSpace.sp10),
          Wrap(
            spacing: VitrioSpace.sp10,
            runSpacing: VitrioSpace.sp10,
            children: [
              for (final photo in _photos)
                _PhotoThumbnail(
                  photo: photo,
                  removeLabel: l10n.requestQuoteRemovePhoto,
                  onRemove: () => setState(() => _photos.remove(photo)),
                ),
              if (_photos.length < _maxPhotos)
                SizedBox.square(
                  dimension: _PhotoThumbnail.size,
                  child: VitrioAddPhotoTile(label: l10n.requestQuoteAddPhoto, busy: _uploading, onTap: _addPhoto),
                ),
            ],
          ),
          const SizedBox(height: 18),
          _SectionTitle(l10n.requestQuoteWhen),
          const SizedBox(height: VitrioSpace.sp10),
          VitrioSegmented<RequestDay>(
            label: l10n.requestQuoteDay,
            value: _day,
            onChanged: _selectDay,
            options: [
              VitrioOption(value: RequestDay.today, label: l10n.requestQuoteToday),
              VitrioOption(value: RequestDay.tomorrow, label: l10n.requestQuoteTomorrow),
              VitrioOption(
                value: RequestDay.pick,
                label: _day == RequestDay.pick && _pickedDate != null ? capitalized(shortDate(l10n, _pickedDate!)) : l10n.requestQuotePickDate,
              ),
            ],
          ),
          const SizedBox(height: VitrioSpace.sp10),
          Semantics(
            container: true,
            label: l10n.requestQuotePeriod,
            explicitChildNodes: true,
            child: Row(
              children: [
                for (final (index, period) in PreferredPeriod.values.indexed) ...[
                  if (index > 0) const SizedBox(width: VitrioSpace.sp8),
                  Expanded(
                    child: _PeriodOption(
                      label: periodLabel(l10n, period),
                      hours: periodHours(l10n, period),
                      selected: _period == period,
                      over: _isOver(period),
                      overLabel: l10n.requestQuotePeriodOver,
                      onTap: () => setState(() => _period = period),
                    ),
                  ),
                ],
              ],
            ),
          ),
          if (errors['period'] case final String error) _FieldError(error),
          const SizedBox(height: 18),
          _SectionTitle(l10n.requestQuoteWhere),
          const SizedBox(height: VitrioSpace.sp10),
          _AddressCard(
            address: address,
            onChange: () => _changeAddress(addresses.value ?? const [], address),
          ),
          if (errors['address'] case final String error) _FieldError(error),
          const SizedBox(height: VitrioSpace.sp10),
          Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Padding(
                padding: const EdgeInsets.only(top: 1),
                child: VitrioIcon(VitrioIconName.lock, size: 16, color: c.inkMuted),
              ),
              const SizedBox(width: VitrioSpace.sp8),
              Expanded(
                child: Text(
                  l10n.requestQuoteAddressPrivacy,
                  style: VitrioText.weighted(VitrioText.caption, 600).copyWith(height: 1.5, color: c.inkMuted),
                ),
              ),
            ],
          ),
          if (_needsPhone) ...[
            const SizedBox(height: 18),
            _SectionTitle(l10n.requestQuotePhone),
            const SizedBox(height: VitrioSpace.sp10),
            VitrioTextField(
              label: l10n.requestQuotePhone,
              showLabel: false,
              controller: _phone,
              hint: l10n.requestQuotePhoneHint,
              keyboardType: TextInputType.phone,
              autofillHints: const [AutofillHints.telephoneNumber],
              inputFormatters: [FilteringTextInputFormatter.allow(RegExp(r'[0-9 ()+-]'))],
              error: errors['phone'] ?? fieldError(_problem, 'phoneNumber'),
              onChanged: (_) => setState(() {}),
            ),
          ],
        ],
      ),
      bottomNavigationBar: VitrioActionBar(
        note: l10n.requestQuoteFree,
        top: serverMessage == null ? null : VitrioNotice(message: serverMessage),
        children: [
          VitrioButton(
            label: l10n.requestQuoteSend,
            trailingIcon: VitrioIconName.send,
            busy: _busy,
            onPressed: provider.hasValue ? () => _send(address) : null,
          ),
        ],
      ),
    );
  }
}

class _SectionTitle extends StatelessWidget {
  const _SectionTitle(this.text, {this.optional});

  final String text;
  final String? optional;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    return Semantics(
      header: true,
      child: Text.rich(
        TextSpan(
          text: text,
          style: VitrioText.price.copyWith(color: c.ink),
          children: [
            if (optional != null)
              TextSpan(text: ' $optional', style: VitrioText.meta.copyWith(color: c.inkMuted)),
          ],
        ),
      ),
    );
  }
}

class _FieldError extends StatelessWidget {
  const _FieldError(this.message);

  final String message;

  @override
  Widget build(BuildContext context) => Padding(
        padding: const EdgeInsets.only(top: VitrioSpace.sp6),
        child: Text(message, style: VitrioText.weighted(VitrioText.caption, 600).copyWith(color: context.vitrio.danger)),
      );
}

class _ProviderSummary extends StatelessWidget {
  const _ProviderSummary({required this.provider});

  final PublicProvider provider;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final rating = provider.ratingScore;
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp14, vertical: VitrioSpace.sp12),
      decoration: BoxDecoration(
        color: c.surfaceRaised,
        borderRadius: const BorderRadius.all(VitrioRadius.xxl),
        border: Border.all(color: c.lineSoft),
      ),
      child: Row(
        children: [
          VitrioAvatar(name: provider.displayName, imageUrl: provider.avatarUrl, size: VitrioSpace.tap, kind: VitrioAvatarKind.provider, seed: provider.id),
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
                      Text(' (${provider.reviewCount ?? 0})', style: VitrioText.weighted(VitrioText.meta, 400).copyWith(color: c.inkMuted)),
                    ],
                  ],
                ),
              ],
            ),
          ),
          if (responseTimeLabel(l10n, provider.responseTime) case final String time) ...[
            const SizedBox(width: VitrioSpace.sp8),
            VitrioBadge(label: time, tone: VitrioBadgeTone.verified),
          ],
        ],
      ),
    );
  }
}

class _PhotoThumbnail extends StatelessWidget {
  const _PhotoThumbnail({required this.photo, required this.removeLabel, required this.onRemove});

  static const size = 76.0;
  static const _removeSize = 24.0;

  final RequestPhoto photo;
  final String removeLabel;
  final VoidCallback onRemove;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    final image = switch (photo) {
      RequestPhoto(:final bytes?) => Image.memory(bytes, fit: BoxFit.cover),
      RequestPhoto(:final url?) => Image.network(url, fit: BoxFit.cover, errorBuilder: (_, _, _) => ColoredBox(color: c.plumSoft)),
      _ => ColoredBox(color: c.plumSoft),
    };
    return SizedBox.square(
      dimension: size + VitrioSpace.sp6,
      child: Stack(
        clipBehavior: Clip.none,
        children: [
          Positioned(
            left: 0,
            bottom: 0,
            width: size,
            height: size,
            child: ClipRRect(borderRadius: const BorderRadius.all(VitrioRadius.lg), child: image),
          ),
          Positioned(
            top: 0,
            right: 0,
            child: Semantics(
              button: true,
              label: removeLabel,
              child: GestureDetector(
                onTap: onRemove,
                child: Container(
                  width: _removeSize,
                  height: _removeSize,
                  alignment: Alignment.center,
                  decoration: BoxDecoration(
                    color: c.ink,
                    shape: BoxShape.circle,
                    border: Border.all(color: c.surface, width: 2),
                  ),
                  child: VitrioIcon(VitrioIconName.x, size: 11, color: c.surfaceRaised, strokeWidth: 3),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _PeriodOption extends StatelessWidget {
  const _PeriodOption({
    required this.label,
    required this.hours,
    required this.selected,
    required this.over,
    required this.overLabel,
    required this.onTap,
  });

  static const _height = 54.0;

  final String label;
  final String hours;
  final bool selected;
  final bool over;
  final String overLabel;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    final chosen = selected && !over;
    return Semantics(
      button: true,
      selected: chosen,
      enabled: !over,
      hint: over ? overLabel : null,
      child: Opacity(
        opacity: over ? 0.4 : 1,
        child: Material(
          color: Colors.transparent,
          child: InkWell(
            borderRadius: const BorderRadius.all(VitrioRadius.lg),
            onTap: over ? null : onTap,
            child: AnimatedContainer(
              duration: const Duration(milliseconds: 180),
              curve: Curves.easeOut,
              height: _height,
              decoration: BoxDecoration(
                color: chosen ? c.plumSoft : c.surfaceRaised,
                borderRadius: const BorderRadius.all(VitrioRadius.lg),
                border: Border.all(color: chosen ? c.plum : c.line, width: chosen ? 1.5 : 1),
              ),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Text(label, style: VitrioText.weighted(VitrioText.label, chosen ? 800 : 700).copyWith(color: chosen ? c.plum : c.ink)),
                  const SizedBox(height: 1),
                  Text(
                    hours,
                    style: VitrioText.weighted(VitrioText.micro, chosen ? 700 : 600).copyWith(color: chosen ? c.plum : c.inkMuted),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}

class _AddressCard extends StatelessWidget {
  const _AddressCard({required this.address, required this.onChange});

  static const _box = 44.0;

  final SavedAddress? address;
  final VoidCallback onChange;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final current = address;
    return Container(
      padding: const EdgeInsets.all(VitrioSpace.sp14),
      decoration: BoxDecoration(
        color: c.surfaceRaised,
        borderRadius: const BorderRadius.all(VitrioRadius.xl),
        border: Border.all(color: c.lineSoft),
      ),
      child: Row(
        children: [
          Container(
            width: _box,
            height: _box,
            alignment: Alignment.center,
            decoration: BoxDecoration(color: c.plumSoft, borderRadius: const BorderRadius.all(VitrioRadius.lg)),
            child: VitrioIcon(VitrioIconName.pin, color: c.plum),
          ),
          const SizedBox(width: VitrioSpace.sp12),
          Expanded(
            child: current == null
                ? Text(l10n.requestQuoteNoAddress, style: VitrioText.weighted(VitrioText.meta, 600).copyWith(color: c.ink2))
                : Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(l10n.addressSummary(current.label, current.neighborhood), style: VitrioText.bodyStrong.copyWith(color: c.ink)),
                      const SizedBox(height: 2),
                      Text(
                        l10n.addressCity(current.city, current.state),
                        style: VitrioText.weighted(VitrioText.meta, 400).copyWith(color: c.inkMuted),
                      ),
                    ],
                  ),
          ),
          const SizedBox(width: VitrioSpace.sp8),
          VitrioButton(
            label: current == null ? l10n.requestQuoteAddAddress : l10n.requestQuoteChangeAddress,
            variant: VitrioButtonVariant.secondary,
            size: VitrioButtonSize.sm,
            onPressed: onChange,
          ),
        ],
      ),
    );
  }
}
