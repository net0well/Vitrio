part of '../routes.dart';

@TypedGoRoute<PublicProviderRoute>(path: '/providers/:id')
class PublicProviderRoute extends GoRouteData with $PublicProviderRoute {
  const PublicProviderRoute({required this.id, this.preview = false});

  final String id;
  final bool preview;

  @override
  Widget build(BuildContext context, GoRouterState state) => PublicProviderScreen(providerId: id, preview: preview);
}
