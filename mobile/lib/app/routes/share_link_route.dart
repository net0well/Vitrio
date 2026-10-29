part of '../routes.dart';

@TypedGoRoute<ShareLinkRoute>(path: '/p/:id')
class ShareLinkRoute extends GoRouteData with $ShareLinkRoute {
  const ShareLinkRoute({required this.id});

  final String id;

  @override
  String? redirect(BuildContext context, GoRouterState state) => PublicProviderRoute(id: id).location;
}
