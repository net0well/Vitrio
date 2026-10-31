import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/routes.dart';
import '../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/design_system/widgets/vitrio_button.dart';
import '../../../core/design_system/widgets/vitrio_button_size.dart';
import '../../../core/design_system/widgets/vitrio_button_variant.dart';
import '../../../core/design_system/widgets/vitrio_chip.dart';
import '../../../core/design_system/widgets/vitrio_empty_state.dart';
import '../../../core/design_system/widgets/vitrio_icon_button.dart';
import '../../../core/design_system/widgets/vitrio_icon_button_variant.dart';
import '../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../core/design_system/widgets/vitrio_notice.dart';
import '../../../core/design_system/widgets/vitrio_search_field.dart';
import '../../../l10n/app_localizations.dart';
import '../../location/presentation/city_picker_sheet.dart';
import '../domain/search_filters.dart';
import '../domain/search_sort.dart';
import 'filters_sheet.dart';
import 'search_filters_controller.dart';
import 'search_labels.dart';
import 'search_queries.dart';
import 'search_results.dart';
import 'search_results_controller.dart';
import 'sort_sheet.dart';
import 'widgets/empty_city_invite.dart';
import 'widgets/listing_skeleton.dart';
import 'widgets/provider_listing_card.dart';

class SearchScreen extends ConsumerStatefulWidget {
  const SearchScreen({super.key});

  @override
  ConsumerState<SearchScreen> createState() => _SearchScreenState();
}

class _SearchScreenState extends ConsumerState<SearchScreen> {
  static const _loadMoreWithin = 600.0;

  late final TextEditingController _query = TextEditingController(
    text: ref.read(searchFiltersControllerProvider).query,
  );
  final ScrollController _scroll = ScrollController();

  @override
  void initState() {
    super.initState();
    _scroll.addListener(() {
      if (_scroll.position.extentAfter < _loadMoreWithin) {
        ref.read(searchResultsControllerProvider.notifier).loadMore();
      }
    });
  }

  @override
  void dispose() {
    _query.dispose();
    _scroll.dispose();
    super.dispose();
  }

  Future<void> _refresh() async {
    ref.invalidate(searchAreaProvider);
    await ref.read(searchResultsControllerProvider.future);
  }

