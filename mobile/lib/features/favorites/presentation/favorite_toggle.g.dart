// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'favorite_toggle.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(FavoriteToggle)
final favoriteToggleProvider = FavoriteToggleFamily._();

final class FavoriteToggleProvider
    extends $NotifierProvider<FavoriteToggle, bool> {
  FavoriteToggleProvider._({
    required FavoriteToggleFamily super.from,
    required String super.argument,
  }) : super(
         retry: null,
         name: r'favoriteToggleProvider',
         isAutoDispose: true,
         dependencies: null,
         $allTransitiveDependencies: null,
       );

  @override
  String debugGetCreateSourceHash() => _$favoriteToggleHash();

  @override
  String toString() {
    return r'favoriteToggleProvider'
        ''
        '($argument)';
  }

  @$internal
  @override
  FavoriteToggle create() => FavoriteToggle();

  /// {@macro riverpod.override_with_value}
  Override overrideWithValue(bool value) {
    return $ProviderOverride(
      origin: this,
      providerOverride: $SyncValueProvider<bool>(value),
    );
  }

  @override
  bool operator ==(Object other) {
    return other is FavoriteToggleProvider && other.argument == argument;
  }

  @override
  int get hashCode {
    return argument.hashCode;
  }
}

String _$favoriteToggleHash() => r'7e8b9097363afe91c515aa012afce011e449f159';

final class FavoriteToggleFamily extends $Family
    with $ClassFamilyOverride<FavoriteToggle, bool, bool, bool, String> {
  FavoriteToggleFamily._()
    : super(
        retry: null,
        name: r'favoriteToggleProvider',
        dependencies: null,
        $allTransitiveDependencies: null,
        isAutoDispose: true,
      );

  FavoriteToggleProvider call(String providerId) =>
      FavoriteToggleProvider._(argument: providerId, from: this);

  @override
  String toString() => r'favoriteToggleProvider';
}

abstract class _$FavoriteToggle extends $Notifier<bool> {
  late final _$args = ref.$arg as String;
  String get providerId => _$args;

  bool build(String providerId);
  @$mustCallSuper
  @override
  WhenComplete runBuild() {
    final ref = this.ref as $Ref<bool, bool>;
    final element =
        ref.element
            as $ClassProviderElement<
              AnyNotifier<bool, bool>,
              bool,
              Object?,
              Object?
            >;
    return element.handleCreate(ref, () => build(_$args));
  }
}
