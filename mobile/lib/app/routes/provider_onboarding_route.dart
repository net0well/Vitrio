part of '../routes.dart';

@TypedGoRoute<ProviderOnboardingRoute>(path: '/provider-onboarding/:step')
class ProviderOnboardingRoute extends GoRouteData with $ProviderOnboardingRoute {
  const ProviderOnboardingRoute({required this.step});

  final ProviderStep step;

  @override
  Widget build(BuildContext context, GoRouterState state) => ProviderStepScreen(step: step, editing: false);
}
