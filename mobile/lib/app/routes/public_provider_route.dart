part of '../routes.dart';

@TypedGoRoute<PublicProviderRoute>(path: '/providers/:id')
class PublicProviderRoute extends GoRouteData with $PublicProviderRoute {
  const PublicProviderRoute({required this.id});

  final String id;

  @override
  Widget build(BuildContext context, GoRouterState state) => PublicProviderScreen(providerId: id);
}
