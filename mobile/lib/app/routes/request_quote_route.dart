part of '../routes.dart';

@TypedGoRoute<RequestQuoteRoute>(path: '/request-quote/:providerId')
class RequestQuoteRoute extends GoRouteData with $RequestQuoteRoute {
  const RequestQuoteRoute({required this.providerId, this.from});

  final String providerId;
  final String? from;

  @override
  Widget build(BuildContext context, GoRouterState state) => RequestQuoteScreen(providerId: providerId, fromOrderId: from);
}
