part of '../routes.dart';

@TypedGoRoute<LocationPermissionRoute>(path: '/location')
class LocationPermissionRoute extends GoRouteData with $LocationPermissionRoute {
  const LocationPermissionRoute();

  @override
  Widget build(BuildContext context, GoRouterState state) => const LocationPermissionScreen();
}
