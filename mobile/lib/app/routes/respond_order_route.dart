part of '../routes.dart';

@TypedGoRoute<RespondOrderRoute>(path: '/provider-respond/:id')
class RespondOrderRoute extends GoRouteData with $RespondOrderRoute {
  const RespondOrderRoute({required this.id});

  final String id;

  @override
  Widget build(BuildContext context, GoRouterState state) => RespondOrderScreen(orderId: id);
}
