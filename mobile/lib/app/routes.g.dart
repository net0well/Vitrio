// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'routes.dart';

// **************************************************************************
// GoRouterGenerator
// **************************************************************************

List<RouteBase> get $appRoutes => [
  $changePasswordRoute,
  $connectedDevicesRoute,
  $editProfileRoute,
  $forgotPasswordRoute,
  $clientShellRoute,
  $locationPermissionRoute,
  $providerAvailabilityRoute,
  $providerEditRoute,
  $providerOnboardingRoute,
  $providerShellRoute,
  $publicProviderRoute,
  $shareLinkRoute,
  $signInRoute,
  $signUpRoute,
  $verifyEmailRoute,
  $welcomeRoute,
];

RouteBase get $changePasswordRoute => GoRouteData.$route(
  path: '/change-password',
  hasOverriddenOnExit: false,
  factory: $ChangePasswordRoute._fromState,
);

mixin $ChangePasswordRoute on GoRouteData {
  static ChangePasswordRoute _fromState(GoRouterState state) =>
      const ChangePasswordRoute();

  @override
  String get location => GoRouteData.$location('/change-password');

  @override
  void go(BuildContext context) => context.go(location);

  @override
  Future<T?> push<T>(BuildContext context) => context.push<T>(location);

  @override
  void pushReplacement(BuildContext context) =>
      context.pushReplacement(location);

  @override
  void replace(BuildContext context) => context.replace(location);
}

RouteBase get $connectedDevicesRoute => GoRouteData.$route(
  path: '/connected-devices',
  hasOverriddenOnExit: false,
  factory: $ConnectedDevicesRoute._fromState,
);

mixin $ConnectedDevicesRoute on GoRouteData {
  static ConnectedDevicesRoute _fromState(GoRouterState state) =>
      const ConnectedDevicesRoute();

  @override
  String get location => GoRouteData.$location('/connected-devices');

  @override
  void go(BuildContext context) => context.go(location);

  @override
  Future<T?> push<T>(BuildContext context) => context.push<T>(location);

  @override
  void pushReplacement(BuildContext context) =>
      context.pushReplacement(location);

  @override
  void replace(BuildContext context) => context.replace(location);
}

RouteBase get $editProfileRoute => GoRouteData.$route(
  path: '/account-details',
  hasOverriddenOnExit: false,
  factory: $EditProfileRoute._fromState,
);

mixin $EditProfileRoute on GoRouteData {
  static EditProfileRoute _fromState(GoRouterState state) =>
      const EditProfileRoute();

  @override
  String get location => GoRouteData.$location('/account-details');

  @override
  void go(BuildContext context) => context.go(location);

  @override
  Future<T?> push<T>(BuildContext context) => context.push<T>(location);

  @override
  void pushReplacement(BuildContext context) =>
      context.pushReplacement(location);

  @override
  void replace(BuildContext context) => context.replace(location);
}

RouteBase get $forgotPasswordRoute => GoRouteData.$route(
  path: '/forgot-password',
  hasOverriddenOnExit: false,
  factory: $ForgotPasswordRoute._fromState,
);

mixin $ForgotPasswordRoute on GoRouteData {
  static ForgotPasswordRoute _fromState(GoRouterState state) =>
      const ForgotPasswordRoute();

  @override
  String get location => GoRouteData.$location('/forgot-password');

  @override
  void go(BuildContext context) => context.go(location);

  @override
  Future<T?> push<T>(BuildContext context) => context.push<T>(location);

  @override
  void pushReplacement(BuildContext context) =>
      context.pushReplacement(location);

  @override
  void replace(BuildContext context) => context.replace(location);
}

RouteBase get $clientShellRoute => StatefulShellRouteData.$route(
  factory: $ClientShellRouteExtension._fromState,
  branches: [
    StatefulShellBranchData.$branch(
      routes: [
        GoRouteData.$route(
          path: '/home',
          hasOverriddenOnExit: false,
          factory: $HomeRoute._fromState,
        ),
      ],
    ),
    StatefulShellBranchData.$branch(
      routes: [
        GoRouteData.$route(
          path: '/search',
          hasOverriddenOnExit: false,
          factory: $SearchRoute._fromState,
        ),
      ],
    ),
    StatefulShellBranchData.$branch(
      routes: [
        GoRouteData.$route(
          path: '/orders',
          hasOverriddenOnExit: false,
          factory: $OrdersRoute._fromState,
        ),
      ],
    ),
    StatefulShellBranchData.$branch(
      routes: [
        GoRouteData.$route(
          path: '/account',
          hasOverriddenOnExit: false,
          factory: $AccountRoute._fromState,
        ),
      ],
    ),
  ],
);

