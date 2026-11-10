// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'provider_order_controller.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(ProviderOrderController)
final providerOrderControllerProvider = ProviderOrderControllerFamily._();

final class ProviderOrderControllerProvider
    extends $AsyncNotifierProvider<ProviderOrderController, ProviderOrder> {
  ProviderOrderControllerProvider._({
    required ProviderOrderControllerFamily super.from,
    required String super.argument,
  }) : super(
         retry: noRetry,
         name: r'providerOrderControllerProvider',
         isAutoDispose: true,
         dependencies: null,
         $allTransitiveDependencies: null,
       );

  @override
  String debugGetCreateSourceHash() => _$providerOrderControllerHash();

  @override
  String toString() {
    return r'providerOrderControllerProvider'
        ''
        '($argument)';
  }

  @$internal
  @override
  ProviderOrderController create() => ProviderOrderController();

  @override
  bool operator ==(Object other) {
    return other is ProviderOrderControllerProvider &&
        other.argument == argument;
  }

  @override
  int get hashCode {
    return argument.hashCode;
  }
}

String _$providerOrderControllerHash() =>
    r'84cb199b625c555d95fb094747a4350f5cce66e3';

final class ProviderOrderControllerFamily extends $Family
    with
        $ClassFamilyOverride<
          ProviderOrderController,
          AsyncValue<ProviderOrder>,
          ProviderOrder,
          FutureOr<ProviderOrder>,
          String
        > {
  ProviderOrderControllerFamily._()
    : super(
        retry: noRetry,
        name: r'providerOrderControllerProvider',
        dependencies: null,
        $allTransitiveDependencies: null,
        isAutoDispose: true,
      );

  ProviderOrderControllerProvider call(String orderId) =>
      ProviderOrderControllerProvider._(argument: orderId, from: this);

  @override
  String toString() => r'providerOrderControllerProvider';
}

abstract class _$ProviderOrderController extends $AsyncNotifier<ProviderOrder> {
  late final _$args = ref.$arg as String;
  String get orderId => _$args;

  FutureOr<ProviderOrder> build(String orderId);
  @$mustCallSuper
  @override
  WhenComplete runBuild() {
    final ref = this.ref as $Ref<AsyncValue<ProviderOrder>, ProviderOrder>;
    final element =
        ref.element
            as $ClassProviderElement<
              AnyNotifier<AsyncValue<ProviderOrder>, ProviderOrder>,
              AsyncValue<ProviderOrder>,
              Object?,
              Object?
            >;
    return element.handleCreate(ref, () => build(_$args));
  }
}
