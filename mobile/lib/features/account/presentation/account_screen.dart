import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/routes.dart';
import '../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/design_system/widgets/vitrio_empty_state.dart';
import '../../../core/design_system/widgets/vitrio_icon.dart';
import '../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../l10n/app_localizations.dart';
import '../../auth/domain/me.dart';
import '../../auth/presentation/session_controller.dart';
import '../../provider/domain/app_mode.dart';
import '../../provider/presentation/app_mode_controller.dart';
import '../../provider/presentation/provider_step.dart';
import 'delete_account_dialog.dart';

class AccountScreen extends ConsumerWidget {
  const AccountScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final session = ref.watch(sessionControllerProvider);
    return Scaffold(
      appBar: AppBar(title: Text(l10n.accountTitle)),
      body: switch (session) {
        AsyncData(value: final Me me) => _SignedIn(me: me),
        AsyncData() => const _SignedOut(),
        AsyncError() => Center(
            child: TextButton(onPressed: () => ref.invalidate(sessionControllerProvider), child: Text(l10n.commonRetry)),
          ),
        _ => const Center(child: CircularProgressIndicator()),
      },
    );
  }
}

class _SignedOut extends StatelessWidget {
  const _SignedOut();

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    return ListView(
      padding: const EdgeInsets.all(VitrioSpace.sp20),
      children: [
        VitrioEmptyState(icon: VitrioIconName.user, message: l10n.accountEmpty),
        const SizedBox(height: VitrioSpace.sp24),
        FilledButton(onPressed: () => const SignInRoute().push<void>(context), child: Text(l10n.authSignIn)),
        const SizedBox(height: VitrioSpace.sp12),
        OutlinedButton(onPressed: () => const SignUpRoute().push<void>(context), child: Text(l10n.authCreateAccount)),
      ],
    );
  }
}

class _SignedIn extends ConsumerWidget {
  const _SignedIn({required this.me});

  final Me me;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    return ListView(
      padding: const EdgeInsets.symmetric(vertical: VitrioSpace.sp16),
      children: [
        ListTile(
          leading: CircleAvatar(
            backgroundColor: c.plumSoft,
            child: Text(me.initials, style: VitrioText.label.copyWith(color: c.plum)),
          ),
          title: Text(me.displayName, style: VitrioText.bodyStrong),
          subtitle: Text(me.phoneNumber == null ? me.email : '${me.email}\n${me.phoneNumber}'),
          isThreeLine: me.phoneNumber != null,
        ),
        const Divider(),
        _ModeTile(me: me),
        const Divider(),
        ListTile(
          leading: const VitrioIcon(VitrioIconName.user),
          title: Text(l10n.accountEditProfile),
          onTap: () => const EditProfileRoute().push<void>(context),
        ),
        ListTile(
          leading: const VitrioIcon(VitrioIconName.lock),
          title: Text(l10n.accountChangePassword),
          onTap: () => const ChangePasswordRoute().push<void>(context),
        ),
        ListTile(
          leading: const VitrioIcon(VitrioIconName.shieldCheck),
          title: Text(l10n.accountDevices),
          onTap: () => const ConnectedDevicesRoute().push<void>(context),
        ),
        const Divider(),
        ListTile(
          leading: const VitrioIcon(VitrioIconName.arrowRight),
          title: Text(l10n.accountSignOut),
          onTap: () => ref.read(sessionControllerProvider.notifier).signOut(allDevices: false),
        ),
        ListTile(
          leading: VitrioIcon(VitrioIconName.x, color: c.danger),
          title: Text(l10n.accountDelete, style: TextStyle(color: c.danger)),
          onTap: () => showDialog<void>(context: context, builder: (_) => const DeleteAccountDialog()),
        ),
      ],
    );
  }
}

class _ModeTile extends ConsumerWidget {
  const _ModeTile({required this.me});

  final Me me;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final mode = ref.watch(appModeControllerProvider);
    if (!me.hasProviderProfile) {
      return ListTile(
        leading: const VitrioIcon(VitrioIconName.wrench),
        title: Text(l10n.providerOfferServices),
        onTap: () => const ProviderOnboardingRoute(step: ProviderStep.about).push<void>(context),
      );
    }

    final toProvider = mode == AppMode.client;
    return ListTile(
      leading: VitrioIcon(toProvider ? VitrioIconName.chart : VitrioIconName.home),
      title: Text(toProvider ? l10n.providerEnterMode : l10n.providerLeaveMode),
      onTap: () async {
        await ref.read(appModeControllerProvider.notifier).change(toProvider ? AppMode.provider : AppMode.client);
        if (context.mounted) {
          toProvider ? const ProviderDashboardRoute().go(context) : const HomeRoute().go(context);
        }
      },
    );
  }
}
