// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'session_expiry.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(SessionExpiry)
final sessionExpiryProvider = SessionExpiryProvider._();

final class SessionExpiryProvider
    extends $NotifierProvider<SessionExpiry, int> {
  SessionExpiryProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'sessionExpiryProvider',
        isAutoDispose: false,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$sessionExpiryHash();

  @$internal
  @override
  SessionExpiry create() => SessionExpiry();

  /// {@macro riverpod.override_with_value}
  Override overrideWithValue(int value) {
    return $ProviderOverride(
      origin: this,
      providerOverride: $SyncValueProvider<int>(value),
    );
  }
}

String _$sessionExpiryHash() => r'eddcc486566edb2f9298a6fb54dae7cf0ad73912';

abstract class _$SessionExpiry extends $Notifier<int> {
  int build();
  @$mustCallSuper
  @override
  WhenComplete runBuild() {
    final ref = this.ref as $Ref<int, int>;
    final element =
        ref.element
            as $ClassProviderElement<
              AnyNotifier<int, int>,
              int,
              Object?,
              Object?
            >;
    return element.handleCreate(ref, build);
  }
}
