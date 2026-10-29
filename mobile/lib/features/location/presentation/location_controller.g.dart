// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'location_controller.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(LocationController)
final locationControllerProvider = LocationControllerProvider._();

final class LocationControllerProvider
    extends $AsyncNotifierProvider<LocationController, LocationState> {
  LocationControllerProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'locationControllerProvider',
        isAutoDispose: false,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$locationControllerHash();

  @$internal
  @override
  LocationController create() => LocationController();
}

String _$locationControllerHash() =>
    r'52c0f6ff4d18abf65a31d6df1d783217d03f5d3e';

abstract class _$LocationController extends $AsyncNotifier<LocationState> {
  FutureOr<LocationState> build();
  @$mustCallSuper
  @override
  WhenComplete runBuild() {
    final ref = this.ref as $Ref<AsyncValue<LocationState>, LocationState>;
    final element =
        ref.element
            as $ClassProviderElement<
              AnyNotifier<AsyncValue<LocationState>, LocationState>,
              AsyncValue<LocationState>,
              Object?,
              Object?
            >;
    return element.handleCreate(ref, build);
  }
}
