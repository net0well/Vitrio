part of '../routes.dart';

@TypedStatefulShellRoute<ProviderShellRoute>(
  branches: [
    TypedStatefulShellBranch<ProviderDashboardBranch>(routes: [TypedGoRoute<ProviderDashboardRoute>(path: '/provider-dashboard')]),
    TypedStatefulShellBranch<ProviderOrdersBranch>(routes: [TypedGoRoute<ProviderOrdersRoute>(path: '/provider-orders')]),
    TypedStatefulShellBranch<ProviderAgendaBranch>(routes: [TypedGoRoute<ProviderAgendaRoute>(path: '/provider-agenda')]),
    TypedStatefulShellBranch<ProviderProfileBranch>(routes: [TypedGoRoute<ProviderProfileRoute>(path: '/provider-profile')]),
  ],
)
class ProviderShellRoute extends StatefulShellRouteData {
  const ProviderShellRoute();

  @override
  Widget builder(BuildContext context, GoRouterState state, StatefulNavigationShell navigationShell) =>
      ProviderShell(navigationShell: navigationShell);
}
