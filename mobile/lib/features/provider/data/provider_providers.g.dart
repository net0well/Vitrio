// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'provider_providers.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(providerRepository)
final providerRepositoryProvider = ProviderRepositoryProvider._();

final class ProviderRepositoryProvider
    extends
        $FunctionalProvider<
          ProviderRepository,
          ProviderRepository,
          ProviderRepository
        >
    with $Provider<ProviderRepository> {
  ProviderRepositoryProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'providerRepositoryProvider',
        isAutoDispose: false,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$providerRepositoryHash();

  @$internal
  @override
  $ProviderElement<ProviderRepository> $createElement(
    $ProviderPointer pointer,
  ) => $ProviderElement(pointer);

  @override
  ProviderRepository create(Ref ref) {
    return providerRepository(ref);
  }

  /// {@macro riverpod.override_with_value}
  Override overrideWithValue(ProviderRepository value) {
    return $ProviderOverride(
      origin: this,
      providerOverride: $SyncValueProvider<ProviderRepository>(value),
    );
  }
}

String _$providerRepositoryHash() =>
    r'ca7a328c12880ce7f3d7c734734a1d5df74a93c7';

@ProviderFor(photoStorage)
final photoStorageProvider = PhotoStorageProvider._();

final class PhotoStorageProvider
    extends $FunctionalProvider<PhotoStorage, PhotoStorage, PhotoStorage>
    with $Provider<PhotoStorage> {
  PhotoStorageProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'photoStorageProvider',
        isAutoDispose: false,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$photoStorageHash();

  @$internal
  @override
  $ProviderElement<PhotoStorage> $createElement($ProviderPointer pointer) =>
      $ProviderElement(pointer);

  @override
  PhotoStorage create(Ref ref) {
    return photoStorage(ref);
  }

  /// {@macro riverpod.override_with_value}
  Override overrideWithValue(PhotoStorage value) {
    return $ProviderOverride(
      origin: this,
      providerOverride: $SyncValueProvider<PhotoStorage>(value),
    );
  }
}

String _$photoStorageHash() => r'ee7e4eb50be09f1c6cdc6b012fbe31872e145f24';

@ProviderFor(photoPicker)
final photoPickerProvider = PhotoPickerProvider._();

final class PhotoPickerProvider
    extends $FunctionalProvider<PhotoPicker, PhotoPicker, PhotoPicker>
    with $Provider<PhotoPicker> {
  PhotoPickerProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'photoPickerProvider',
        isAutoDispose: false,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$photoPickerHash();

  @$internal
  @override
  $ProviderElement<PhotoPicker> $createElement($ProviderPointer pointer) =>
      $ProviderElement(pointer);

  @override
  PhotoPicker create(Ref ref) {
    return photoPicker(ref);
  }

  /// {@macro riverpod.override_with_value}
  Override overrideWithValue(PhotoPicker value) {
    return $ProviderOverride(
      origin: this,
      providerOverride: $SyncValueProvider<PhotoPicker>(value),
    );
  }
}

String _$photoPickerHash() => r'7f2148f1628817aaedcb83b09a14df5f4510e4a0';

@ProviderFor(appModeStore)
final appModeStoreProvider = AppModeStoreProvider._();

final class AppModeStoreProvider
    extends $FunctionalProvider<AppModeStore, AppModeStore, AppModeStore>
    with $Provider<AppModeStore> {
  AppModeStoreProvider._()
    : super(
        from: null,
        argument: null,
        retry: null,
        name: r'appModeStoreProvider',
        isAutoDispose: false,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$appModeStoreHash();

  @$internal
  @override
  $ProviderElement<AppModeStore> $createElement($ProviderPointer pointer) =>
      $ProviderElement(pointer);

  @override
  AppModeStore create(Ref ref) {
    return appModeStore(ref);
  }

  /// {@macro riverpod.override_with_value}
  Override overrideWithValue(AppModeStore value) {
    return $ProviderOverride(
      origin: this,
      providerOverride: $SyncValueProvider<AppModeStore>(value),
    );
  }
}

String _$appModeStoreHash() => r'193452f261f52c9de9c733d693387386876cdc62';
