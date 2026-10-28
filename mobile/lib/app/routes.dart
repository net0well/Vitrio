import 'package:flutter/widgets.dart';
import 'package:go_router/go_router.dart';

import '../features/account/presentation/account_screen.dart';
import '../features/home/presentation/home_screen.dart';
import '../features/orders/presentation/orders_screen.dart';
import '../features/search/presentation/search_screen.dart';
import '../features/welcome/presentation/welcome_screen.dart';
import 'client_shell.dart';

part 'routes.g.dart';

@TypedGoRoute<WelcomeRoute>(path: '/welcome')
class WelcomeRoute extends GoRouteData with $WelcomeRoute {
  const WelcomeRoute();

  @override
  Widget build(BuildContext context, GoRouterState state) => const WelcomeScreen();
}

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

class HomeBranch extends StatefulShellBranchData {
  const HomeBranch();
}

class SearchBranch extends StatefulShellBranchData {
  const SearchBranch();
}

class OrdersBranch extends StatefulShellBranchData {
  const OrdersBranch();
}

class AccountBranch extends StatefulShellBranchData {
  const AccountBranch();
}

class HomeRoute extends GoRouteData with $HomeRoute {
  const HomeRoute();

  @override
  Widget build(BuildContext context, GoRouterState state) => const HomeScreen();
}

class SearchRoute extends GoRouteData with $SearchRoute {
  const SearchRoute();

  @override
  Widget build(BuildContext context, GoRouterState state) => const SearchScreen();
}

class OrdersRoute extends GoRouteData with $OrdersRoute {
  const OrdersRoute();

  @override
  Widget build(BuildContext context, GoRouterState state) => const OrdersScreen();
}

class AccountRoute extends GoRouteData with $AccountRoute {
  const AccountRoute();

  @override
  Widget build(BuildContext context, GoRouterState state) => const AccountScreen();
}
