import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/routes.dart';
import '../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../core/design_system/tokens/vitrio_radius.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/design_system/widgets/vitrio_button.dart';
import '../../../core/design_system/widgets/vitrio_button_size.dart';
import '../../../core/design_system/widgets/vitrio_button_variant.dart';
import '../../../core/design_system/widgets/vitrio_hero_card.dart';
import '../../../core/design_system/widgets/vitrio_icon_button.dart';
import '../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../core/design_system/widgets/vitrio_search_field.dart';
import '../../../core/design_system/widgets/vitrio_section_header.dart';
import '../../../l10n/app_localizations.dart';
import '../../auth/presentation/session_controller.dart';
import '../../location/domain/location_mode.dart';
import '../../location/domain/location_state.dart';
import '../../location/presentation/city_picker_sheet.dart';
import '../../location/presentation/location_controller.dart';
import '../../location/presentation/location_pill.dart';
import '../../provider/presentation/provider_queries.dart';
import '../../search/domain/home_feed.dart';
import '../../search/domain/search_filters.dart';
import '../../search/presentation/categories_sheet.dart';
import '../../search/presentation/search_filters_controller.dart';
import '../../search/presentation/search_queries.dart';
import '../../search/presentation/widgets/category_grid.dart';
import '../../search/presentation/widgets/empty_city_invite.dart';
import '../../search/presentation/widgets/provider_listing_card.dart';
import 'widgets/sign_in_invite.dart';

class HomeScreen extends ConsumerWidget {
  const HomeScreen({super.key});

  static const _tiles = 7;
  static const _topRatedWidth = 250.0;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final locating = ref.watch(locationControllerProvider);
    final location = locating.value ?? LocationState.unset;
    final session = ref.watch(sessionControllerProvider);
    final me = session.value;
    final categories = ref.watch(serviceCategoriesProvider);
    final feed = ref.watch(homeFeedProvider).value;
    final filters = ref.read(searchFiltersControllerProvider.notifier);
    void openSearch() => const SearchRoute().go(context);

