import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/design_system/widgets/vitrio_icon.dart';
import '../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../core/network/api_result.dart';
import '../../../core/network/problem_details.dart';
import '../../../l10n/app_localizations.dart';
import '../../auth/presentation/widgets/problem_message.dart';
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
              padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp16),
              child: Text(
                l10n.locationChooseCity,
                style: Theme.of(context).textTheme.titleLarge,
                textAlign: TextAlign.center,
              ),
            ),
            Padding(
              padding: const EdgeInsets.all(VitrioSpace.sp16),
              child: TextField(
                autofocus: true,
                onChanged: _onChanged,
                textInputAction: TextInputAction.search,
                decoration: InputDecoration(
                  hintText: l10n.locationSearchHint,
                  prefixIcon: const Padding(
                    padding: EdgeInsets.all(VitrioSpace.sp12),
                    child: VitrioIcon(VitrioIconName.search),
                  ),
                ),
              ),
            ),
            if (_searching) const LinearProgressIndicator(minHeight: 2),
            if (message != null)
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp16),
                child: Text(message, style: VitrioText.meta.copyWith(color: Theme.of(context).colorScheme.error)),
              ),
            Expanded(
              child: ListView(
                children: [
                  if (results == null && _failure == null) ...[
                    ListTile(
                      leading: const VitrioIcon(VitrioIconName.pin),
                      title: Text(l10n.locationUseMine),
                      onTap: _useNearMe,
                    ),
                    if (recent.isNotEmpty)
                      Padding(
                        padding: const EdgeInsets.fromLTRB(
                          VitrioSpace.sp16,
                          VitrioSpace.sp16,
                          VitrioSpace.sp16,
                          VitrioSpace.sp4,
                        ),
                        child: Text(l10n.locationRecent, style: VitrioText.label),
                      ),
                    for (final city in recent) _CityTile(city: city, onTap: () => _choose(city)),
                  ] else if (results == null)
                    const SizedBox.shrink()
                  else if (results.isEmpty)
                    Padding(padding: const EdgeInsets.all(VitrioSpace.sp16), child: Text(l10n.locationNoResults))
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
  Widget build(BuildContext context) => ListTile(title: Text('${city.name} · ${city.state}'), onTap: onTap);
}
