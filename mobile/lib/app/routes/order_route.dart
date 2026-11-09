part of '../routes.dart';

class OrderRoute extends GoRouteData with $OrderRoute {
  const OrderRoute({required this.id});

  final String id;

  @override
  Widget build(BuildContext context, GoRouterState state) => OrderScreen(orderId: id);
}
