import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/design_system/widgets/vitrio_chip.dart';
import '../../../l10n/app_localizations.dart';
import '../domain/search_sort.dart';
import 'search_filters_controller.dart';
import 'search_labels.dart';

class SortSheet extends ConsumerWidget {
  const SortSheet({super.key, required this.nearMe});

  final bool nearMe;

  static Future<void> show(BuildContext context, {required bool nearMe}) => showModalBottomSheet<void>(
        context: context,
        useSafeArea: true,
        useRootNavigator: true,
        showDragHandle: true,
        builder: (_) => SortSheet(nearMe: nearMe),
      );

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final current = ref.watch(searchFiltersControllerProvider).sort;
    return Padding(
      padding: const EdgeInsets.fromLTRB(
        VitrioSpace.sp20,
        0,
        VitrioSpace.sp20,
        VitrioSpace.sp24,
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Semantics(
            header: true,
            child: Text(
              l10n.searchSortTitle,
              style: VitrioText.title.copyWith(color: c.ink),
            ),
          ),
          const SizedBox(height: VitrioSpace.sp16),
          Wrap(
            spacing: VitrioSpace.sp8,
            runSpacing: VitrioSpace.sp8,
            children: [
              for (final sort in SearchSort.values)
                if (sort != SearchSort.distance || nearMe)
                  VitrioChip(
                    label: sortLabel(l10n, sort),
                    selected: sort == current,
                    onSelected: (_) {
                      ref.read(searchFiltersControllerProvider.notifier).sortBy(sort);
                      Navigator.of(context).pop();
                    },
                  ),
            ],
          ),
        ],
      ),
    );
  }
}
