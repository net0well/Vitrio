// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'search_filters_controller.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(SearchFiltersController)
final searchFiltersControllerProvider = SearchFiltersControllerProvider._();

final class SearchFiltersControllerProvider
    extends $NotifierProvider<SearchFiltersController, SearchFilters> {
  SearchFiltersControllerProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'searchFiltersControllerProvider',
        isAutoDispose: false,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$searchFiltersControllerHash();

  @$internal
  @override
  SearchFiltersController create() => SearchFiltersController();

  /// {@macro riverpod.override_with_value}
  Override overrideWithValue(SearchFilters value) {
    return $ProviderOverride(
      origin: this,
      providerOverride: $SyncValueProvider<SearchFilters>(value),
    );
  }
}

String _$searchFiltersControllerHash() =>
    r'f51a568c19ad2f0d9d349242525f55fd068ee77c';

abstract class _$SearchFiltersController extends $Notifier<SearchFilters> {
  SearchFilters build();
  @$mustCallSuper
  @override
  WhenComplete runBuild() {
    final ref = this.ref as $Ref<SearchFilters, SearchFilters>;
    final element =
        ref.element
            as $ClassProviderElement<
              AnyNotifier<SearchFilters, SearchFilters>,
              SearchFilters,
              Object?,
              Object?
            >;
    return element.handleCreate(ref, build);
  }
}
