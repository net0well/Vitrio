part of '../routes.dart';

@TypedStatefulShellRoute<ClientShellRoute>(
  branches: [
    TypedStatefulShellBranch<HomeBranch>(routes: [TypedGoRoute<HomeRoute>(path: '/home')]),
    TypedStatefulShellBranch<SearchBranch>(routes: [TypedGoRoute<SearchRoute>(path: '/search')]),
    TypedStatefulShellBranch<OrdersBranch>(routes: [TypedGoRoute<OrdersRoute>(path: '/orders')]),
    TypedStatefulShellBranch<AccountBranch>(routes: [TypedGoRoute<AccountRoute>(path: '/account')]),
  ],
)
class ClientShellRoute extends StatefulShellRouteData {
  const ClientShellRoute();

  @override
  Widget builder(BuildContext context, GoRouterState state, StatefulNavigationShell navigationShell) =>
      ClientShell(navigationShell: navigationShell);
}
