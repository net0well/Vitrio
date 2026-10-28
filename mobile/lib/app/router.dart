import 'package:flutter/foundation.dart';
import 'package:go_router/go_router.dart';
import 'package:riverpod_annotation/riverpod_annotation.dart';

import '../features/welcome/presentation/welcome_controller.dart';
import 'routes.dart';

part 'router.g.dart';

@Riverpod(keepAlive: true)
GoRouter router(Ref ref) {
  final welcomeSeen = ValueNotifier(ref.read(welcomeControllerProvider));
  ref.listen(welcomeControllerProvider, (_, seen) => welcomeSeen.value = seen);

  final welcomeLocation = const WelcomeRoute().location;
  final router = GoRouter(
    initialLocation: const HomeRoute().location,
    routes: $appRoutes,
    refreshListenable: welcomeSeen,
    redirect: (context, state) =>
        !welcomeSeen.value && state.matchedLocation != welcomeLocation ? welcomeLocation : null,
  );

  ref.onDispose(() {
    router.dispose();
    welcomeSeen.dispose();
  });
  return router;
}
