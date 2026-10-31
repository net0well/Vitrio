import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/routes.dart';
import '../../../core/design_system/widgets/vitrio_icon_button.dart';
import '../../../core/design_system/widgets/vitrio_icon_button_variant.dart';
import '../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../l10n/app_localizations.dart';
import '../../auth/domain/auth_intent.dart';
import '../../auth/presentation/pending_auth_intent.dart';
import '../../auth/presentation/pending_favorite.dart';
import '../../auth/presentation/session_controller.dart';
import 'favorite_toggle.dart';

class FavoriteButton extends ConsumerWidget {
  const FavoriteButton({
    super.key,
    required this.providerId,
    required this.saved,
  });

  final String providerId;
  final bool saved;

  Future<void> _toggle(BuildContext context, WidgetRef ref) async {
    if (ref.read(sessionControllerProvider).value == null) {
      ref.read(pendingAuthIntentProvider.notifier).remember(AuthIntent.favorite);
      ref.read(pendingFavoriteProvider.notifier).remember(providerId);
      await const SignInRoute().push<void>(context);
      return;
    }

    final done = await ref.read(favoriteToggleProvider(providerId).notifier).toggle(saved: saved);
    if (!done && context.mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(AppLocalizations.of(context).favoriteFailed)),
      );
    }
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final busy = ref.watch(favoriteToggleProvider(providerId));
    return VitrioIconButton(
      icon: VitrioIconName.heart,
      label: saved ? l10n.favoriteRemove : l10n.favoriteSave,
      variant: saved ? VitrioIconButtonVariant.solid : VitrioIconButtonVariant.surface,
      onPressed: busy ? null : () => _toggle(context, ref),
    );
  }
}
