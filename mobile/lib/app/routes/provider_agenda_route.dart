part of '../routes.dart';

class ProviderAgendaRoute extends GoRouteData with $ProviderAgendaRoute {
  const ProviderAgendaRoute();

  @override
  Widget build(BuildContext context, GoRouterState state) => const AgendaScreen();
}
