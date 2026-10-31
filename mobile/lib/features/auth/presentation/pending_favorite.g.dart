// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'pending_favorite.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(PendingFavorite)
final pendingFavoriteProvider = PendingFavoriteProvider._();

final class PendingFavoriteProvider
    extends $NotifierProvider<PendingFavorite, String?> {
  PendingFavoriteProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'pendingFavoriteProvider',
        isAutoDispose: false,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$pendingFavoriteHash();

  @$internal
  @override
  PendingFavorite create() => PendingFavorite();

  /// {@macro riverpod.override_with_value}
  Override overrideWithValue(String? value) {
    return $ProviderOverride(
      origin: this,
      providerOverride: $SyncValueProvider<String?>(value),
    );
  }
}

String _$pendingFavoriteHash() => r'22bc5cd6c552498594bec8da4e8eef4e9ccde561';

abstract class _$PendingFavorite extends $Notifier<String?> {
  String? build();
  @$mustCallSuper
  @override
  WhenComplete runBuild() {
    final ref = this.ref as $Ref<String?, String?>;
    final element =
        ref.element
            as $ClassProviderElement<
              AnyNotifier<String?, String?>,
              String?,
              Object?,
              Object?
            >;
    return element.handleCreate(ref, build);
  }
}
