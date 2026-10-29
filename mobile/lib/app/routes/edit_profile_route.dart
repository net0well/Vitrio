part of '../routes.dart';

@TypedGoRoute<EditProfileRoute>(path: '/account-details')
class EditProfileRoute extends GoRouteData with $EditProfileRoute {
  const EditProfileRoute();

  @override
  Widget build(BuildContext context, GoRouterState state) => const EditProfileScreen();
}
