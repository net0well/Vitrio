import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:vitrio/app/app.dart';
import 'package:vitrio/app/app_config.dart';
import 'package:vitrio/app/flavor.dart';
import 'package:vitrio/app/router.dart';
import 'package:vitrio/core/network/network_providers.dart';
import 'package:vitrio/core/presentation/clock.dart';
import 'package:vitrio/core/storage/preferences.dart';
import 'package:vitrio/features/account/data/address_providers.dart';
import 'package:vitrio/features/auth/data/auth_providers.dart';
import 'package:vitrio/features/favorites/data/favorites_providers.dart';
import 'package:vitrio/features/location/data/location_providers.dart';
import 'package:vitrio/features/orders/data/orders_providers.dart';
import 'package:vitrio/features/provider/data/provider_providers.dart';
import 'package:vitrio/features/search/data/search_providers.dart';

import 'fake_account_repository.dart';
import 'fake_address_repository.dart';
import 'fake_auth_repository.dart';
import 'fake_cities_repository.dart';
import 'fake_device_location.dart';
import 'fake_favorites_repository.dart';
import 'fake_orders_repository.dart';
import 'fake_photo_picker.dart';
import 'fake_photo_storage.dart';
import 'fake_provider_repository.dart';
import 'fake_search_repository.dart';
import 'memory_token_store.dart';

final class VitrioHarness {
  VitrioHarness({
    required this.container,
    required this.tokens,
    required this.auth,
    required this.account,
    required this.device,
    required this.cities,
    required this.provider,
    required this.photos,
    required this.search,
    required this.favorites,
    required this.orders,
    required this.addresses,
  });

  final ProviderContainer container;
  final MemoryTokenStore tokens;
  final FakeAuthRepository auth;
  final FakeAccountRepository account;
  final FakeDeviceLocation device;
  final FakeCitiesRepository cities;
  final FakeProviderRepository provider;
  final FakePhotoStorage photos;
  final FakeSearchRepository search;
  final FakeFavoritesRepository favorites;
  final FakeOrdersRepository orders;
  final FakeAddressRepository addresses;

  void go(String location) => container.read(routerProvider).go(location);
}

Future<VitrioHarness> pumpVitrio(
  WidgetTester tester, {
  Map<String, Object> preferences = const {'welcome_seen': true},
  FakeAuthRepository? auth,
  FakeAccountRepository? account,
  MemoryTokenStore? tokens,
  FakeDeviceLocation? device,
  FakeCitiesRepository? cities,
  FakeProviderRepository? provider,
  FakeSearchRepository? search,
  FakeFavoritesRepository? favorites,
  FakeOrdersRepository? orders,
  FakeAddressRepository? addresses,
  DateTime? now,
  bool settle = true,
}) async {
  SharedPreferences.setMockInitialValues(preferences);
  final instance = await SharedPreferences.getInstance();
  final harnessTokens = tokens ?? MemoryTokenStore();
  final harnessAuth = auth ?? FakeAuthRepository();
  final harnessAccount = account ?? FakeAccountRepository();
  final harnessDevice = device ?? FakeDeviceLocation();
  final harnessCities = cities ?? FakeCitiesRepository();
  final harnessProvider = provider ?? FakeProviderRepository();
  final harnessPhotos = FakePhotoStorage();
  final harnessSearch = search ?? FakeSearchRepository();
  final harnessFavorites = favorites ?? FakeFavoritesRepository();
  final harnessOrders = orders ?? FakeOrdersRepository();
  final harnessAddresses = addresses ?? FakeAddressRepository();
  final container = ProviderContainer(
    overrides: [
      appConfigProvider.overrideWithValue(AppConfig.forFlavor(Flavor.dev)),
      preferencesProvider.overrideWithValue(instance),
      tokenStoreProvider.overrideWithValue(harnessTokens),
      authRepositoryProvider.overrideWithValue(harnessAuth),
      accountRepositoryProvider.overrideWithValue(harnessAccount),
      deviceLocationProvider.overrideWithValue(harnessDevice),
      citiesRepositoryProvider.overrideWithValue(harnessCities),
      providerRepositoryProvider.overrideWithValue(harnessProvider),
      photoStorageProvider.overrideWithValue(harnessPhotos),
      photoPickerProvider.overrideWithValue(FakePhotoPicker()),
      searchRepositoryProvider.overrideWithValue(harnessSearch),
      favoritesRepositoryProvider.overrideWithValue(harnessFavorites),
      ordersRepositoryProvider.overrideWithValue(harnessOrders),
      addressRepositoryProvider.overrideWithValue(harnessAddresses),
      if (now != null) clockProvider.overrideWithValue(() => now),
    ],
  );
  addTearDown(container.dispose);

  await tester.pumpWidget(UncontrolledProviderScope(container: container, child: const VitrioApp()));
  if (settle) {
    await tester.pumpAndSettle();
  }

  return VitrioHarness(
    container: container,
    tokens: harnessTokens,
    auth: harnessAuth,
    account: harnessAccount,
    device: harnessDevice,
    cities: harnessCities,
    provider: harnessProvider,
    photos: harnessPhotos,
    search: harnessSearch,
    favorites: harnessFavorites,
    orders: harnessOrders,
    addresses: harnessAddresses,
  );
}
