import 'package:flutter/material.dart';

import '../../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../../core/design_system/tokens/vitrio_radius.dart';
import '../../../../core/design_system/tokens/vitrio_space.dart';
import '../../../../core/design_system/tokens/vitrio_text.dart';
import '../../../../core/design_system/widgets/vitrio_icon.dart';
import '../../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../../l10n/app_localizations.dart';
import '../../../orders/domain/order_photo.dart';
import '../../../orders/domain/order_photo_status.dart';

class OrderPhotoStrip extends StatelessWidget {
  const OrderPhotoStrip({super.key, required this.photos, required this.removed});

  static const size = 72.0;

  final List<OrderPhoto> photos;
  final bool removed;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    if (removed) {
      return Text(l10n.orderPhotosRemoved, style: VitrioText.weighted(VitrioText.caption, 600).copyWith(color: c.inkMuted));
    }

    return Wrap(
      spacing: VitrioSpace.sp8,
      runSpacing: VitrioSpace.sp8,
      children: [
        for (final (index, photo) in photos.indexed)
          Semantics(
            image: true,
            label: l10n.orderPhotoLabel(index + 1),
            child: ClipRRect(
              borderRadius: const BorderRadius.all(VitrioRadius.md),
              child: SizedBox.square(
                dimension: size,
                child: switch ((photo.status, photo.url)) {
                  (OrderPhotoStatus.ready, final String url) => Image.network(
                    url,
                    fit: BoxFit.cover,
                    errorBuilder: (_, _, _) => const _Placeholder(),
                  ),
                  _ => const _Placeholder(),
                },
              ),
            ),
          ),
      ],
    );
  }
}

class _Placeholder extends StatelessWidget {
  const _Placeholder();

  @override
  Widget build(BuildContext context) => ColoredBox(
    color: context.vitrio.plumSoft,
    child: Center(child: VitrioIcon(VitrioIconName.image, size: 22, color: context.vitrio.plum)),
  );
}
