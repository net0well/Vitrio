part of '../routes.dart';

@TypedStatefulShellRoute<ProviderShellRoute>(
  branches: [
    TypedStatefulShellBranch<ProviderDashboardBranch>(routes: [TypedGoRoute<ProviderDashboardRoute>(path: '/provider-dashboard')]),
    TypedStatefulShellBranch<ProviderProfileBranch>(routes: [TypedGoRoute<ProviderProfileRoute>(path: '/provider-profile')]),
    TypedStatefulShellBranch<ProviderAccountBranch>(routes: [TypedGoRoute<ProviderAccountRoute>(path: '/provider-account')]),
  ],
)
class ProviderShellRoute extends StatefulShellRouteData {
  const ProviderShellRoute();

  @override
  Widget builder(BuildContext context, GoRouterState state, StatefulNavigationShell navigationShell) =>
      ProviderShell(navigationShell: navigationShell);
}
