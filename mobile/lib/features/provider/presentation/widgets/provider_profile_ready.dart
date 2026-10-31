import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../../core/design_system/widgets/vitrio_button.dart';
import '../../../../core/design_system/widgets/vitrio_button_variant.dart';
import '../../../../core/design_system/widgets/vitrio_top_bar.dart';
import '../../../../l10n/app_localizations.dart';
import '../../domain/provider_profile.dart';
import '../provider_controller.dart';

class ProviderProfileReady extends ConsumerStatefulWidget {
  const ProviderProfileReady({super.key, required this.child});

  final Widget child;

  @override
  ConsumerState<ProviderProfileReady> createState() => _ProviderProfileReadyState();
}

class _ProviderProfileReadyState extends ConsumerState<ProviderProfileReady> {
  late Future<ProviderProfile?> _profile = ref.read(providerControllerProvider.future);

  void _retry() {
    ref.invalidate(providerControllerProvider);
    setState(() => _profile = ref.read(providerControllerProvider.future));
  }

  @override
  Widget build(BuildContext context) => FutureBuilder<ProviderProfile?>(
        future: _profile,
        builder: (context, snapshot) => switch (snapshot) {
          AsyncSnapshot(connectionState: ConnectionState.done, hasError: false) => widget.child,
          AsyncSnapshot(connectionState: ConnectionState.done) => Scaffold(
              appBar: const VitrioTopBar(),
              body: Center(
                child: VitrioButton(
                  label: AppLocalizations.of(context).commonRetry,
                  variant: VitrioButtonVariant.ghost,
                  onPressed: _retry,
                ),
              ),
            ),
          _ => const Scaffold(body: Center(child: CircularProgressIndicator())),
        },
      );
}
