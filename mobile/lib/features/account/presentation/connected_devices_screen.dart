import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';

import '../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/design_system/widgets/vitrio_action_bar.dart';
import '../../../core/design_system/widgets/vitrio_button.dart';
import '../../../core/design_system/widgets/vitrio_button_size.dart';
import '../../../core/design_system/widgets/vitrio_button_variant.dart';
import '../../../core/design_system/widgets/vitrio_list_card.dart';
import '../../../core/design_system/widgets/vitrio_list_row.dart';
import '../../../core/design_system/widgets/vitrio_top_bar.dart';
import '../../../l10n/app_localizations.dart';
import '../../auth/presentation/session_controller.dart';
import 'connected_devices_controller.dart';

class ConnectedDevicesScreen extends ConsumerWidget {
  const ConnectedDevicesScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final sessions = ref.watch(connectedDevicesControllerProvider);
    final format = DateFormat("dd/MM 'às' HH:mm", 'pt_BR');
    return Scaffold(
      appBar: VitrioTopBar(title: l10n.accountDevices),
      body: switch (sessions) {
        AsyncData(:final value) => ListView(
            padding: const EdgeInsets.fromLTRB(VitrioSpace.sp20, VitrioSpace.sp16, VitrioSpace.sp20, VitrioSpace.sp24),
            children: [
              VitrioListCard(
                children: [
                  for (final session in value)
                    VitrioListRow(
                      title: session.current ? '${session.deviceName} · ${l10n.devicesThisDevice}' : session.deviceName,
                      subtitle: l10n.devicesLastSeen(format.format(session.lastSeenAt.toLocal())),
                      trailing: session.current
                          ? null
                          : VitrioButton(
                              label: l10n.devicesDisconnect,
                              variant: VitrioButtonVariant.secondary,
                              size: VitrioButtonSize.sm,
                              onPressed: () => ref.read(connectedDevicesControllerProvider.notifier).disconnect(session.id),
                            ),
                    ),
                ],
              ),
              if (value.length <= 1) ...[
                const SizedBox(height: VitrioSpace.sp12),
                Text(l10n.devicesOnlyThis, style: VitrioText.meta.copyWith(color: c.inkMuted)),
              ],
            ],
          ),
        AsyncError() => Center(
            child: VitrioButton(
              label: l10n.commonRetry,
              variant: VitrioButtonVariant.ghost,
              onPressed: () => ref.invalidate(connectedDevicesControllerProvider),
            ),
          ),
        _ => const Center(child: CircularProgressIndicator()),
      },
      bottomNavigationBar: VitrioActionBar(
        children: [
          VitrioButton(
            label: l10n.devicesSignOutAll,
            variant: VitrioButtonVariant.secondary,
            onPressed: () async {
              await ref.read(sessionControllerProvider.notifier).signOut(allDevices: true);
              if (context.mounted) {
                Navigator.of(context).pop();
              }
            },
          ),
        ],
      ),
    );
  }
}
