// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'customer_order_controller.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(CustomerOrderController)
final customerOrderControllerProvider = CustomerOrderControllerFamily._();

final class CustomerOrderControllerProvider
    extends $AsyncNotifierProvider<CustomerOrderController, CustomerOrder> {
  CustomerOrderControllerProvider._({
    required CustomerOrderControllerFamily super.from,
    required String super.argument,
  }) : super(
         retry: noRetry,
         name: r'customerOrderControllerProvider',
         isAutoDispose: true,
         dependencies: null,
         $allTransitiveDependencies: null,
       );

  @override
  String debugGetCreateSourceHash() => _$customerOrderControllerHash();

  @override
  String toString() {
    return r'customerOrderControllerProvider'
        ''
        '($argument)';
  }

  @$internal
  @override
  CustomerOrderController create() => CustomerOrderController();

  @override
  bool operator ==(Object other) {
    return other is CustomerOrderControllerProvider &&
        other.argument == argument;
  }

  @override
  int get hashCode {
    return argument.hashCode;
  }
}

String _$customerOrderControllerHash() =>
    r'008597af458d949c2a944db75da637b937efe49e';

final class CustomerOrderControllerFamily extends $Family
    with
        $ClassFamilyOverride<
          CustomerOrderController,
          AsyncValue<CustomerOrder>,
          CustomerOrder,
          FutureOr<CustomerOrder>,
          String
        > {
  CustomerOrderControllerFamily._()
    : super(
        retry: noRetry,
        name: r'customerOrderControllerProvider',
        dependencies: null,
        $allTransitiveDependencies: null,
        isAutoDispose: true,
      );

  CustomerOrderControllerProvider call(String orderId) =>
      CustomerOrderControllerProvider._(argument: orderId, from: this);

  @override
  String toString() => r'customerOrderControllerProvider';
}

abstract class _$CustomerOrderController extends $AsyncNotifier<CustomerOrder> {
  late final _$args = ref.$arg as String;
  String get orderId => _$args;

  FutureOr<CustomerOrder> build(String orderId);
  @$mustCallSuper
  @override
  WhenComplete runBuild() {
    final ref = this.ref as $Ref<AsyncValue<CustomerOrder>, CustomerOrder>;
    final element =
        ref.element
            as $ClassProviderElement<
              AnyNotifier<AsyncValue<CustomerOrder>, CustomerOrder>,
              AsyncValue<CustomerOrder>,
              Object?,
              Object?
            >;
    return element.handleCreate(ref, () => build(_$args));
  }
}

@ProviderFor(orderSuggestions)
final orderSuggestionsProvider = OrderSuggestionsFamily._();

final class OrderSuggestionsProvider
    extends
        $FunctionalProvider<
          AsyncValue<List<ProviderListing>>,
          List<ProviderListing>,
          FutureOr<List<ProviderListing>>
        >
    with
        $FutureModifier<List<ProviderListing>>,
        $FutureProvider<List<ProviderListing>> {
  OrderSuggestionsProvider._({
    required OrderSuggestionsFamily super.from,
    required String super.argument,
  }) : super(
         retry: noRetry,
         name: r'orderSuggestionsProvider',
         isAutoDispose: true,
         dependencies: null,
         $allTransitiveDependencies: null,
       );

  @override
  String debugGetCreateSourceHash() => _$orderSuggestionsHash();

  @override
  String toString() {
    return r'orderSuggestionsProvider'
        ''
        '($argument)';
  }

  @$internal
  @override
  $FutureProviderElement<List<ProviderListing>> $createElement(
    $ProviderPointer pointer,
  ) => $FutureProviderElement(pointer);

  @override
  FutureOr<List<ProviderListing>> create(Ref ref) {
    final argument = this.argument as String;
    return orderSuggestions(ref, argument);
  }

  @override
  bool operator ==(Object other) {
    return other is OrderSuggestionsProvider && other.argument == argument;
  }

  @override
  int get hashCode {
    return argument.hashCode;
  }
}

String _$orderSuggestionsHash() => r'72ce23d4c71106697ca4becba200b7762dff3b99';

final class OrderSuggestionsFamily extends $Family
    with $FunctionalFamilyOverride<FutureOr<List<ProviderListing>>, String> {
  OrderSuggestionsFamily._()
    : super(
        retry: noRetry,
        name: r'orderSuggestionsProvider',
        dependencies: null,
        $allTransitiveDependencies: null,
        isAutoDispose: true,
      );

  OrderSuggestionsProvider call(String orderId) =>
      OrderSuggestionsProvider._(argument: orderId, from: this);

  @override
  String toString() => r'orderSuggestionsProvider';
}
