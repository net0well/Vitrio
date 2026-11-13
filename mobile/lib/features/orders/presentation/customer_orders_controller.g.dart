// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'customer_orders_controller.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(CustomerOrdersController)
final customerOrdersControllerProvider = CustomerOrdersControllerFamily._();

final class CustomerOrdersControllerProvider
    extends
        $AsyncNotifierProvider<CustomerOrdersController, CustomerOrdersPage> {
  CustomerOrdersControllerProvider._({
    required CustomerOrdersControllerFamily super.from,
    required bool super.argument,
  }) : super(
         retry: noRetry,
         name: r'customerOrdersControllerProvider',
         isAutoDispose: true,
         dependencies: null,
         $allTransitiveDependencies: null,
       );

  @override
  String debugGetCreateSourceHash() => _$customerOrdersControllerHash();

  @override
  String toString() {
    return r'customerOrdersControllerProvider'
        ''
        '($argument)';
  }

  @$internal
  @override
  CustomerOrdersController create() => CustomerOrdersController();

  @override
  bool operator ==(Object other) {
    return other is CustomerOrdersControllerProvider &&
        other.argument == argument;
  }

  @override
  int get hashCode {
    return argument.hashCode;
  }
}

String _$customerOrdersControllerHash() =>
    r'b882106648815f3eb17c1f1ec2e0859c987bbb7b';

final class CustomerOrdersControllerFamily extends $Family
    with
        $ClassFamilyOverride<
          CustomerOrdersController,
          AsyncValue<CustomerOrdersPage>,
          CustomerOrdersPage,
          FutureOr<CustomerOrdersPage>,
          bool
        > {
  CustomerOrdersControllerFamily._()
    : super(
        retry: noRetry,
        name: r'customerOrdersControllerProvider',
        dependencies: null,
        $allTransitiveDependencies: null,
        isAutoDispose: true,
      );

  CustomerOrdersControllerProvider call({required bool open}) =>
      CustomerOrdersControllerProvider._(argument: open, from: this);

  @override
  String toString() => r'customerOrdersControllerProvider';
}

abstract class _$CustomerOrdersController
    extends $AsyncNotifier<CustomerOrdersPage> {
  late final _$args = ref.$arg as bool;
  bool get open => _$args;

  FutureOr<CustomerOrdersPage> build({required bool open});
  @$mustCallSuper
  @override
  WhenComplete runBuild() {
    final ref =
        this.ref as $Ref<AsyncValue<CustomerOrdersPage>, CustomerOrdersPage>;
    final element =
        ref.element
            as $ClassProviderElement<
              AnyNotifier<AsyncValue<CustomerOrdersPage>, CustomerOrdersPage>,
              AsyncValue<CustomerOrdersPage>,
              Object?,
              Object?
            >;
    return element.handleCreate(ref, () => build(open: _$args));
  }
}

@ProviderFor(latestOpenOrder)
final latestOpenOrderProvider = LatestOpenOrderProvider._();

final class LatestOpenOrderProvider
    extends
        $FunctionalProvider<
          AsyncValue<CustomerOrderSummary?>,
          CustomerOrderSummary?,
          FutureOr<CustomerOrderSummary?>
        >
    with
        $FutureModifier<CustomerOrderSummary?>,
        $FutureProvider<CustomerOrderSummary?> {
  LatestOpenOrderProvider._()
    : super(
        from: null,
        argument: null,
        retry: noRetry,
        name: r'latestOpenOrderProvider',
        isAutoDispose: true,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$latestOpenOrderHash();

  @$internal
  @override
  $FutureProviderElement<CustomerOrderSummary?> $createElement(
    $ProviderPointer pointer,
  ) => $FutureProviderElement(pointer);

  @override
  FutureOr<CustomerOrderSummary?> create(Ref ref) {
    return latestOpenOrder(ref);
  }
}

String _$latestOpenOrderHash() => r'67cb5d636872b0f68d1882118f8902d7a2ba038c';
