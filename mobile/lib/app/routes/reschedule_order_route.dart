part of '../routes.dart';

@TypedGoRoute<RescheduleOrderRoute>(path: '/provider-reschedule/:id')
class RescheduleOrderRoute extends GoRouteData with $RescheduleOrderRoute {
  const RescheduleOrderRoute({required this.id});

  final String id;

  @override
  Widget build(BuildContext context, GoRouterState state) => RescheduleOrderScreen(orderId: id);
}
