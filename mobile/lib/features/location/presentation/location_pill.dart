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
    return Semantics(
      button: true,
      hint: AppLocalizations.of(context).locationPillHint,
      child: Material(
        color: c.plumSoft,
        borderRadius: const BorderRadius.all(VitrioRadius.full),
        child: InkWell(
          borderRadius: const BorderRadius.all(VitrioRadius.full),
          onTap: onTap,
          child: Padding(
            padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp14, vertical: VitrioSpace.sp8),
            child: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                VitrioIcon(VitrioIconName.pin, size: 18, color: c.plum),
                const SizedBox(width: VitrioSpace.sp6),
                Flexible(
                  child: Text(labelOf(location), style: VitrioText.label.copyWith(color: c.plum), overflow: TextOverflow.ellipsis),
                ),
                const SizedBox(width: VitrioSpace.sp4),
                VitrioIcon(VitrioIconName.chevronDown, size: 18, color: c.plum),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
