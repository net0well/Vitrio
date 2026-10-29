import 'package:flutter/foundation.dart';
import 'package:go_router/go_router.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../features/auth/presentation/session_controller.dart';
import '../features/location/data/location_providers.dart';
import '../features/provider/data/provider_providers.dart';
import '../features/provider/domain/app_mode.dart';
import '../features/welcome/presentation/welcome_controller.dart';
import 'routes.dart';

part 'router.g.dart';

@Riverpod(keepAlive: true)
GoRouter router(Ref ref) {
  final welcomeSeen = ValueNotifier(ref.read(welcomeControllerProvider));
  ref.listen(welcomeControllerProvider, (_, seen) => welcomeSeen.value = seen);

  final signedIn = ValueNotifier(ref.read(sessionControllerProvider).value != null);
  ref.listen(sessionControllerProvider, (_, session) => signedIn.value = session.value != null);

  final welcomeLocation = const WelcomeRoute().location;
  final homeLocation = const HomeRoute().location;
  final locationPermissionLocation = const LocationPermissionRoute().location;
  final signInLocation = const SignInRoute().location;
  final protectedLocations = {
    const EditProfileRoute().location,
    const ChangePasswordRoute().location,
    const ConnectedDevicesRoute().location,
  };

  const providerPrefix = '/provider-';
  final startsAsProvider = ref.read(appModeStoreProvider).mode == AppMode.provider && signedIn.value;

  final router = GoRouter(
    initialLocation: startsAsProvider ? const ProviderDashboardRoute().location : const HomeRoute().location,
    routes: $appRoutes,
    refreshListenable: Listenable.merge([welcomeSeen, signedIn]),
    redirect: (context, state) {
      final location = state.matchedLocation;
      if (!welcomeSeen.value && location != welcomeLocation) {
        return welcomeLocation;
      }

      if (location == homeLocation && ref.read(locationPreferencesProvider).mode == null) {
        return locationPermissionLocation;
      }

      if (!signedIn.value && (protectedLocations.contains(location) || location.startsWith(providerPrefix))) {
        return signInLocation;
      }

      return null;
    },
  );

  ref.onDispose(() {
    router.dispose();
    welcomeSeen.dispose();
    signedIn.dispose();
  });
  return router;
}
