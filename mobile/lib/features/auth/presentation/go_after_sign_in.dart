import 'dart:async';

import 'package:flutter/widgets.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/routes.dart';
import '../../favorites/data/favorites_providers.dart';
import '../../favorites/presentation/favorites_controller.dart';
import '../../provider/presentation/provider_queries.dart';
import '../../provider/presentation/provider_step.dart';
import '../domain/auth_intent.dart';
import 'pending_auth_intent.dart';
import 'pending_favorite.dart';

void goAfterSignIn(BuildContext context, WidgetRef ref) => switch (ref.read(pendingAuthIntentProvider.notifier).take()) {
      AuthIntent.offerServices => const ProviderOnboardingRoute(
          step: ProviderStep.about,
        ).go(context),
      AuthIntent.browse => const HomeRoute().go(context),
      AuthIntent.favorite => _saveFavoriteAndReturn(context, ref),
      null => const AccountRoute().go(context),
    };

Future<void> _saveFavoriteAndReturn(BuildContext context, WidgetRef ref) async {
  final providerId = ref.read(pendingFavoriteProvider.notifier).take();
  if (providerId == null) {
    const HomeRoute().go(context);
    return;
  }

  await ref.read(favoritesRepositoryProvider).save(providerId);
  ref
    ..invalidate(publicProviderProvider(providerId))
    ..invalidate(favoritesControllerProvider);
  if (context.mounted) {
    const HomeRoute().go(context);
    unawaited(PublicProviderRoute(id: providerId).push<void>(context));
  }
}
