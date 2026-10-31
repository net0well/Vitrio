// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'search_results_controller.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(SearchResultsController)
final searchResultsControllerProvider = SearchResultsControllerProvider._();

final class SearchResultsControllerProvider
    extends $AsyncNotifierProvider<SearchResultsController, SearchResults?> {
  SearchResultsControllerProvider._()
    : super(
        from: null,
        argument: null,
        retry: noRetry,
        name: r'searchResultsControllerProvider',
        isAutoDispose: true,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$searchResultsControllerHash();

  @$internal
  @override
  SearchResultsController create() => SearchResultsController();
}

String _$searchResultsControllerHash() =>
    r'f91f86efc727a03eed4413e19b5fb7f9e73090c9';

abstract class _$SearchResultsController
    extends $AsyncNotifier<SearchResults?> {
  FutureOr<SearchResults?> build();
  @$mustCallSuper
  @override
  WhenComplete runBuild() {
    final ref = this.ref as $Ref<AsyncValue<SearchResults?>, SearchResults?>;
    final element =
        ref.element
            as $ClassProviderElement<
              AnyNotifier<AsyncValue<SearchResults?>, SearchResults?>,
              AsyncValue<SearchResults?>,
              Object?,
              Object?
            >;
    return element.handleCreate(ref, build);
  }
}
