part of '../routes.dart';

@TypedGoRoute<ProviderAccountRoute>(path: '/provider-account')
class ProviderAccountRoute extends GoRouteData with $ProviderAccountRoute {
  const ProviderAccountRoute();

  @override
  Widget build(BuildContext context, GoRouterState state) => const AccountScreen();
}
