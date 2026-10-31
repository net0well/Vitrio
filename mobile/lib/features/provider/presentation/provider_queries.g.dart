// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'provider_queries.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(serviceCategories)
final serviceCategoriesProvider = ServiceCategoriesProvider._();

final class ServiceCategoriesProvider
    extends
        $FunctionalProvider<
          AsyncValue<List<ServiceCategory>>,
          List<ServiceCategory>,
          FutureOr<List<ServiceCategory>>
        >
    with
        $FutureModifier<List<ServiceCategory>>,
        $FutureProvider<List<ServiceCategory>> {
  ServiceCategoriesProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'serviceCategoriesProvider',
        isAutoDispose: false,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$serviceCategoriesHash();

  @$internal
  @override
  $FutureProviderElement<List<ServiceCategory>> $createElement(
    $ProviderPointer pointer,
  ) => $FutureProviderElement(pointer);

  @override
  FutureOr<List<ServiceCategory>> create(Ref ref) {
    return serviceCategories(ref);
  }
}

String _$serviceCategoriesHash() => r'54a0503accc5df0025cfbf10e731a722451950aa';

@ProviderFor(providerDashboard)
final providerDashboardProvider = ProviderDashboardProvider._();

final class ProviderDashboardProvider
    extends
        $FunctionalProvider<
          AsyncValue<ProviderDashboard>,
          ProviderDashboard,
          FutureOr<ProviderDashboard>
        >
    with
        $FutureModifier<ProviderDashboard>,
        $FutureProvider<ProviderDashboard> {
  ProviderDashboardProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'providerDashboardProvider',
        isAutoDispose: true,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$providerDashboardHash();

  @$internal
  @override
  $FutureProviderElement<ProviderDashboard> $createElement(
    $ProviderPointer pointer,
  ) => $FutureProviderElement(pointer);

  @override
  FutureOr<ProviderDashboard> create(Ref ref) {
    return providerDashboard(ref);
  }
}

String _$providerDashboardHash() => r'e22a83956577df809709af1402d40361a6c3acab';

@ProviderFor(providerPhotos)
final providerPhotosProvider = ProviderPhotosProvider._();

final class ProviderPhotosProvider
    extends
        $FunctionalProvider<
          AsyncValue<List<ProviderPhoto>>,
          List<ProviderPhoto>,
          FutureOr<List<ProviderPhoto>>
        >
    with
        $FutureModifier<List<ProviderPhoto>>,
        $FutureProvider<List<ProviderPhoto>> {
  ProviderPhotosProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'providerPhotosProvider',
        isAutoDispose: true,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$providerPhotosHash();

  @$internal
  @override
  $FutureProviderElement<List<ProviderPhoto>> $createElement(
    $ProviderPointer pointer,
  ) => $FutureProviderElement(pointer);

  @override
  FutureOr<List<ProviderPhoto>> create(Ref ref) {
    return providerPhotos(ref);
  }
}

String _$providerPhotosHash() => r'18c0a80b8cc2aedbc5069ad70eb8bd6d4d0357f4';

@ProviderFor(publicProvider)
final publicProviderProvider = PublicProviderFamily._();

final class PublicProviderProvider
    extends
        $FunctionalProvider<
          AsyncValue<PublicProvider?>,
          PublicProvider?,
          FutureOr<PublicProvider?>
        >
    with $FutureModifier<PublicProvider?>, $FutureProvider<PublicProvider?> {
  PublicProviderProvider._({
    required PublicProviderFamily super.from,
    required String super.argument,
  }) : super(
         retry: null,
         name: r'publicProviderProvider',
         isAutoDispose: true,
         dependencies: null,
         $allTransitiveDependencies: null,
       );

  @override
  String debugGetCreateSourceHash() => _$publicProviderHash();

  @override
  String toString() {
    return r'publicProviderProvider'
        ''
        '($argument)';
  }

  @$internal
  @override
  $FutureProviderElement<PublicProvider?> $createElement(
    $ProviderPointer pointer,
  ) => $FutureProviderElement(pointer);

  @override
  FutureOr<PublicProvider?> create(Ref ref) {
    final argument = this.argument as String;
    return publicProvider(ref, argument);
  }

  @override
  bool operator ==(Object other) {
    return other is PublicProviderProvider && other.argument == argument;
  }

  @override
  int get hashCode {
    return argument.hashCode;
  }
}

String _$publicProviderHash() => r'057a350e9ff3393833fb8cdc12543c15e590d21a';

final class PublicProviderFamily extends $Family
    with $FunctionalFamilyOverride<FutureOr<PublicProvider?>, String> {
  PublicProviderFamily._()
    : super(
        retry: null,
        name: r'publicProviderProvider',
        dependencies: null,
        $allTransitiveDependencies: null,
        isAutoDispose: true,
      );

  PublicProviderProvider call(String providerId) =>
      PublicProviderProvider._(argument: providerId, from: this);

  @override
  String toString() => r'publicProviderProvider';
}
