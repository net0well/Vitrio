import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/routes.dart';
import '../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/design_system/widgets/vitrio_button.dart';
import '../../../core/design_system/widgets/vitrio_button_size.dart';
import '../../../core/design_system/widgets/vitrio_button_variant.dart';
import '../../../l10n/app_localizations.dart';
import '../../provider/presentation/provider_queries.dart';
import 'search_filters_controller.dart';
import 'widgets/category_grid.dart';

class CategoriesSheet extends ConsumerWidget {
  const CategoriesSheet({super.key});

  static Future<void> show(BuildContext context) => showModalBottomSheet<void>(
        context: context,
        isScrollControlled: true,
        useSafeArea: true,
        useRootNavigator: true,
        showDragHandle: true,
        builder: (_) => const CategoriesSheet(),
      );

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final categories = ref.watch(serviceCategoriesProvider);
    return SingleChildScrollView(
      padding: const EdgeInsets.fromLTRB(
        VitrioSpace.sp20,
        0,
        VitrioSpace.sp20,
        VitrioSpace.sp24,
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Semantics(
            header: true,
            child: Text(
              l10n.homeCategories,
              style: VitrioText.title.copyWith(color: c.ink),
            ),
          ),
          const SizedBox(height: VitrioSpace.sp16),
          switch (categories) {
            AsyncData(:final value) => CategoryGrid(
                categories: value,
                onTap: (category) {
                  ref.read(searchFiltersControllerProvider.notifier).startWithCategory(category);
                  Navigator.of(context).pop();
                  const SearchRoute().go(context);
                },
              ),
            AsyncError() => VitrioButton(
                label: l10n.commonRetry,
                variant: VitrioButtonVariant.ghost,
                size: VitrioButtonSize.sm,
                onPressed: () => ref.invalidate(serviceCategoriesProvider),
              ),
            _ => const Center(child: CircularProgressIndicator()),
          },
        ],
      ),
    );
  }
}
