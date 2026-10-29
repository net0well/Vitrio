part of '../routes.dart';

@TypedGoRoute<ChangePasswordRoute>(path: '/change-password')
class ChangePasswordRoute extends GoRouteData with $ChangePasswordRoute {
  const ChangePasswordRoute();

  @override
  Widget build(BuildContext context, GoRouterState state) => const ChangePasswordScreen();
}
