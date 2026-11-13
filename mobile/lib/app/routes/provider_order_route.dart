part of '../routes.dart';

class ProviderOrderRoute extends GoRouteData with $ProviderOrderRoute {
  const ProviderOrderRoute({required this.id});

  final String id;

  @override
  Widget build(BuildContext context, GoRouterState state) => ProviderOrderScreen(orderId: id);
}
