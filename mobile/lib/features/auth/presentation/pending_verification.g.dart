// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'pending_verification.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(PendingVerification)
final pendingVerificationProvider = PendingVerificationProvider._();

final class PendingVerificationProvider
    extends $NotifierProvider<PendingVerification, PendingCredentials?> {
  PendingVerificationProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'pendingVerificationProvider',
        isAutoDispose: false,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$pendingVerificationHash();

  @$internal
  @override
  PendingVerification create() => PendingVerification();

  /// {@macro riverpod.override_with_value}
  Override overrideWithValue(PendingCredentials? value) {
    return $ProviderOverride(
      origin: this,
      providerOverride: $SyncValueProvider<PendingCredentials?>(value),
    );
  }
}

String _$pendingVerificationHash() =>
    r'757d26423ea8c5c898046b60b7808ddcd59f6fb8';

abstract class _$PendingVerification extends $Notifier<PendingCredentials?> {
  PendingCredentials? build();
  @$mustCallSuper
  @override
  WhenComplete runBuild() {
    final ref = this.ref as $Ref<PendingCredentials?, PendingCredentials?>;
    final element =
        ref.element
            as $ClassProviderElement<
              AnyNotifier<PendingCredentials?, PendingCredentials?>,
              PendingCredentials?,
              Object?,
              Object?
            >;
    return element.handleCreate(ref, build);
  }
}
