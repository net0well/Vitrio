import 'package:flutter/foundation.dart';
import 'package:go_router/go_router.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../features/auth/presentation/session_controller.dart';
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
  final signInLocation = const SignInRoute().location;
  final protectedLocations = {
    const EditProfileRoute().location,
    const ChangePasswordRoute().location,
    const ConnectedDevicesRoute().location,
  };

  final router = GoRouter(
    initialLocation: const HomeRoute().location,
    routes: $appRoutes,
    refreshListenable: Listenable.merge([welcomeSeen, signedIn]),
    redirect: (context, state) {
      final location = state.matchedLocation;
      if (!welcomeSeen.value && location != welcomeLocation) {
        return welcomeLocation;
      }

      if (!signedIn.value && protectedLocations.contains(location)) {
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
