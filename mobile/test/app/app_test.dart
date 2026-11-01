import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:vitrio/app/app.dart';
import 'package:vitrio/app/app_config.dart';
import 'package:vitrio/app/flavor.dart';
import 'package:vitrio/core/network/network_providers.dart';
import 'package:vitrio/core/storage/preferences.dart';
import 'package:vitrio/features/location/data/location_providers.dart';

import '../support/fake_cities_repository.dart';
import '../support/fake_device_location.dart';
import '../support/memory_token_store.dart';

void main() {
  const returning = {
    'welcome_seen': true,
    'location_mode': 'chosenCity',
    'location_chosen_city': '{"ibgeCode":3524303,"name":"Jaboticabal","state":"SP"}',
  };

  Future<SharedPreferences> pumpApp(WidgetTester tester, {Map<String, Object> preferences = const {}}) async {
    SharedPreferences.setMockInitialValues(preferences);
    final instance = await SharedPreferences.getInstance();
    await tester.pumpWidget(
      ProviderScope(
        overrides: [
          appConfigProvider.overrideWithValue(AppConfig.forFlavor(Flavor.dev)),
          preferencesProvider.overrideWithValue(instance),
          tokenStoreProvider.overrideWithValue(MemoryTokenStore()),
          deviceLocationProvider.overrideWithValue(FakeDeviceLocation()),
          citiesRepositoryProvider.overrideWithValue(FakeCitiesRepository()),
        ],
        child: const VitrioApp(),
      ),
    );
    await tester.pumpAndSettle();
    return instance;
  }

  testWidgets('first launch shows the welcome screen', (tester) async {
    await pumpApp(tester);

    expect(find.text('Os profissionais da sua cidade, numa vitrine só.'), findsOneWidget);
    expect(find.text('O Vitrio é grátis para todo mundo.'), findsOneWidget);
    expect(find.byType(NavigationBar), findsNothing);
  });

  testWidgets('finding a professional asks where to search and remembers the welcome', (tester) async {
    final preferences = await pumpApp(tester);

    await tester.ensureVisible(find.text('Encontrar um profissional'));
    await tester.tap(find.text('Encontrar um profissional'));
    await tester.pumpAndSettle();

    expect(find.text('Para mostrar quem atende perto de você'), findsOneWidget);
    expect(preferences.getBool('welcome_seen'), isTrue);
  });

  testWidgets('offering services without an account opens the sign-up', (tester) async {
    await pumpApp(tester);

    await tester.ensureVisible(find.text('Quero oferecer meus serviços'));
    await tester.tap(find.text('Quero oferecer meus serviços'));
    await tester.pumpAndSettle();

    expect(find.byType(NavigationBar), findsNothing);
    expect(find.text('Crie sua conta'), findsOneWidget);
    expect(find.text('Criar conta'), findsWidgets);
  });

  testWidgets('returning users start on the home tab of their city', (tester) async {
    await pumpApp(tester, preferences: returning);

    expect(find.text('Do que você precisa hoje?'), findsOneWidget);
    expect(find.text('Mostrando profissionais de Jaboticabal, SP.'), findsOneWidget);
  });

  testWidgets('the empty orders tab leads to search', (tester) async {
    await pumpApp(tester, preferences: returning);

    await tester.tap(find.text('Pedidos'));
    await tester.pumpAndSettle();
    expect(find.text('Você ainda não fez nenhum pedido. Que tal começar pela busca?'), findsOneWidget);

    await tester.tap(find.text('Buscar profissionais'));
    await tester.pumpAndSettle();
    expect(find.text('O que você procura?'), findsOneWidget);
  });
}
