import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';

import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../l10n/app_localizations.dart';
import '../../auth/presentation/session_controller.dart';
import 'connected_devices_controller.dart';

class ConnectedDevicesScreen extends ConsumerWidget {
  const ConnectedDevicesScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final sessions = ref.watch(connectedDevicesControllerProvider);
    final format = DateFormat("dd/MM 'às' HH:mm", 'pt_BR');
    return Scaffold(
      appBar: AppBar(title: Text(l10n.accountDevices)),
      body: switch (sessions) {
        AsyncData(:final value) => ListView(
            padding: const EdgeInsets.symmetric(vertical: VitrioSpace.sp8),
            children: [
              for (final session in value)
                ListTile(
                  title: Text(session.current ? '${session.deviceName} · ${l10n.devicesThisDevice}' : session.deviceName),
                  subtitle: Text(l10n.devicesLastSeen(format.format(session.lastSeenAt.toLocal()))),
                  trailing: session.current
                      ? null
                      : TextButton(
                          onPressed: () => ref.read(connectedDevicesControllerProvider.notifier).disconnect(session.id),
                          child: Text(l10n.devicesDisconnect),
                        ),
                ),
              if (value.length <= 1)
                Padding(
                  padding: const EdgeInsets.all(VitrioSpace.sp20),
                  child: Text(l10n.devicesOnlyThis, style: VitrioText.meta),
                ),
            ],
          ),
        AsyncError() => Center(
            child: TextButton(onPressed: () => ref.invalidate(connectedDevicesControllerProvider), child: Text(l10n.commonRetry)),
          ),
        _ => const Center(child: CircularProgressIndicator()),
      },
      bottomNavigationBar: SafeArea(
        child: Padding(
          padding: const EdgeInsets.fromLTRB(VitrioSpace.sp20, VitrioSpace.sp8, VitrioSpace.sp20, VitrioSpace.sp16),
          child: SizedBox(
            height: VitrioSpace.cta,
            child: OutlinedButton(
              onPressed: () async {
                await ref.read(sessionControllerProvider.notifier).signOut(allDevices: true);
                if (context.mounted) {
                  Navigator.of(context).pop();
                }
              },
              child: Text(l10n.devicesSignOutAll),
            ),
          ),
        ),
      ),
    );
  }
}
