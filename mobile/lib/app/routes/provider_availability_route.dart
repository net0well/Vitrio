part of '../routes.dart';

@TypedGoRoute<ProviderAvailabilityRoute>(path: '/provider-availability')
class ProviderAvailabilityRoute extends GoRouteData with $ProviderAvailabilityRoute {
  const ProviderAvailabilityRoute();

  @override
  Widget build(BuildContext context, GoRouterState state) => const ProviderProfileReady(child: AvailabilityScreen());
}
