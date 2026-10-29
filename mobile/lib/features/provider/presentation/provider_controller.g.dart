// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'provider_controller.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(ProviderController)
final providerControllerProvider = ProviderControllerProvider._();

final class ProviderControllerProvider
    extends $AsyncNotifierProvider<ProviderController, ProviderProfile?> {
  ProviderControllerProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'providerControllerProvider',
        isAutoDispose: false,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$providerControllerHash();

  @$internal
  @override
  ProviderController create() => ProviderController();
}

String _$providerControllerHash() =>
    r'bb5002b626396428a2ccaf13e11702a57a6cb79a';

abstract class _$ProviderController extends $AsyncNotifier<ProviderProfile?> {
  FutureOr<ProviderProfile?> build();
  @$mustCallSuper
  @override
  WhenComplete runBuild() {
    final ref =
        this.ref as $Ref<AsyncValue<ProviderProfile?>, ProviderProfile?>;
    final element =
        ref.element
            as $ClassProviderElement<
              AnyNotifier<AsyncValue<ProviderProfile?>, ProviderProfile?>,
              AsyncValue<ProviderProfile?>,
              Object?,
              Object?
            >;
    return element.handleCreate(ref, build);
  }
}
