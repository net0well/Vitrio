// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'welcome_controller.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(WelcomeController)
final welcomeControllerProvider = WelcomeControllerProvider._();

final class WelcomeControllerProvider
    extends $NotifierProvider<WelcomeController, bool> {
  WelcomeControllerProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'welcomeControllerProvider',
        isAutoDispose: false,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$welcomeControllerHash();

  @$internal
  @override
  WelcomeController create() => WelcomeController();

  /// {@macro riverpod.override_with_value}
  Override overrideWithValue(bool value) {
    return $ProviderOverride(
      origin: this,
      providerOverride: $SyncValueProvider<bool>(value),
    );
  }
}

String _$welcomeControllerHash() => r'2bd8291a493e9a7db06fc81c83f2e9ae7ec1630c';

abstract class _$WelcomeController extends $Notifier<bool> {
  bool build();
  @$mustCallSuper
  @override
  WhenComplete runBuild() {
    final ref = this.ref as $Ref<bool, bool>;
    final element =
        ref.element
            as $ClassProviderElement<
              AnyNotifier<bool, bool>,
              bool,
              Object?,
              Object?
            >;
    return element.handleCreate(ref, build);
  }
}
