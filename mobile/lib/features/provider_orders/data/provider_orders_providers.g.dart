// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'provider_orders_providers.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(providerOrdersRepository)
final providerOrdersRepositoryProvider = ProviderOrdersRepositoryProvider._();

final class ProviderOrdersRepositoryProvider
    extends
        $FunctionalProvider<
          ProviderOrdersRepository,
          ProviderOrdersRepository,
          ProviderOrdersRepository
        >
    with $Provider<ProviderOrdersRepository> {
  ProviderOrdersRepositoryProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'providerOrdersRepositoryProvider',
        isAutoDispose: false,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$providerOrdersRepositoryHash();

  @$internal
  @override
  $ProviderElement<ProviderOrdersRepository> $createElement(
    $ProviderPointer pointer,
  ) => $ProviderElement(pointer);

  @override
  ProviderOrdersRepository create(Ref ref) {
    return providerOrdersRepository(ref);
  }

  /// {@macro riverpod.override_with_value}
  Override overrideWithValue(ProviderOrdersRepository value) {
    return $ProviderOverride(
      origin: this,
      providerOverride: $SyncValueProvider<ProviderOrdersRepository>(value),
    );
  }
}

String _$providerOrdersRepositoryHash() =>
    r'3aa4b3169bc758df07e3d3e2be13a4c191f64500';
