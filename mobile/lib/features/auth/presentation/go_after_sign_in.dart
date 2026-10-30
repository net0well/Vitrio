import 'package:flutter/widgets.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/routes.dart';
import '../../provider/presentation/provider_step.dart';
import '../domain/auth_intent.dart';
import 'pending_auth_intent.dart';

void goAfterSignIn(BuildContext context, WidgetRef ref) => switch (ref.read(pendingAuthIntentProvider.notifier).take()) {
      AuthIntent.offerServices => const ProviderOnboardingRoute(step: ProviderStep.about).go(context),
      AuthIntent.browse => const HomeRoute().go(context),
      null => const AccountRoute().go(context),
    };
