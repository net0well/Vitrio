// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'favorites_controller.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(FavoritesController)
final favoritesControllerProvider = FavoritesControllerProvider._();

final class FavoritesControllerProvider
    extends $AsyncNotifierProvider<FavoritesController, FavoritesPage> {
  FavoritesControllerProvider._()
    : super(
        from: null,
        argument: null,
        retry: noRetry,
        name: r'favoritesControllerProvider',
        isAutoDispose: true,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$favoritesControllerHash();

  @$internal
  @override
  FavoritesController create() => FavoritesController();
}

String _$favoritesControllerHash() =>
    r'58faba39dfab16b9734c68a994e521b9e3798aac';

abstract class _$FavoritesController extends $AsyncNotifier<FavoritesPage> {
  FutureOr<FavoritesPage> build();
  @$mustCallSuper
  @override
  WhenComplete runBuild() {
    final ref = this.ref as $Ref<AsyncValue<FavoritesPage>, FavoritesPage>;
    final element =
        ref.element
            as $ClassProviderElement<
              AnyNotifier<AsyncValue<FavoritesPage>, FavoritesPage>,
              AsyncValue<FavoritesPage>,
              Object?,
              Object?
            >;
    return element.handleCreate(ref, build);
  }
}
