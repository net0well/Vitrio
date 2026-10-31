import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/design_system/widgets/vitrio_button.dart';
import '../../../core/design_system/widgets/vitrio_button_size.dart';
import '../../../core/design_system/widgets/vitrio_button_variant.dart';
import '../../../core/design_system/widgets/vitrio_chip.dart';
import '../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../core/design_system/widgets/vitrio_list_card.dart';
import '../../../core/design_system/widgets/vitrio_list_row.dart';
import '../../../core/design_system/widgets/vitrio_switch_row.dart';
import '../../../core/presentation/brazilian_format.dart';
import '../../../l10n/app_localizations.dart';
import '../../location/domain/location_mode.dart';
import '../../location/presentation/city_picker_sheet.dart';
import '../../location/presentation/location_controller.dart';
import '../domain/search_filters.dart';
import 'search_filters_controller.dart';
import 'search_labels.dart';

class FiltersSheet extends ConsumerStatefulWidget {
  const FiltersSheet({super.key, required this.nearMe});

  final bool nearMe;

  static Future<void> show(BuildContext context, {required bool nearMe}) => showModalBottomSheet<void>(
        context: context,
        isScrollControlled: true,
        useSafeArea: true,
        useRootNavigator: true,
        showDragHandle: true,
        builder: (_) => FiltersSheet(nearMe: nearMe),
      );

  @override
  ConsumerState<FiltersSheet> createState() => _FiltersSheetState();
}

class _FiltersSheetState extends ConsumerState<FiltersSheet> {
  static const _defaultDistanceKm = 20;
  static const _ratings = [3.0, 4.0, 4.5];
  static const _prices = [50, 100, 200, 500];

  late SearchFilters _draft = ref.read(searchFiltersControllerProvider);

  void _apply() {
    ref.read(searchFiltersControllerProvider.notifier).apply(_draft);
    Navigator.of(context).pop();
  }

  void _reset() => setState(
        () => _draft = SearchFilters(
          query: _draft.query,
          category: _draft.category,
          sort: _draft.sort,
        ),
      );

  String _ratingLabel(AppLocalizations l10n, double rating) => l10n.searchFilterRatingValue(
        rating == rating.roundToDouble() ? rating.toInt().toString() : formatRating(rating),
      );

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final location = ref.watch(locationControllerProvider).value;
    final distance = (_draft.maxDistanceKm ?? _defaultDistanceKm).toDouble();
    Widget title(String text) => Text(text, style: VitrioText.label.copyWith(color: c.ink));

    return Column(
      mainAxisSize: MainAxisSize.min,
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Flexible(
          child: SingleChildScrollView(
            padding: const EdgeInsets.fromLTRB(
              VitrioSpace.sp20,
              0,
              VitrioSpace.sp20,
              VitrioSpace.sp16,
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Semantics(
                  header: true,
                  child: Text(
                    l10n.searchFilters,
                    style: VitrioText.title.copyWith(color: c.ink),
                  ),
                ),
                const SizedBox(height: VitrioSpace.sp20),
                if (widget.nearMe) ...[
                  Row(
                    children: [
                      Expanded(child: title(l10n.searchFilterDistance)),
                      Text(
                        l10n.searchFilterDistanceValue(
                          distance.round().toString(),
                        ),
                        style: VitrioText.label.copyWith(color: c.plum),
                      ),
                    ],
                  ),
                  Slider(
                    value: distance,
                    min: 1,
                    max: 50,
                    divisions: 49,
                    label: l10n.searchFilterDistanceValue(
                      distance.round().toString(),
                    ),
                    semanticFormatterCallback: (value) => l10n.searchFilterDistanceValue(value.round().toString()),
                    onChanged: (value) => setState(
                      () => _draft = _draft.copyWith(
                        maxDistanceKm: () => value.round(),
                      ),
                    ),
                  ),
                  const SizedBox(height: VitrioSpace.sp12),
                ],
                title(l10n.searchFilterRating),
                const SizedBox(height: VitrioSpace.sp10),
                Wrap(
                  spacing: VitrioSpace.sp8,
                  runSpacing: VitrioSpace.sp8,
                  children: [
                    VitrioChip(
                      label: l10n.searchFilterAny,
                      selected: _draft.minRating == null,
                      onSelected: (_) => setState(
                        () => _draft = _draft.copyWith(minRating: () => null),
                      ),
                    ),
                    for (final rating in _ratings)
                      VitrioChip(
                        label: _ratingLabel(l10n, rating),
                        selected: _draft.minRating == rating,
                        onSelected: (_) => setState(
                          () => _draft = _draft.copyWith(minRating: () => rating),
                        ),
                      ),
                  ],
                ),
                const SizedBox(height: VitrioSpace.sp20),
                VitrioSwitchRow(
                  label: l10n.providerBadgeMei,
                  description: l10n.searchFilterMeiHint,
                  value: _draft.meiVerified,
                  onChanged: (value) => setState(
                    () => _draft = _draft.copyWith(meiVerified: value),
                  ),
                ),
                const SizedBox(height: VitrioSpace.sp12),
                VitrioSwitchRow(
                  label: l10n.providerBadgeToday,
                  description: l10n.searchFilterTodayHint,
                  value: _draft.availableToday,
                  onChanged: (value) => setState(
                    () => _draft = _draft.copyWith(availableToday: value),
                  ),
                ),
                const SizedBox(height: VitrioSpace.sp20),
                title(l10n.searchFilterPrice),
                const SizedBox(height: VitrioSpace.sp10),
                Wrap(
                  spacing: VitrioSpace.sp8,
                  runSpacing: VitrioSpace.sp8,
                  children: [
                    VitrioChip(
                      label: l10n.searchFilterAny,
                      selected: _draft.maxPriceFrom == null,
                      onSelected: (_) => setState(
                        () => _draft = _draft.copyWith(maxPriceFrom: () => null),
                      ),
                    ),
                    for (final price in _prices)
                      VitrioChip(
                        label: l10n.searchFilterPriceValue(
                          formatMoney(price.toDouble()),
                        ),
                        selected: _draft.maxPriceFrom == price,
                        onSelected: (_) => setState(
                          () => _draft = _draft.copyWith(
                            maxPriceFrom: () => price,
                          ),
                        ),
                      ),
                  ],
                ),
                const SizedBox(height: VitrioSpace.sp20),
                VitrioListCard(
                  children: [
                    VitrioListRow(
                      icon: VitrioIconName.pin,
                      title: l10n.searchFilterCity,
                      subtitle: switch ((location?.mode, location?.city)) {
                        (_, null) => l10n.locationChooseCity,
                        (LocationMode.nearMe, _) => l10n.searchNearMe,
                        (_, final city?) => cityLabel(city),
                      },
                      onTap: () => CityPickerSheet.show(context),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ),
        SafeArea(
          top: false,
          child: Padding(
            padding: const EdgeInsets.fromLTRB(
              VitrioSpace.sp20,
              VitrioSpace.sp8,
              VitrioSpace.sp20,
              VitrioSpace.sp16,
            ),
            child: Row(
              children: [
                Expanded(
                  child: VitrioButton(
                    label: l10n.searchFilterReset,
                    variant: VitrioButtonVariant.secondary,
                    size: VitrioButtonSize.md,
                    onPressed: _reset,
                  ),
                ),
                const SizedBox(width: VitrioSpace.sp12),
                Expanded(
                  child: VitrioButton(
                    label: l10n.searchFilterApply,
                    size: VitrioButtonSize.md,
                    onPressed: _apply,
                  ),
                ),
              ],
            ),
          ),
        ),
      ],
    );
  }
}
