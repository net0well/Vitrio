import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/routes.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/widgets/vitrio_button.dart';
import '../../../core/design_system/widgets/vitrio_button_size.dart';
import '../../../core/design_system/widgets/vitrio_button_variant.dart';
import '../../../core/design_system/widgets/vitrio_empty_state.dart';
import '../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../core/design_system/widgets/vitrio_notice.dart';
import '../../../core/design_system/widgets/vitrio_top_bar.dart';
import '../../../l10n/app_localizations.dart';
import '../../search/domain/search_filters.dart';
import '../../search/presentation/search_filters_controller.dart';
import '../../search/presentation/widgets/listing_skeleton.dart';
import '../../search/presentation/widgets/provider_listing_card.dart';
import 'favorites_controller.dart';

class FavoritesScreen extends ConsumerWidget {
  const FavoritesScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final favorites = ref.watch(favoritesControllerProvider);
    return Scaffold(
      appBar: VitrioTopBar(title: l10n.favoritesTitle),
      body: RefreshIndicator(
        onRefresh: () => ref.refresh(favoritesControllerProvider.future),
        child: NotificationListener<ScrollEndNotification>(
          onNotification: (notification) {
            if (notification.metrics.extentAfter < 400) {
              ref.read(favoritesControllerProvider.notifier).loadMore();
            }

            return false;
          },
          child: ListView(
            physics: const AlwaysScrollableScrollPhysics(),
            padding: const EdgeInsets.fromLTRB(
              VitrioSpace.sp20,
              VitrioSpace.sp16,
              VitrioSpace.sp20,
              VitrioSpace.sp24,
            ),
            children: switch (favorites) {
              AsyncData(:final value) when value.items.isEmpty => [
                  const SizedBox(height: VitrioSpace.sp32),
                  VitrioEmptyState(
                    icon: VitrioIconName.heart,
                    message: l10n.favoritesEmpty,
                    actionLabel: l10n.favoritesSearch,
                    onAction: () {
                      ref.read(searchFiltersControllerProvider.notifier).apply(SearchFilters.empty);
                      const SearchRoute().go(context);
                    },
                  ),
                ],
              AsyncData(:final value) => [
                  for (final listing in value.items) ...[
                    ProviderListingCard(listing: listing),
                    const SizedBox(height: VitrioSpace.sp12),
                  ],
                ],
              AsyncError() => [
                  VitrioNotice(message: l10n.favoritesError),
                  const SizedBox(height: VitrioSpace.sp12),
                  VitrioButton(
                    label: l10n.commonRetry,
                    variant: VitrioButtonVariant.secondary,
                    size: VitrioButtonSize.md,
                    onPressed: () => ref.invalidate(favoritesControllerProvider),
                  ),
                ],
              _ => const [ListingSkeleton()],
            },
          ),
        ),
      ),
    );
  }
}
