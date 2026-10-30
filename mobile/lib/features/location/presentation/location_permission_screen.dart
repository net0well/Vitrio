import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_svg/flutter_svg.dart';

import '../../../app/routes.dart';
import '../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/design_system/widgets/vitrio_button.dart';
import '../../../core/design_system/widgets/vitrio_button_variant.dart';
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
    final c = context.vitrio;
    return AnnotatedRegion<SystemUiOverlayStyle>(
      value: SystemUiOverlayStyle.dark,
      child: Scaffold(
        body: SafeArea(
          child: Padding(
            padding: const EdgeInsets.fromLTRB(VitrioSpace.sp20, VitrioSpace.sp24, VitrioSpace.sp20, VitrioSpace.sp32),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                const Spacer(),
                Center(
                  child: SvgPicture.asset('assets/brand/vitrio-mark.svg', height: 72, excludeFromSemantics: true),
                ),
                const SizedBox(height: VitrioSpace.sp24),
                Semantics(
                  header: true,
                  child: Text(
                    l10n.locationPermissionTitle,
                    textAlign: TextAlign.center,
                    style: VitrioText.display.copyWith(color: c.ink),
                  ),
                ),
                const SizedBox(height: VitrioSpace.sp12),
                Text(
                  l10n.locationPermissionMessage,
                  textAlign: TextAlign.center,
                  style: VitrioText.body.copyWith(color: c.inkMuted),
                ),
                const Spacer(),
                VitrioButton(label: l10n.locationUseMine, busy: _busy, onPressed: _useNearMe),
                const SizedBox(height: VitrioSpace.sp12),
                VitrioButton(
                  label: l10n.locationChooseCity,
                  variant: VitrioButtonVariant.secondary,
                  onPressed: _busy ? null : _chooseCity,
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
