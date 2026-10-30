import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../core/design_system/tokens/vitrio_radius.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/design_system/widgets/vitrio_icon.dart';
import '../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../core/design_system/widgets/vitrio_list_row.dart';
import '../../../core/design_system/widgets/vitrio_notice.dart';
import '../../../core/network/api_result.dart';
import '../../../core/network/problem_details.dart';
import '../../../core/presentation/problem_message.dart';
import '../../../l10n/app_localizations.dart';
import '../data/location_providers.dart';
import '../domain/city_summary.dart';
import 'location_controller.dart';
import 'location_outcome.dart';

class CityPickerSheet extends ConsumerStatefulWidget {
  const CityPickerSheet({super.key});

  static Future<void> show(BuildContext context) => showModalBottomSheet<void>(
    context: context,
    isScrollControlled: true,
    useSafeArea: true,
    useRootNavigator: true,
    showDragHandle: true,
    builder: (_) => const CityPickerSheet(),
  );

  @override
  ConsumerState<CityPickerSheet> createState() => _CityPickerSheetState();
}

class _CityPickerSheetState extends ConsumerState<CityPickerSheet> {
  static const _debounce = Duration(milliseconds: 300);

  Timer? _timer;
  int _searchId = 0;
  bool _searching = false;
  List<CitySummary>? _results;
  ProblemDetails? _failure;
  String? _message;

  @override
  void dispose() {
    _timer?.cancel();
    super.dispose();
  }

  void _onChanged(String text) {
    _timer?.cancel();
    final searchId = ++_searchId;
    final term = text.trim();
    if (term.length < 2) {
      setState(() {
        _searching = false;
        _results = null;
        _failure = null;
      });
      return;
    }

    setState(() => _searching = true);
    _timer = Timer(_debounce, () async {
      final result = await ref.read(citiesRepositoryProvider).search(term);
      if (!mounted || searchId != _searchId) {
        return;
      }

      setState(() {
        _searching = false;
        switch (result) {
          case ApiSuccess(:final data):
            _results = data;
            _failure = null;
          case ApiFailure(:final problem):
            _results = null;
            _failure = problem;
        }
      });
    });
  }

  Future<void> _choose(CitySummary city) async {
    await ref.read(locationControllerProvider.notifier).choose(city);
    if (mounted) {
      Navigator.of(context).pop();
    }
  }

  Future<void> _useNearMe() async {
    final outcome = await ref.read(locationControllerProvider.notifier).useNearMe();
    if (!mounted) {
      return;
    }

    final l10n = AppLocalizations.of(context);
    switch (outcome) {
      case LocationOutcome.located:
        Navigator.of(context).pop();
      case LocationOutcome.permissionDenied:
        setState(() => _message = l10n.locationDenied);
      case LocationOutcome.notFound:
        setState(() => _message = l10n.locationNotFound);
      case LocationOutcome.openedSettings:
        break;
    }
  }

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final recent = ref.read(locationPreferencesProvider).recent;
    final results = _results;
    final keyboard = MediaQuery.viewInsetsOf(context).bottom;
    final message = _failure == null ? _message : problemMessage(l10n, _failure!);
    return Padding(
      padding: EdgeInsets.only(bottom: keyboard),
      child: SizedBox(
        height: (MediaQuery.sizeOf(context).height - keyboard) * 0.85,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp20),
              child: Semantics(
                header: true,
                child: Text(l10n.locationChooseCity, style: VitrioText.section.copyWith(color: c.ink), textAlign: TextAlign.center),
              ),
            ),
            Padding(
              padding: const EdgeInsets.fromLTRB(VitrioSpace.sp20, VitrioSpace.sp16, VitrioSpace.sp20, VitrioSpace.sp12),
              child: TextField(
                autofocus: true,
                onChanged: _onChanged,
                textInputAction: TextInputAction.search,
                style: VitrioText.weighted(VitrioText.bodyStrong, 600).copyWith(color: c.ink),
                decoration: InputDecoration(
                  hintText: l10n.locationSearchHint,
                  prefixIcon: Padding(
                    padding: const EdgeInsetsDirectional.only(start: VitrioSpace.sp16, end: VitrioSpace.sp10),
                    child: VitrioIcon(VitrioIconName.search, color: c.inkMuted),
                  ),
                  prefixIconConstraints: const BoxConstraints(minHeight: VitrioSpace.tap),
                  enabledBorder: OutlineInputBorder(
                    borderRadius: const BorderRadius.all(VitrioRadius.xl),
                    borderSide: BorderSide(color: c.lineStrong),
                  ),
                  focusedBorder: OutlineInputBorder(
                    borderRadius: const BorderRadius.all(VitrioRadius.xl),
                    borderSide: BorderSide(color: c.plum, width: 1.5),
                  ),
                ),
              ),
            ),
            if (_searching) const LinearProgressIndicator(minHeight: 2),
            if (message != null)
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp20),
                child: VitrioNotice(message: message),
              ),
            Expanded(
              child: ListView(
                children: [
                  if (results == null && _failure == null) ...[
                    VitrioListRow(icon: VitrioIconName.pin, title: l10n.locationUseMine, onTap: _useNearMe),
                    if (recent.isNotEmpty)
                      Padding(
                        padding: const EdgeInsets.fromLTRB(VitrioSpace.sp16, VitrioSpace.sp16, VitrioSpace.sp16, VitrioSpace.sp4),
                        child: Text(l10n.locationRecent, style: VitrioText.label.copyWith(color: c.inkMuted)),
                      ),
                    for (final city in recent) _CityTile(city: city, onTap: () => _choose(city)),
                  ] else if (results == null)
                    const SizedBox.shrink()
                  else if (results.isEmpty)
                    Padding(
                      padding: const EdgeInsets.all(VitrioSpace.sp20),
                      child: Text(l10n.locationNoResults, style: VitrioText.body.copyWith(color: c.inkMuted)),
                    )
                  else
                    for (final city in results) _CityTile(city: city, onTap: () => _choose(city)),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _CityTile extends StatelessWidget {
  const _CityTile({required this.city, required this.onTap});

  final CitySummary city;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) => VitrioListRow(title: '${city.name} · ${city.state}', onTap: onTap);
}
