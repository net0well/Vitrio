// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'search_queries.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(searchArea)
final searchAreaProvider = SearchAreaProvider._();

final class SearchAreaProvider
    extends
        $FunctionalProvider<
          AsyncValue<SearchArea?>,
          SearchArea?,
          FutureOr<SearchArea?>
        >
    with $FutureModifier<SearchArea?>, $FutureProvider<SearchArea?> {
  SearchAreaProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'searchAreaProvider',
        isAutoDispose: false,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$searchAreaHash();

  @$internal
  @override
  $FutureProviderElement<SearchArea?> $createElement(
    $ProviderPointer pointer,
  ) => $FutureProviderElement(pointer);

  @override
  FutureOr<SearchArea?> create(Ref ref) {
    return searchArea(ref);
  }
}

String _$searchAreaHash() => r'256e8c55009662a08d6f1446482d389f161e4d17';

@ProviderFor(homeFeed)
final homeFeedProvider = HomeFeedProvider._();

final class HomeFeedProvider
    extends
        $FunctionalProvider<
          AsyncValue<HomeFeed?>,
          HomeFeed?,
          FutureOr<HomeFeed?>
        >
    with $FutureModifier<HomeFeed?>, $FutureProvider<HomeFeed?> {
  HomeFeedProvider._()
    : super(
        from: null,
        argument: null,
        retry: noRetry,
        name: r'homeFeedProvider',
        isAutoDispose: true,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$homeFeedHash();

  @$internal
  @override
  $FutureProviderElement<HomeFeed?> $createElement($ProviderPointer pointer) =>
      $FutureProviderElement(pointer);

  @override
  FutureOr<HomeFeed?> create(Ref ref) {
    return homeFeed(ref);
  }
}

String _$homeFeedHash() => r'3dad79a1ba16352dbf550ad332c2f9d794a3c285';
