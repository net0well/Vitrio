// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'pending_auth_intent.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(PendingAuthIntent)
final pendingAuthIntentProvider = PendingAuthIntentProvider._();

final class PendingAuthIntentProvider
    extends $NotifierProvider<PendingAuthIntent, AuthIntent?> {
  PendingAuthIntentProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'pendingAuthIntentProvider',
        isAutoDispose: false,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$pendingAuthIntentHash();

  @$internal
  @override
  PendingAuthIntent create() => PendingAuthIntent();

  /// {@macro riverpod.override_with_value}
  Override overrideWithValue(AuthIntent? value) {
    return $ProviderOverride(
      origin: this,
      providerOverride: $SyncValueProvider<AuthIntent?>(value),
    );
  }
}

String _$pendingAuthIntentHash() => r'2354f182950dce09b49a98497684e9bf33fd3911';

abstract class _$PendingAuthIntent extends $Notifier<AuthIntent?> {
  AuthIntent? build();
  @$mustCallSuper
  @override
  WhenComplete runBuild() {
    final ref = this.ref as $Ref<AuthIntent?, AuthIntent?>;
    final element =
        ref.element
            as $ClassProviderElement<
              AnyNotifier<AuthIntent?, AuthIntent?>,
              AuthIntent?,
              Object?,
              Object?
            >;
    return element.handleCreate(ref, build);
  }
}
