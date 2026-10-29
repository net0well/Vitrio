import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/design_system/widgets/vitrio_empty_state.dart';
import '../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../l10n/app_localizations.dart';
import '../../location/domain/location_mode.dart';
import '../../location/domain/location_state.dart';
import '../../location/presentation/city_picker_sheet.dart';
import '../../location/presentation/location_controller.dart';
import '../../location/presentation/location_pill.dart';

class HomeScreen extends ConsumerWidget {
  const HomeScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final locating = ref.watch(locationControllerProvider);
    final location = locating.value ?? LocationState.unset;
    return Scaffold(
      appBar: AppBar(title: Text(l10n.homeTitle)),
      body: ListView(
        padding: const EdgeInsets.all(VitrioSpace.sp20),
        children: [
          if (location.isSet) ...[
            Align(
              alignment: AlignmentDirectional.centerStart,
              child: LocationPill(location: location, onTap: () => CityPickerSheet.show(context)),
            ),
            if (location.mode == LocationMode.chosenCity) ...[
              const SizedBox(height: VitrioSpace.sp12),
              Text(l10n.locationShowingCity(location.city!.name), style: VitrioText.meta),
            ],
          ] else if (locating.isLoading)
            const Align(
              alignment: AlignmentDirectional.centerStart,
              child: SizedBox.square(dimension: VitrioSpace.sp20, child: CircularProgressIndicator(strokeWidth: 2)),
            )
          else
            Align(
              alignment: AlignmentDirectional.centerStart,
              child: OutlinedButton(onPressed: () => CityPickerSheet.show(context), child: Text(l10n.locationChooseCity)),
            ),
          const SizedBox(height: VitrioSpace.sp32),
          VitrioEmptyState(icon: VitrioIconName.pin, message: l10n.homeEmpty),
        ],
      ),
    );
  }
}