extension $ClientShellRouteExtension on ClientShellRoute {
  static ClientShellRoute _fromState(GoRouterState state) =>
      const ClientShellRoute();
}

mixin $HomeRoute on GoRouteData {
  static HomeRoute _fromState(GoRouterState state) => const HomeRoute();

  @override
  String get location => GoRouteData.$location('/home');

  @override
  void go(BuildContext context) => context.go(location);

  @override
  Future<T?> push<T>(BuildContext context) => context.push<T>(location);

  @override
  void pushReplacement(BuildContext context) =>
      context.pushReplacement(location);

  @override
  void replace(BuildContext context) => context.replace(location);
}

mixin $SearchRoute on GoRouteData {
  static SearchRoute _fromState(GoRouterState state) => const SearchRoute();

  @override
  String get location => GoRouteData.$location('/search');

  @override
  void go(BuildContext context) => context.go(location);

  @override
  Future<T?> push<T>(BuildContext context) => context.push<T>(location);

  @override
  void pushReplacement(BuildContext context) =>
      context.pushReplacement(location);

  @override
  void replace(BuildContext context) => context.replace(location);
}

mixin $OrdersRoute on GoRouteData {
  static OrdersRoute _fromState(GoRouterState state) => const OrdersRoute();

  @override
  String get location => GoRouteData.$location('/orders');

  @override
  void go(BuildContext context) => context.go(location);

  @override
  Future<T?> push<T>(BuildContext context) => context.push<T>(location);

  @override
  void pushReplacement(BuildContext context) =>
      context.pushReplacement(location);

  @override
  void replace(BuildContext context) => context.replace(location);
}

mixin $AccountRoute on GoRouteData {
  static AccountRoute _fromState(GoRouterState state) => const AccountRoute();

  @override
  String get location => GoRouteData.$location('/account');

  @override
  void go(BuildContext context) => context.go(location);

  @override
  Future<T?> push<T>(BuildContext context) => context.push<T>(location);

  @override
  void pushReplacement(BuildContext context) =>
      context.pushReplacement(location);

  @override
  void replace(BuildContext context) => context.replace(location);
}

RouteBase get $locationPermissionRoute => GoRouteData.$route(
  path: '/location',
  hasOverriddenOnExit: false,
  factory: $LocationPermissionRoute._fromState,
);

mixin $LocationPermissionRoute on GoRouteData {
  static LocationPermissionRoute _fromState(GoRouterState state) =>
      const LocationPermissionRoute();

  @override
  String get location => GoRouteData.$location('/location');

  @override
  void go(BuildContext context) => context.go(location);

  @override
  Future<T?> push<T>(BuildContext context) => context.push<T>(location);

  @override
  void pushReplacement(BuildContext context) =>
      context.pushReplacement(location);

  @override
  void replace(BuildContext context) => context.replace(location);
}

RouteBase get $providerAvailabilityRoute => GoRouteData.$route(
  path: '/provider-availability',
  hasOverriddenOnExit: false,
  factory: $ProviderAvailabilityRoute._fromState,
);

mixin $ProviderAvailabilityRoute on GoRouteData {
  static ProviderAvailabilityRoute _fromState(GoRouterState state) =>
      const ProviderAvailabilityRoute();

  @override
  String get location => GoRouteData.$location('/provider-availability');

  @override
  void go(BuildContext context) => context.go(location);

  @override
  Future<T?> push<T>(BuildContext context) => context.push<T>(location);

  @override
  void pushReplacement(BuildContext context) =>
      context.pushReplacement(location);

  @override
  void replace(BuildContext context) => context.replace(location);
}

RouteBase get $providerEditRoute => GoRouteData.$route(
  path: '/provider-edit/:step',
  hasOverriddenOnExit: false,
  factory: $ProviderEditRoute._fromState,
);

mixin $ProviderEditRoute on GoRouteData {
  static ProviderEditRoute _fromState(GoRouterState state) => ProviderEditRoute(
    step: _$ProviderStepEnumMap._$fromName(state.pathParameters['step']!)!,
  );

  ProviderEditRoute get _self => this as ProviderEditRoute;

  @override
  String get location => GoRouteData.$location(
    '/provider-edit/${Uri.encodeComponent(_$ProviderStepEnumMap[_self.step]!)}',
  );

  @override
  void go(BuildContext context) => context.go(location);

  @override
  Future<T?> push<T>(BuildContext context) => context.push<T>(location);

  @override
  void pushReplacement(BuildContext context) =>
      context.pushReplacement(location);

  @override
  void replace(BuildContext context) => context.replace(location);
}

