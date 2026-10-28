import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:vitrio/app/app.dart';
import 'package:vitrio/app/app_config.dart';
import 'package:vitrio/app/flavor.dart';
import 'package:vitrio/core/storage/preferences.dart';

void main() {
  Future<SharedPreferences> pumpApp(WidgetTester tester, {Map<String, Object> preferences = const {}}) async {
    SharedPreferences.setMockInitialValues(preferences);
    final instance = await SharedPreferences.getInstance();
    await tester.pumpWidget(
      ProviderScope(
        overrides: [
          appConfigProvider.overrideWithValue(AppConfig.forFlavor(Flavor.dev)),
          preferencesProvider.overrideWithValue(instance),
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
    expect(find.byType(NavigationBar), findsNothing);
  });

  testWidgets('finding a professional opens the home tab and remembers the welcome', (tester) async {
    final preferences = await pumpApp(tester);

    await tester.tap(find.text('Encontrar um profissional'));
    await tester.pumpAndSettle();

    expect(find.byType(NavigationBar), findsOneWidget);
    expect(find.text('Perto de você'), findsOneWidget);
    expect(preferences.getBool('welcome_seen'), isTrue);
  });

  testWidgets('offering services opens the account tab', (tester) async {
    await pumpApp(tester);

    await tester.tap(find.text('Quero oferecer meus serviços'));
    await tester.pumpAndSettle();

    expect(find.byType(NavigationBar), findsOneWidget);
    expect(find.textContaining('entrar com Google ou Apple'), findsOneWidget);
  });

  testWidgets('returning users start on the home tab', (tester) async {
    await pumpApp(tester, preferences: {'welcome_seen': true});

    expect(find.text('Perto de você'), findsOneWidget);
  });

  testWidgets('the empty orders tab leads to search', (tester) async {
    await pumpApp(tester, preferences: {'welcome_seen': true});

    await tester.tap(find.text('Pedidos'));
    await tester.pumpAndSettle();
    expect(find.text('Você ainda não fez nenhum pedido. Que tal começar pela busca?'), findsOneWidget);

    await tester.tap(find.text('Buscar profissionais'));
    await tester.pumpAndSettle();
    expect(find.textContaining('buscar por serviço'), findsOneWidget);
  });
}
