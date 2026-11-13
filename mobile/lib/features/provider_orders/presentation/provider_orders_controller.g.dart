// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'provider_orders_controller.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(ProviderOrdersController)
final providerOrdersControllerProvider = ProviderOrdersControllerFamily._();

final class ProviderOrdersControllerProvider
    extends
        $AsyncNotifierProvider<ProviderOrdersController, ProviderOrdersPage> {
  ProviderOrdersControllerProvider._({
    required ProviderOrdersControllerFamily super.from,
    required ProviderOrderTab super.argument,
  }) : super(
         retry: noRetry,
         name: r'providerOrdersControllerProvider',
         isAutoDispose: true,
         dependencies: null,
         $allTransitiveDependencies: null,
       );

  @override
  String debugGetCreateSourceHash() => _$providerOrdersControllerHash();

  @override
  String toString() {
    return r'providerOrdersControllerProvider'
        ''
        '($argument)';
  }

  @$internal
  @override
  ProviderOrdersController create() => ProviderOrdersController();

  @override
  bool operator ==(Object other) {
    return other is ProviderOrdersControllerProvider &&
        other.argument == argument;
  }

  @override
  int get hashCode {
    return argument.hashCode;
  }
}

String _$providerOrdersControllerHash() =>
    r'd09003811aa7626f8fb852c7f20726401d922f93';

final class ProviderOrdersControllerFamily extends $Family
    with
        $ClassFamilyOverride<
          ProviderOrdersController,
          AsyncValue<ProviderOrdersPage>,
          ProviderOrdersPage,
          FutureOr<ProviderOrdersPage>,
          ProviderOrderTab
        > {
  ProviderOrdersControllerFamily._()
    : super(
        retry: noRetry,
        name: r'providerOrdersControllerProvider',
        dependencies: null,
        $allTransitiveDependencies: null,
        isAutoDispose: true,
      );

  ProviderOrdersControllerProvider call(ProviderOrderTab tab) =>
      ProviderOrdersControllerProvider._(argument: tab, from: this);

  @override
  String toString() => r'providerOrdersControllerProvider';
}

abstract class _$ProviderOrdersController
    extends $AsyncNotifier<ProviderOrdersPage> {
  late final _$args = ref.$arg as ProviderOrderTab;
  ProviderOrderTab get tab => _$args;

  FutureOr<ProviderOrdersPage> build(ProviderOrderTab tab);
  @$mustCallSuper
  @override
  WhenComplete runBuild() {
    final ref =
        this.ref as $Ref<AsyncValue<ProviderOrdersPage>, ProviderOrdersPage>;
    final element =
        ref.element
            as $ClassProviderElement<
              AnyNotifier<AsyncValue<ProviderOrdersPage>, ProviderOrdersPage>,
              AsyncValue<ProviderOrdersPage>,
              Object?,
              Object?
            >;
    return element.handleCreate(ref, () => build(_$args));
  }
}