  @override
  Widget build(BuildContext context) {
    ref.listen(searchFiltersControllerProvider, (_, next) {
      if (_query.text != next.query) {
        _query.text = next.query;
      }
    });
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final filters = ref.watch(searchFiltersControllerProvider);
    final notifier = ref.read(searchFiltersControllerProvider.notifier);
    final nearMe = ref.watch(searchAreaProvider).value?.hasPoint ?? false;
    final results = ref.watch(searchResultsControllerProvider);
    final loaded = results.value;

    return Scaffold(
      body: SafeArea(
        child: RefreshIndicator(
          onRefresh: _refresh,
          child: CustomScrollView(
            controller: _scroll,
            physics: const AlwaysScrollableScrollPhysics(),
            slivers: [
              SliverPadding(
                padding: const EdgeInsets.fromLTRB(
                  VitrioSpace.sp20,
                  VitrioSpace.sp16,
                  VitrioSpace.sp20,
                  0,
                ),
                sliver: SliverToBoxAdapter(
                  child: Row(
                    children: [
                      VitrioIconButton(
                        icon: VitrioIconName.chevronLeft,
                        label: MaterialLocalizations.of(context).backButtonTooltip,
                        onPressed: () => const HomeRoute().go(context),
                      ),
                      const SizedBox(width: VitrioSpace.sp10),
                      Expanded(
                        child: VitrioSearchField(
                          label: l10n.searchFieldLabel,
                          submitLabel: l10n.tabSearch,
                          placeholder: l10n.searchActivePlaceholder,
                          controller: _query,
                          clearLabel: l10n.searchClear,
                          onSubmit: notifier.search,
                          onClear: () => notifier.search(''),
                        ),
                      ),
                      const SizedBox(width: VitrioSpace.sp10),
                      VitrioIconButton(
                        icon: VitrioIconName.sliders,
                        label: l10n.searchFilters,
                        variant: VitrioIconButtonVariant.solid,
                        size: 48,
                        onPressed: () => FiltersSheet.show(context, nearMe: nearMe),
                      ),
                    ],
                  ),
                ),
              ),
              SliverToBoxAdapter(
                child: SizedBox(
                  height: VitrioSpace.chip + VitrioSpace.sp14,
                  child: ListView(
                    scrollDirection: Axis.horizontal,
                    padding: const EdgeInsets.fromLTRB(
                      VitrioSpace.sp20,
                      VitrioSpace.sp14,
                      VitrioSpace.sp20,
                      0,
                    ),
                    children: [
                      for (final chip in [
                        if (filters.category case final category?)
                          VitrioChip(
                            label: category.name,
                            selected: true,
                            onSelected: (_) => notifier.removeCategory(),
                          ),
                        if (nearMe)
                          VitrioChip(
                            label: l10n.searchNearMe,
                            selected: filters.sort == SearchSort.distance,
                            onSelected: (_) => notifier.toggleSort(SearchSort.distance),
                          ),
                        VitrioChip(
                          label: l10n.searchTopRated,
                          selected: filters.minRating == SearchFiltersController.topRatedMinimum,
                          onSelected: (_) => notifier.toggleTopRatedOnly(),
                        ),
                        VitrioChip(
                          label: l10n.providerBadgeMei,
                          selected: filters.meiVerified,
                          onSelected: (_) => notifier.toggleMeiVerified(),
                        ),
                        VitrioChip(
                          label: l10n.providerBadgeToday,
                          selected: filters.availableToday,
                          onSelected: (_) => notifier.toggleAvailableToday(),
                        ),
                        VitrioChip(
                          label: l10n.searchLowestPrice,
                          selected: filters.sort == SearchSort.price,
                          onSelected: (_) => notifier.toggleSort(SearchSort.price),
                        ),
                      ]) ...[chip, const SizedBox(width: VitrioSpace.sp8)],
                    ],
                  ),
                ),
              ),
              if (loaded != null && loaded.items.isNotEmpty)
                SliverPadding(
                  padding: const EdgeInsets.fromLTRB(
                    VitrioSpace.sp20,
                    VitrioSpace.sp16,
                    VitrioSpace.sp12,
                    0,
                  ),
                  sliver: SliverToBoxAdapter(
                    child: Row(
                      children: [
                        Expanded(
                          child: Text(
                            resultsHeadline(l10n, loaded),
                            overflow: TextOverflow.ellipsis,
                            style: VitrioText.label.copyWith(color: c.ink),
                          ),
                        ),
                        VitrioButton(
                          label: sortLabel(
                            l10n,
                            filters.sort == SearchSort.distance && !nearMe ? SearchSort.relevance : filters.sort,
                          ),
                          variant: VitrioButtonVariant.ghost,
                          size: VitrioButtonSize.sm,
                          trailingIcon: VitrioIconName.chevronDown,
                          onPressed: () => SortSheet.show(context, nearMe: nearMe),
                        ),
                      ],
                    ),
                  ),
                ),
              SliverPadding(
                padding: const EdgeInsets.fromLTRB(
                  VitrioSpace.sp20,
                  VitrioSpace.sp12,
                  VitrioSpace.sp20,
                  VitrioSpace.sp24,
                ),
                sliver: switch (results) {
                  AsyncData(value: null) => SliverToBoxAdapter(
                      child: Padding(
                        padding: const EdgeInsets.only(top: VitrioSpace.sp32),
                        child: VitrioEmptyState(
                          icon: VitrioIconName.pin,
                          message: l10n.searchChooseCity,
                          actionLabel: l10n.locationChooseCity,
                          onAction: () => CityPickerSheet.show(context),
                        ),
                      ),
                    ),
                  AsyncData(value: final SearchResults found) when found.items.isEmpty => SliverToBoxAdapter(
                      child: _narrowed(filters)
                          ? Padding(
                              padding: const EdgeInsets.only(
                                top: VitrioSpace.sp32,
                              ),
                              child: VitrioEmptyState(
                                icon: VitrioIconName.search,
                                message: l10n.searchNoResults,
                                actionLabel: l10n.searchClearFilters,
                                onAction: () => notifier.apply(SearchFilters.empty),
                              ),
                            )
                          : EmptyCityInvite(city: found.city),
                    ),
                  AsyncData(value: final SearchResults found) => _ResultsList(
                      results: found,
                    ),
                  AsyncError() => SliverToBoxAdapter(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.stretch,
                        children: [
                          VitrioNotice(message: l10n.searchError),
                          const SizedBox(height: VitrioSpace.sp12),
                          VitrioButton(
                            label: l10n.commonRetry,
                            variant: VitrioButtonVariant.secondary,
                            size: VitrioButtonSize.md,
                            onPressed: () => ref.invalidate(searchResultsControllerProvider),
                          ),
                        ],
                      ),
                    ),
                  _ => const SliverToBoxAdapter(child: ListingSkeleton()),
                },
              ),
            ],
          ),
        ),
      ),
    );
  }

  static bool _narrowed(SearchFilters filters) =>
      filters.query.trim().isNotEmpty ||
      filters.category != null ||
      filters.minRating != null ||
      filters.meiVerified ||
      filters.availableToday ||
      filters.maxPriceFrom != null ||
      filters.maxDistanceKm != null;
}

class _ResultsList extends ConsumerWidget {
  const _ResultsList({required this.results});

  final SearchResults results;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    return SliverList.list(
      children: [
        for (final listing in results.items) ...[
          ProviderListingCard(listing: listing),
          const SizedBox(height: VitrioSpace.sp12),
        ],
        if (results.loadingMore)
          const Padding(
            padding: EdgeInsets.all(VitrioSpace.sp12),
            child: Center(
              child: SizedBox.square(
                dimension: VitrioSpace.sp24,
                child: CircularProgressIndicator(strokeWidth: 2),
              ),
            ),
          ),
        if (results.loadMoreFailed)
          VitrioButton(
            label: l10n.commonRetry,
            variant: VitrioButtonVariant.ghost,
            size: VitrioButtonSize.sm,
            onPressed: () => ref.read(searchResultsControllerProvider.notifier).loadMore(),
          ),
      ],
    );
  }
}
