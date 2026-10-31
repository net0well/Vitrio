import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/routes.dart';
import '../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../core/design_system/tokens/vitrio_radius.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/design_system/widgets/vitrio_avatar.dart';
import '../../../core/design_system/widgets/vitrio_avatar_kind.dart';
import '../../../core/design_system/widgets/vitrio_button.dart';
import '../../../core/design_system/widgets/vitrio_button_variant.dart';
import '../../../core/design_system/widgets/vitrio_empty_state.dart';
import '../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../core/design_system/widgets/vitrio_list_card.dart';
import '../../../core/design_system/widgets/vitrio_list_row.dart';
import '../../../core/design_system/widgets/vitrio_top_bar.dart';
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
      appBar: VitrioTopBar(title: l10n.accountTitle),
      body: switch (session) {
        AsyncData(value: final Me me) => _SignedIn(me: me),
        AsyncData() => const _SignedOut(),
        AsyncError() => Center(
            child: VitrioButton(
              label: l10n.commonRetry,
              variant: VitrioButtonVariant.ghost,
              onPressed: () => ref.invalidate(sessionControllerProvider),
            ),
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
      padding: const EdgeInsets.fromLTRB(VitrioSpace.sp20, VitrioSpace.sp32, VitrioSpace.sp20, VitrioSpace.sp24),
      children: [
        VitrioEmptyState(icon: VitrioIconName.user, message: l10n.accountEmpty),
        const SizedBox(height: VitrioSpace.sp24),
        VitrioButton(label: l10n.authSignIn, onPressed: () => const SignInRoute().push<void>(context)),
        const SizedBox(height: VitrioSpace.sp12),
        VitrioButton(
          label: l10n.authCreateAccount,
          variant: VitrioButtonVariant.secondary,
          onPressed: () => const SignUpRoute().push<void>(context),
        ),
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
      padding: const EdgeInsets.fromLTRB(VitrioSpace.sp20, VitrioSpace.sp16, VitrioSpace.sp20, VitrioSpace.sp24),
      children: [
        Container(
          padding: const EdgeInsets.all(VitrioSpace.sp16),
          decoration: BoxDecoration(
            color: c.surfaceRaised,
            borderRadius: const BorderRadius.all(VitrioRadius.card),
            border: Border.all(color: c.lineSoft),
          ),
          child: Row(
            children: [
              VitrioAvatar(name: me.displayName, initials: me.initials, seed: me.id, size: 52, kind: VitrioAvatarKind.client),
              const SizedBox(width: VitrioSpace.sp14),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(me.displayName, style: VitrioText.bodyStrong.copyWith(color: c.ink)),
                    const SizedBox(height: 2),
                    Text(me.email, style: VitrioText.meta.copyWith(color: c.inkMuted)),
                    if (me.phoneNumber case final phone?) Text(phone, style: VitrioText.meta.copyWith(color: c.inkMuted)),
                  ],
                ),
              ),
            ],
          ),
        ),
        const SizedBox(height: VitrioSpace.sp12),
        VitrioListCard(children: [_ModeRow(me: me)]),
        const SizedBox(height: VitrioSpace.sp12),
        VitrioListCard(
          children: [
            VitrioListRow(
              icon: VitrioIconName.heart,
              title: l10n.favoritesTitle,
              onTap: () => const FavoritesRoute().push<void>(context),
            ),
            VitrioListRow(
              icon: VitrioIconName.user,
              title: l10n.accountEditProfile,
              onTap: () => const EditProfileRoute().push<void>(context),
            ),
            VitrioListRow(
              icon: VitrioIconName.lock,
              title: l10n.accountChangePassword,
              onTap: () => const ChangePasswordRoute().push<void>(context),
            ),
            VitrioListRow(
              icon: VitrioIconName.shieldCheck,
              title: l10n.accountDevices,
              onTap: () => const ConnectedDevicesRoute().push<void>(context),
            ),
          ],
        ),
        const SizedBox(height: VitrioSpace.sp12),
        VitrioListCard(
          children: [
            VitrioListRow(
              icon: VitrioIconName.arrowRight,
              title: l10n.accountSignOut,
              chevron: false,
              onTap: () => ref.read(sessionControllerProvider.notifier).signOut(allDevices: false),
            ),
            VitrioListRow(
              icon: VitrioIconName.x,
              title: l10n.accountDelete,
              danger: true,
              onTap: () => showDialog<void>(context: context, builder: (_) => const DeleteAccountDialog()),
            ),
          ],
        ),
      ],
    );
  }
}

class _ModeRow extends ConsumerWidget {
  const _ModeRow({required this.me});

  final Me me;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final mode = ref.watch(appModeControllerProvider);
    if (!me.hasProviderProfile) {
      return VitrioListRow(
        icon: VitrioIconName.wrench,
        title: l10n.providerOfferServices,
        subtitle: l10n.providerOfferServicesHint,
        onTap: () => const ProviderOnboardingRoute(step: ProviderStep.about).push<void>(context),
      );
    }

    final toProvider = mode == AppMode.client;
    return VitrioListRow(
      icon: toProvider ? VitrioIconName.chart : VitrioIconName.home,
      title: toProvider ? l10n.providerEnterMode : l10n.providerLeaveMode,
      onTap: () async {
        await ref.read(appModeControllerProvider.notifier).change(toProvider ? AppMode.provider : AppMode.client);
        if (context.mounted) {
          toProvider ? const ProviderDashboardRoute().go(context) : const HomeRoute().go(context);
        }
      },
    );
  }
}
