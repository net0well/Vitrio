import 'package:flutter/material.dart';

import '../../../../core/design_system/tokens/vitrio_space.dart';
import '../../../../core/design_system/widgets/vitrio_category_tile.dart';
import '../../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../provider/domain/service_category.dart';

class CategoryGrid extends StatelessWidget {
  const CategoryGrid({
    super.key,
    required this.categories,
    required this.onTap,
    this.moreLabel,
    this.onMore,
  });

  static const columns = 4;

  final List<ServiceCategory> categories;
  final ValueChanged<ServiceCategory> onTap;
  final String? moreLabel;
  final VoidCallback? onMore;

  @override
  Widget build(BuildContext context) {
    final tiles = [
      for (final category in categories)
        VitrioCategoryTile(
          icon: VitrioIconName.fromFileName(category.icon),
          label: category.name,
          onTap: () => onTap(category),
        ),
      if (moreLabel != null && onMore != null)
        VitrioCategoryTile(
          icon: VitrioIconName.grid,
          label: moreLabel!,
          more: true,
          onTap: onMore!,
        ),
    ];
    return Column(
      children: [
        for (var start = 0; start < tiles.length; start += columns) ...[
          if (start > 0) const SizedBox(height: VitrioSpace.sp12),
          IntrinsicHeight(
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                for (var index = start; index < start + columns; index++) ...[
                  if (index > start) const SizedBox(width: VitrioSpace.sp12),
                  Expanded(
                    child: index < tiles.length ? tiles[index] : const SizedBox.shrink(),
                  ),
                ],
              ],
            ),
          ),
        ],
      ],
    );
  }
}
