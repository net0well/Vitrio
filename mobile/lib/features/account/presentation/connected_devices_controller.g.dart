// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'connected_devices_controller.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(ConnectedDevicesController)
final connectedDevicesControllerProvider =
    ConnectedDevicesControllerProvider._();

final class ConnectedDevicesControllerProvider
    extends
        $AsyncNotifierProvider<
          ConnectedDevicesController,
          List<ConnectedSession>
        > {
  ConnectedDevicesControllerProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'connectedDevicesControllerProvider',
        isAutoDispose: true,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$connectedDevicesControllerHash();

  @$internal
  @override
  ConnectedDevicesController create() => ConnectedDevicesController();
}

String _$connectedDevicesControllerHash() =>
    r'02a017d2b9affdf034d83e871c203f5f089d0793';

abstract class _$ConnectedDevicesController
    extends $AsyncNotifier<List<ConnectedSession>> {
  FutureOr<List<ConnectedSession>> build();
  @$mustCallSuper
  @override
  WhenComplete runBuild() {
    final ref =
        this.ref
            as $Ref<AsyncValue<List<ConnectedSession>>, List<ConnectedSession>>;
    final element =
        ref.element
            as $ClassProviderElement<
              AnyNotifier<
                AsyncValue<List<ConnectedSession>>,
                List<ConnectedSession>
              >,
              AsyncValue<List<ConnectedSession>>,
              Object?,
              Object?
            >;
    return element.handleCreate(ref, build);
  }
}
