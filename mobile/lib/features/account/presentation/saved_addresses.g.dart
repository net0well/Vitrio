// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'saved_addresses.dart';

// **************************************************************************
// RiverpodGenerator
// **************************************************************************

// GENERATED CODE - DO NOT MODIFY BY HAND
// ignore_for_file: type=lint, type=warning

@ProviderFor(savedAddresses)
final savedAddressesProvider = SavedAddressesProvider._();

final class SavedAddressesProvider
    extends
        $FunctionalProvider<
          AsyncValue<List<SavedAddress>>,
          List<SavedAddress>,
          FutureOr<List<SavedAddress>>
        >
    with
        $FutureModifier<List<SavedAddress>>,
        $FutureProvider<List<SavedAddress>> {
  SavedAddressesProvider._()
    : super(
        from: null,
        argument: null,
        retry: noRetry,
        name: r'savedAddressesProvider',
        isAutoDispose: true,
        dependencies: null,
        $allTransitiveDependencies: null,
      );

  @override
  String debugGetCreateSourceHash() => _$savedAddressesHash();

  @$internal
  @override
  $FutureProviderElement<List<SavedAddress>> $createElement(
    $ProviderPointer pointer,
  ) => $FutureProviderElement(pointer);

  @override
  FutureOr<List<SavedAddress>> create(Ref ref) {
    return savedAddresses(ref);
  }
}

String _$savedAddressesHash() => r'a563766aabee4ff31bcef40f64fdde7ecd465c14';
