part of '../routes.dart';

@TypedGoRoute<VerifyEmailRoute>(path: '/verify-email')
class VerifyEmailRoute extends GoRouteData with $VerifyEmailRoute {
  const VerifyEmailRoute();

  @override
  Widget build(BuildContext context, GoRouterState state) => const VerifyEmailScreen();
}
