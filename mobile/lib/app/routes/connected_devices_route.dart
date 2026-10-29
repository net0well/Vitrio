part of '../routes.dart';

@TypedGoRoute<ConnectedDevicesRoute>(path: '/connected-devices')
class ConnectedDevicesRoute extends GoRouteData with $ConnectedDevicesRoute {
  const ConnectedDevicesRoute();

  @override
  Widget build(BuildContext context, GoRouterState state) => const ConnectedDevicesScreen();
}