const _$ProviderStepEnumMap = {
  ProviderStep.about: 'about',
  ProviderStep.showcase: 'showcase',
  ProviderStep.photos: 'photos',
  ProviderStep.review: 'review',
};

extension<T extends Enum> on Map<T, String> {
  T? _$fromName(String? value) =>
      entries.where((element) => element.value == value).firstOrNull?.key;
}

RouteBase get $providerOnboardingRoute => GoRouteData.$route(
  path: '/provider-onboarding/:step',
  hasOverriddenOnExit: false,
  factory: $ProviderOnboardingRoute._fromState,
);

mixin $ProviderOnboardingRoute on GoRouteData {
  static ProviderOnboardingRoute _fromState(GoRouterState state) =>
      ProviderOnboardingRoute(
        step: _$ProviderStepEnumMap._$fromName(state.pathParameters['step']!)!,
      );

  ProviderOnboardingRoute get _self => this as ProviderOnboardingRoute;

  @override
  String get location => GoRouteData.$location(
    '/provider-onboarding/${Uri.encodeComponent(_$ProviderStepEnumMap[_self.step]!)}',
  );

  @override
  void go(BuildContext context) => context.go(location);

  @override
  Future<T?> push<T>(BuildContext context) => context.push<T>(location);

  @override
  void pushReplacement(BuildContext context) =>
      context.pushReplacement(location);

  @override
  void replace(BuildContext context) => context.replace(location);
}

RouteBase get $providerShellRoute => StatefulShellRouteData.$route(
  factory: $ProviderShellRouteExtension._fromState,
  branches: [
    StatefulShellBranchData.$branch(
      routes: [
        GoRouteData.$route(
          path: '/provider-dashboard',
          hasOverriddenOnExit: false,
          factory: $ProviderDashboardRoute._fromState,
        ),
      ],
    ),
    StatefulShellBranchData.$branch(
      routes: [
        GoRouteData.$route(
          path: '/provider-profile',
          hasOverriddenOnExit: false,
          factory: $ProviderProfileRoute._fromState,
        ),
      ],
    ),
    StatefulShellBranchData.$branch(
      routes: [
        GoRouteData.$route(
          path: '/provider-account',
          hasOverriddenOnExit: false,
          factory: $ProviderAccountRoute._fromState,
        ),
      ],
    ),
  ],
);

extension $ProviderShellRouteExtension on ProviderShellRoute {
  static ProviderShellRoute _fromState(GoRouterState state) =>
      const ProviderShellRoute();
}

mixin $ProviderDashboardRoute on GoRouteData {
  static ProviderDashboardRoute _fromState(GoRouterState state) =>
      const ProviderDashboardRoute();

  @override
  String get location => GoRouteData.$location('/provider-dashboard');

  @override
  void go(BuildContext context) => context.go(location);

  @override
  Future<T?> push<T>(BuildContext context) => context.push<T>(location);

  @override
  void pushReplacement(BuildContext context) =>
      context.pushReplacement(location);

  @override
  void replace(BuildContext context) => context.replace(location);
}

mixin $ProviderProfileRoute on GoRouteData {
  static ProviderProfileRoute _fromState(GoRouterState state) =>
      const ProviderProfileRoute();

  @override
  String get location => GoRouteData.$location('/provider-profile');

  @override
  void go(BuildContext context) => context.go(location);

  @override
  Future<T?> push<T>(BuildContext context) => context.push<T>(location);

  @override
  void pushReplacement(BuildContext context) =>
      context.pushReplacement(location);

  @override
  void replace(BuildContext context) => context.replace(location);
}

mixin $ProviderAccountRoute on GoRouteData {
  static ProviderAccountRoute _fromState(GoRouterState state) =>
      const ProviderAccountRoute();

  @override
  String get location => GoRouteData.$location('/provider-account');

  @override
  void go(BuildContext context) => context.go(location);

  @override
  Future<T?> push<T>(BuildContext context) => context.push<T>(location);

  @override
  void pushReplacement(BuildContext context) =>
      context.pushReplacement(location);

  @override
  void replace(BuildContext context) => context.replace(location);
}

