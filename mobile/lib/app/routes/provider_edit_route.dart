part of '../routes.dart';

@TypedGoRoute<ProviderEditRoute>(path: '/provider-edit/:step')
class ProviderEditRoute extends GoRouteData with $ProviderEditRoute {
  const ProviderEditRoute({required this.step});

  final ProviderStep step;

  @override
  Widget build(BuildContext context, GoRouterState state) => ProviderStepScreen(step: step, editing: true);
}
