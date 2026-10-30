import 'package:flutter/material.dart';

import '../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../core/design_system/tokens/vitrio_radius.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/design_system/widgets/vitrio_icon.dart';
import '../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../l10n/app_localizations.dart';
import '../domain/location_state.dart';

class LocationPill extends StatelessWidget {
  const LocationPill({super.key, required this.location, required this.onTap});

  final LocationState location;
  final VoidCallback onTap;

  static String labelOf(LocationState location) {
    final city = location.city?.name ?? '';
    return location.neighborhood == null ? city : '${location.neighborhood}, $city';
  }

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    final l10n = AppLocalizations.of(context);
    const shape = BorderRadius.all(VitrioRadius.full);
    return Semantics(
      button: true,
      hint: l10n.locationPillHint,
      child: Material(
        color: c.surfaceRaised,
        shape: RoundedRectangleBorder(borderRadius: shape, side: BorderSide(color: c.line)),
        child: InkWell(
          borderRadius: shape,
          onTap: onTap,
          child: SizedBox(
            height: VitrioSpace.tap,
            child: Padding(
              padding: const EdgeInsetsDirectional.only(start: VitrioSpace.sp12, end: VitrioSpace.sp14),
              child: Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  VitrioIcon(VitrioIconName.pin, size: 18, color: c.plum),
                  const SizedBox(width: VitrioSpace.sp8),
                  Flexible(
                    child: Text(
                      location.isSet ? labelOf(location) : l10n.locationChooseCity,
                      style: VitrioText.label.copyWith(color: c.ink),
                      overflow: TextOverflow.ellipsis,
                    ),
                  ),
                  const SizedBox(width: VitrioSpace.sp8),
                  VitrioIcon(VitrioIconName.chevronDown, size: 16, color: c.inkMuted),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