RouteBase get $publicProviderRoute => GoRouteData.$route(
  path: '/providers/:id',
  hasOverriddenOnExit: false,
  factory: $PublicProviderRoute._fromState,
);

mixin $PublicProviderRoute on GoRouteData {
  static PublicProviderRoute _fromState(GoRouterState state) =>
      PublicProviderRoute(id: state.pathParameters['id']!);

  PublicProviderRoute get _self => this as PublicProviderRoute;

  @override
  String get location =>
      GoRouteData.$location('/providers/${Uri.encodeComponent(_self.id)}');

  @override
  void go(BuildContext context) => context.go(location);

  @override
  Future<T?> push<T>(BuildContext context) => context.push<T>(location);

  @override
  void pushReplacement(BuildContext context) =>
      context.pushReplacement(location);

  @override
  void replace(BuildContext context) => context.replace(location);
}

RouteBase get $shareLinkRoute => GoRouteData.$route(
  path: '/p/:id',
  hasOverriddenOnExit: false,
  factory: $ShareLinkRoute._fromState,
);

mixin $ShareLinkRoute on GoRouteData {
  static ShareLinkRoute _fromState(GoRouterState state) =>
      ShareLinkRoute(id: state.pathParameters['id']!);

  ShareLinkRoute get _self => this as ShareLinkRoute;

  @override
  String get location =>
      GoRouteData.$location('/p/${Uri.encodeComponent(_self.id)}');

  @override
  void go(BuildContext context) => context.go(location);

  @override
  Future<T?> push<T>(BuildContext context) => context.push<T>(location);

  @override
  void pushReplacement(BuildContext context) =>
      context.pushReplacement(location);

  @override
  void replace(BuildContext context) => context.replace(location);
}

RouteBase get $signInRoute => GoRouteData.$route(
  path: '/sign-in',
  hasOverriddenOnExit: false,
  factory: $SignInRoute._fromState,
);

mixin $SignInRoute on GoRouteData {
  static SignInRoute _fromState(GoRouterState state) => const SignInRoute();

  @override
  String get location => GoRouteData.$location('/sign-in');

  @override
  void go(BuildContext context) => context.go(location);

  @override
  Future<T?> push<T>(BuildContext context) => context.push<T>(location);

  @override
  void pushReplacement(BuildContext context) =>
      context.pushReplacement(location);

  @override
  void replace(BuildContext context) => context.replace(location);
}

RouteBase get $signUpRoute => GoRouteData.$route(
  path: '/sign-up',
  hasOverriddenOnExit: false,
  factory: $SignUpRoute._fromState,
);

mixin $SignUpRoute on GoRouteData {
  static SignUpRoute _fromState(GoRouterState state) => const SignUpRoute();

  @override
  String get location => GoRouteData.$location('/sign-up');

  @override
  void go(BuildContext context) => context.go(location);

  @override
  Future<T?> push<T>(BuildContext context) => context.push<T>(location);

  @override
  void pushReplacement(BuildContext context) =>
      context.pushReplacement(location);

  @override
  void replace(BuildContext context) => context.replace(location);
}

RouteBase get $verifyEmailRoute => GoRouteData.$route(
  path: '/verify-email',
  hasOverriddenOnExit: false,
  factory: $VerifyEmailRoute._fromState,
);

mixin $VerifyEmailRoute on GoRouteData {
  static VerifyEmailRoute _fromState(GoRouterState state) =>
      const VerifyEmailRoute();

  @override
  String get location => GoRouteData.$location('/verify-email');

  @override
  void go(BuildContext context) => context.go(location);

  @override
  Future<T?> push<T>(BuildContext context) => context.push<T>(location);

  @override
  void pushReplacement(BuildContext context) =>
      context.pushReplacement(location);

  @override
  void replace(BuildContext context) => context.replace(location);
}

RouteBase get $welcomeRoute => GoRouteData.$route(
  path: '/welcome',
  hasOverriddenOnExit: false,
  factory: $WelcomeRoute._fromState,
);

mixin $WelcomeRoute on GoRouteData {
  static WelcomeRoute _fromState(GoRouterState state) => const WelcomeRoute();

  @override
  String get location => GoRouteData.$location('/welcome');

  @override
  void go(BuildContext context) => context.go(location);

  @override
  Future<T?> push<T>(BuildContext context) => context.push<T>(location);

  @override
  void pushReplacement(BuildContext context) =>
      context.pushReplacement(location);

  @override
  void replace(BuildContext context) => context.replace(location);
}
