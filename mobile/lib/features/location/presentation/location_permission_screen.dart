import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/routes.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/widgets/vitrio_empty_state.dart';
import '../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../l10n/app_localizations.dart';
import 'city_picker_sheet.dart';
import 'location_controller.dart';
import 'location_outcome.dart';

class LocationPermissionScreen extends ConsumerStatefulWidget {
  const LocationPermissionScreen({super.key});

  @override
  ConsumerState<LocationPermissionScreen> createState() =>
      _LocationPermissionScreenState();
}

class _LocationPermissionScreenState
    extends ConsumerState<LocationPermissionScreen> {
  bool _busy = false;

  Future<void> _useNearMe() async {
    setState(() => _busy = true);
    final outcome = await ref
        .read(locationControllerProvider.notifier)
        .useNearMe();
    if (!mounted) {
      return;
    }

    setState(() => _busy = false);
    if (outcome == LocationOutcome.located) {
      const HomeRoute().go(context);
      return;
    }

    if (outcome != LocationOutcome.openedSettings) {
      final l10n = AppLocalizations.of(context);
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            outcome == LocationOutcome.notFound
                ? l10n.locationNotFound
                : l10n.locationDenied,
          ),
        ),
      );
      await _chooseCity();
    }
  }

  Future<void> _chooseCity() async {
    await CityPickerSheet.show(context);
    if (mounted && ref.read(locationControllerProvider).value?.isSet == true) {
      const HomeRoute().go(context);
    }
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    return AnnotatedRegion<SystemUiOverlayStyle>(
      value: SystemUiOverlayStyle.dark,
      child: Scaffold(
        body: SafeArea(
          child: Padding(
            padding: const EdgeInsets.all(VitrioSpace.sp20),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                const Spacer(),
                VitrioEmptyState(
                  icon: VitrioIconName.pin,
                  title: l10n.locationPermissionTitle,
                  message: l10n.locationPermissionMessage,
                ),
                const Spacer(),
                SizedBox(
                  height: VitrioSpace.cta,
                  child: FilledButton(
                    onPressed: _busy ? null : _useNearMe,
                    child: Text(l10n.locationUseMine),
                  ),
                ),
                const SizedBox(height: VitrioSpace.sp12),
                SizedBox(
                  height: VitrioSpace.cta,
                  child: OutlinedButton(
                    onPressed: _busy ? null : _chooseCity,
                    child: Text(l10n.locationChooseCity),
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