    return AnnotatedRegion<SystemUiOverlayStyle>(
      value: Theme.of(context).brightness == Brightness.dark ? SystemUiOverlayStyle.light : SystemUiOverlayStyle.dark,
      child: Scaffold(
        body: SafeArea(
          child: RefreshIndicator(
            onRefresh: () async {
              ref.invalidate(searchAreaProvider);
              await ref.read(homeFeedProvider.future);
            },
            child: ListView(
              padding: const EdgeInsets.fromLTRB(
                VitrioSpace.sp20,
                VitrioSpace.sp20,
                VitrioSpace.sp20,
                VitrioSpace.sp24,
              ),
              children: [
                Row(
                  children: [
                    Flexible(
                      child: Align(
                        alignment: AlignmentDirectional.centerStart,
                        child: locating.isLoading && !location.isSet
                            ? const SizedBox.square(
                                dimension: VitrioSpace.sp20,
                                child: CircularProgressIndicator(
                                  strokeWidth: 2,
                                ),
                              )
                            : LocationPill(
                                location: location,
                                onTap: () => CityPickerSheet.show(context),
                              ),
                      ),
                    ),
                    const SizedBox(width: VitrioSpace.sp12),
                    VitrioIconButton(
                      icon: VitrioIconName.bell,
                      label: l10n.notificationsTitle,
                      onPressed: () => const NotificationsRoute().push<void>(context),
                    ),
                  ],
                ),
                const SizedBox(height: VitrioSpace.sp20),
                if (me != null) ...[
                  Text(
                    l10n.homeGreeting(me.displayName.split(' ').first),
                    style: VitrioText.weighted(
                      VitrioText.label,
                      700,
                    ).copyWith(color: c.inkMuted),
                  ),
                  const SizedBox(height: VitrioSpace.sp4),
                ],
                Semantics(
                  header: true,
                  child: Text(
                    l10n.homeQuestion,
                    style: VitrioText.display.copyWith(color: c.ink),
                  ),
                ),
                if (location.mode == LocationMode.chosenCity && location.city != null) ...[
                  const SizedBox(height: VitrioSpace.sp6),
                  Wrap(
                    crossAxisAlignment: WrapCrossAlignment.center,
                    children: [
                      Text(
                        l10n.locationShowingCity(
                          '${location.city!.name}, ${location.city!.state}',
                        ),
                        style: VitrioText.meta.copyWith(color: c.inkMuted),
                      ),
                      VitrioButton(
                        label: l10n.locationPillHint,
                        variant: VitrioButtonVariant.ghost,
                        size: VitrioButtonSize.sm,
                        onPressed: () => CityPickerSheet.show(context),
                      ),
                    ],
                  ),
                ],
                const SizedBox(height: VitrioSpace.sp16),
                VitrioSearchField(
                  label: l10n.searchFieldLabel,
                  submitLabel: l10n.tabSearch,
                  placeholder: l10n.searchPlaceholder,
                  onSubmit: (query) {
                    filters.apply(SearchFilters(query: query));
                    openSearch();
                  },
                ),
                if (session case AsyncData(value: null)) ...[
                  const SizedBox(height: VitrioSpace.sp16),
                  const SignInInvite(),
                ],
                const SizedBox(height: VitrioSpace.sp20),
                VitrioHeroCard(
                  tag: l10n.homeHeroTag,
                  title: l10n.homeHeroTitle,
                  text: l10n.homeHeroText,
                  actionLabel: l10n.homeHeroAction,
                  icon: VitrioIconName.bolt,
                  onAction: () {
                    filters.startWithAvailableToday();
                    openSearch();
                  },
                ),
                const SizedBox(height: VitrioSpace.sp24),
                VitrioSectionHeader(
                  l10n.homeCategories,
                  actionLabel: l10n.homeSeeAll,
                  onAction: () => CategoriesSheet.show(context),
                ),
                const SizedBox(height: VitrioSpace.sp14),
                switch (categories) {
                  AsyncData(:final value) => CategoryGrid(
                      categories: value.take(_tiles).toList(),
                      moreLabel: l10n.homeMore,
                      onMore: () => CategoriesSheet.show(context),
                      onTap: (category) {
                        filters.startWithCategory(category);
                        openSearch();
                      },
                    ),
                  AsyncError() => Align(
                      alignment: AlignmentDirectional.centerStart,
                      child: VitrioButton(
                        label: l10n.commonRetry,
                        variant: VitrioButtonVariant.ghost,
                        size: VitrioButtonSize.sm,
                        onPressed: () => ref.invalidate(serviceCategoriesProvider),
                      ),
                    ),
                  _ => const _CategorySkeleton(),
                },
                if (feed case final HomeFeed loaded when !loaded.hasProviders) EmptyCityInvite(city: loaded.city),
                if (feed case final HomeFeed loaded when loaded.topRated.isNotEmpty) ...[
                  const SizedBox(height: VitrioSpace.sp24),
                  VitrioSectionHeader(
                    l10n.homeTopRated,
                    actionLabel: l10n.homeSeeMore,
                    onAction: () {
                      filters.startWithTopRated();
                      openSearch();
                    },
                  ),
                  const SizedBox(height: VitrioSpace.sp14),
                  SingleChildScrollView(
                    scrollDirection: Axis.horizontal,
                    clipBehavior: Clip.none,
                    child: IntrinsicHeight(
                      child: Row(
                        crossAxisAlignment: CrossAxisAlignment.stretch,
                        children: [
                          for (final (index, listing) in loaded.topRated.indexed) ...[
                            if (index > 0) const SizedBox(width: VitrioSpace.sp12),
                            SizedBox(
                              width: _topRatedWidth,
                              child: ProviderListingCard.compact(
                                listing: listing,
                              ),
                            ),
                          ],
                        ],
                      ),
                    ),
                  ),
                ],
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class _CategorySkeleton extends StatelessWidget {
  const _CategorySkeleton();

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    Widget block() => Expanded(
          child: Container(
            height: 90,
            decoration: BoxDecoration(
              color: c.track,
              borderRadius: const BorderRadius.all(VitrioRadius.xxl),
            ),
          ),
        );
    Widget row() => Row(
          children: [
            block(),
            const SizedBox(width: VitrioSpace.sp12),
            block(),
            const SizedBox(width: VitrioSpace.sp12),
            block(),
            const SizedBox(width: VitrioSpace.sp12),
            block(),
          ],
        );
    return Column(
      children: [
        row(),
        const SizedBox(height: VitrioSpace.sp12),
        row(),
      ],
    );
  }
}
