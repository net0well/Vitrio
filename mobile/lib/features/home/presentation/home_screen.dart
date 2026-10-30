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
import '../../../core/design_system/widgets/vitrio_category_tile.dart';
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
import '../../provider/domain/service_category.dart';
import '../../provider/presentation/provider_queries.dart';

class HomeScreen extends ConsumerWidget {
  const HomeScreen({super.key});

  static const _tiles = 7;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final locating = ref.watch(locationControllerProvider);
    final location = locating.value ?? LocationState.unset;
    final me = ref.watch(sessionControllerProvider).value;
    final categories = ref.watch(serviceCategoriesProvider);
    void openSearch() => const SearchRoute().go(context);

    return AnnotatedRegion<SystemUiOverlayStyle>(
      value: Theme.of(context).brightness == Brightness.dark ? SystemUiOverlayStyle.light : SystemUiOverlayStyle.dark,
      child: Scaffold(
        body: SafeArea(
          child: ListView(
            padding: const EdgeInsets.fromLTRB(VitrioSpace.sp20, VitrioSpace.sp20, VitrioSpace.sp20, VitrioSpace.sp24),
            children: [
              Row(
                children: [
                  Flexible(
                    child: Align(
                      alignment: AlignmentDirectional.centerStart,
                      child: locating.isLoading && !location.isSet
                          ? const SizedBox.square(dimension: VitrioSpace.sp20, child: CircularProgressIndicator(strokeWidth: 2))
                          : LocationPill(location: location, onTap: () => CityPickerSheet.show(context)),
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
                  style: VitrioText.weighted(VitrioText.label, 700).copyWith(color: c.inkMuted),
                ),
                const SizedBox(height: VitrioSpace.sp4),
              ],
              Semantics(
                header: true,
                child: Text(l10n.homeQuestion, style: VitrioText.display.copyWith(color: c.ink)),
              ),
              if (location.mode == LocationMode.chosenCity && location.city != null) ...[
                const SizedBox(height: VitrioSpace.sp6),
                Wrap(
                  crossAxisAlignment: WrapCrossAlignment.center,
                  children: [
                    Text(
                      l10n.locationShowingCity('${location.city!.name}, ${location.city!.state}'),
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
                onSubmit: (_) => openSearch(),
              ),
              const SizedBox(height: VitrioSpace.sp20),
              VitrioHeroCard(
                tag: l10n.homeHeroTag,
                title: l10n.homeHeroTitle,
                text: l10n.homeHeroText,
                actionLabel: l10n.homeHeroAction,
                icon: VitrioIconName.bolt,
                onAction: openSearch,
              ),
              const SizedBox(height: VitrioSpace.sp24),
              VitrioSectionHeader(l10n.homeCategories, actionLabel: l10n.homeSeeAll, onAction: openSearch),
              const SizedBox(height: VitrioSpace.sp14),
              switch (categories) {
                AsyncData(:final value) => _CategoryGrid(categories: value.take(_tiles).toList(), onTap: openSearch),
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
            ],
          ),
        ),
      ),
    );
  }
}

class _CategoryGrid extends StatelessWidget {
  const _CategoryGrid({required this.categories, required this.onTap});

  final List<ServiceCategory> categories;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final tiles = [
      for (final category in categories)
        VitrioCategoryTile(icon: VitrioIconName.fromFileName(category.icon), label: category.name, onTap: onTap),
      VitrioCategoryTile(icon: VitrioIconName.grid, label: l10n.homeMore, more: true, onTap: onTap),
    ];
    return Column(
      children: [
        for (var start = 0; start < tiles.length; start += 4) ...[
          if (start > 0) const SizedBox(height: VitrioSpace.sp12),
          IntrinsicHeight(
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                for (var index = start; index < start + 4; index++) ...[
                  if (index > start) const SizedBox(width: VitrioSpace.sp12),
                  Expanded(child: index < tiles.length ? tiles[index] : const SizedBox.shrink()),
                ],
              ],
            ),
          ),
        ],
      ],
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
        decoration: BoxDecoration(color: c.track, borderRadius: const BorderRadius.all(VitrioRadius.xxl)),
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
