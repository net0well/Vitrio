import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:vitrio/core/design_system/widgets/vitrio_chip.dart';
import 'package:vitrio/core/network/api_result.dart';
import 'package:vitrio/core/network/problem_details.dart';
import 'package:vitrio/core/presentation/brazilian_format.dart';
import 'package:vitrio/features/location/domain/device_permission.dart';
import 'package:vitrio/features/search/domain/professional_name.dart';
import 'package:vitrio/features/search/domain/search_sort.dart';

import '../../support/fake_account_repository.dart';
import '../../support/fake_device_location.dart';
import '../../support/fake_favorites_repository.dart';
import '../../support/fake_provider_repository.dart';
import '../../support/fake_search_repository.dart';
import '../../support/memory_token_store.dart';
import '../../support/pump_vitrio.dart';
import '../../support/sample_account.dart';
import '../../support/sample_listing.dart';
import '../../support/sample_provider.dart';

void main() {
  const nearMe = {'welcome_seen': true, 'location_mode': 'nearMe'};
  const electricians = ProfessionalName(singular: 'eletricista', plural: 'eletricistas');

  FakeDeviceLocation gps() => FakeDeviceLocation()..current = DevicePermission.granted;

  void phone(WidgetTester tester) {
    tester.view.physicalSize = const Size(1080, 2400);
    tester.view.devicePixelRatio = 2.6;
    addTearDown(tester.view.reset);
  }

  Future<void> reveal(WidgetTester tester, Finder finder) => tester.scrollUntilVisible(finder, 200, scrollable: find.byType(Scrollable).first);

  final chipRow = find
      .descendant(
        of: find.byWidgetPredicate((widget) => widget is ListView && widget.scrollDirection == Axis.horizontal),
        matching: find.byType(Scrollable),
      )
      .first;

  Future<Finder> chip(WidgetTester tester, String label) async {
    final finder = find.widgetWithText(VitrioChip, label);
    await tester.drag(chipRow, const Offset(2000, 0));
    await tester.pumpAndSettle();
    await tester.scrollUntilVisible(finder, 80, scrollable: chipRow);
    await tester.ensureVisible(finder);
    await tester.pumpAndSettle();
    return finder;
  }

  Future<void> tapChip(WidgetTester tester, String label) async {
    await tester.tap(await chip(tester, label));
    await tester.pumpAndSettle();
  }

  Future<bool> chipSelected(WidgetTester tester, String label) async => tester.widget<VitrioChip>(await chip(tester, label)).selected;

  testWidgets('home shows the top rated providers nearby', (tester) async {
    phone(tester);
    final search = FakeSearchRepository()..topRated = [sampleListing()];
    final harness = await pumpVitrio(tester, preferences: nearMe, device: gps(), search: search);
    harness.go('/home');
    await tester.pumpAndSettle();
    await reveal(tester, find.text('Carlos Menezes'));

    expect(find.text('Bem avaliados perto de você'), findsOneWidget);
    expect(find.text('Carlos Menezes'), findsOneWidget);
    expect(find.text('4,9'), findsOneWidget);
    expect(find.text('(127) · 1,2 km · Jaboticabal'), findsOneWidget);
  });

  testWidgets('home invites a visitor without an account to sign in', (tester) async {
    phone(tester);
    final harness = await pumpVitrio(tester, preferences: nearMe, device: gps());
    harness.go('/home');
    await tester.pumpAndSettle();

    expect(find.text('O Vitrio funciona melhor com sua conta'), findsOneWidget);
    expect(find.text('Criar conta'), findsOneWidget);

    await tester.tap(find.widgetWithText(FilledButton, 'Entrar'));
    await tester.pumpAndSettle();

    expect(find.text('Esqueci a senha'), findsOneWidget);
  });

  testWidgets('home does not invite to sign in when signed in', (tester) async {
    phone(tester);
    final harness = await pumpVitrio(
      tester,
      preferences: nearMe,
      device: gps(),
      account: FakeAccountRepository()..meResult = const ApiSuccess(sampleMe),
      tokens: MemoryTokenStore()..tokens = sampleSession.tokens,
    );
    harness.go('/home');
    await tester.pumpAndSettle();

    expect(find.text('O Vitrio funciona melhor com sua conta'), findsNothing);
  });

  testWidgets('home without rated providers hides the top rated section', (tester) async {
    phone(tester);
    final harness = await pumpVitrio(tester, preferences: nearMe, device: gps());
    harness.go('/home');
    await tester.pumpAndSettle();

    expect(find.text('Bem avaliados perto de você'), findsNothing);
    expect(find.textContaining('Ainda não tem profissionais'), findsNothing);
  });

  testWidgets('home in a city without providers invites to share the app', (tester) async {
    phone(tester);
    final search = FakeSearchRepository()..hasProviders = false;
    final harness = await pumpVitrio(tester, preferences: nearMe, device: gps(), search: search);
    harness.go('/home');
    await tester.pumpAndSettle();
    await reveal(tester, find.text('Quero oferecer meus serviços'));

    expect(find.text('Ainda não tem profissionais do Vitrio em Jaboticabal, SP.'), findsOneWidget);
    expect(find.text('Indicar o Vitrio'), findsOneWidget);
    expect(find.text('Quero oferecer meus serviços'), findsOneWidget);
  });

  testWidgets('see available opens the search with available today on', (tester) async {
    phone(tester);
    final search = FakeSearchRepository()..listings = [sampleListing()];
    final harness = await pumpVitrio(tester, preferences: nearMe, device: gps(), search: search);
    harness.go('/home');
    await tester.pumpAndSettle();

    await tester.tap(find.text('Ver disponíveis'));
    await tester.pumpAndSettle();

    expect(await chipSelected(tester, 'Atende hoje'), isTrue);
    expect(search.searches.last.$2.availableToday, isTrue);
    expect(find.text('Carlos Menezes'), findsOneWidget);
  });

  testWidgets('a category from the home filters the search and counts its professionals', (tester) async {
    phone(tester);
    final search = FakeSearchRepository()
      ..listings = [sampleListing()]
      ..count = 28
      ..professional = electricians;
    final harness = await pumpVitrio(tester, preferences: nearMe, device: gps(), search: search);
    harness.go('/home');
    await tester.pumpAndSettle();
    await reveal(tester, find.text('Elétrica'));

    await tester.tap(find.text('Elétrica'));
    await tester.pumpAndSettle();

    expect(await chipSelected(tester, 'Elétrica'), isTrue);
    expect(search.searches.last.$2.category?.id, 1);
    expect(find.text('28 eletricistas perto de você'), findsOneWidget);
  });

  testWidgets('search beyond two hundred results says more than two hundred', (tester) async {
    phone(tester);
    final search = FakeSearchRepository()
      ..listings = [sampleListing()]
      ..count = 200
      ..moreThanCount = true
      ..professional = electricians;
    final harness = await pumpVitrio(tester, preferences: nearMe, device: gps(), search: search);
    harness.go('/search');
    await tester.pumpAndSettle();

    expect(find.text('Mais de 200 eletricistas perto de você'), findsOneWidget);
  });

  testWidgets('search shows the card with price, badges and the nearby count', (tester) async {
    phone(tester);
    final search = FakeSearchRepository()..listings = [sampleListing()];
    final harness = await pumpVitrio(tester, preferences: nearMe, device: gps(), search: search);
    harness.go('/search');
    await tester.pumpAndSettle();

    expect(find.text('1 profissional perto de você'), findsOneWidget);
    expect(find.text('Relevância'), findsOneWidget);
    expect(find.text('A partir de'), findsOneWidget);
    expect(find.text(formatMoney(80)), findsOneWidget);
    expect(find.text('MEI verificado'), findsWidgets);
    expect(find.text('Ver perfil'), findsOneWidget);
  });

  testWidgets('near me and lowest price chips change the sort and replace each other', (tester) async {
    phone(tester);
    final search = FakeSearchRepository()..listings = [sampleListing()];
    final harness = await pumpVitrio(tester, preferences: nearMe, device: gps(), search: search);
    harness.go('/search');
    await tester.pumpAndSettle();

    await tapChip(tester, 'Perto de mim');
    expect(search.searches.last.$2.sort, SearchSort.distance);
    expect(find.text('Mais perto'), findsOneWidget);

    await tapChip(tester, 'Menor preço');
    expect(search.searches.last.$2.sort, SearchSort.price);
    expect(await chipSelected(tester, 'Perto de mim'), isFalse);
  });

  testWidgets('without gps the near me chip is not offered', (tester) async {
    phone(tester);
    final search = FakeSearchRepository()..listings = [sampleListing(distanceKm: null)];
    final harness = await pumpVitrio(
      tester,
      preferences: {
        'welcome_seen': true,
        'location_mode': 'chosenCity',
        'location_chosen_city': '{"ibgeCode":3524303,"name":"Jaboticabal","state":"SP"}',
      },
      search: search,
    );
    harness.go('/search');
    await tester.pumpAndSettle();

    expect(find.text('Perto de mim'), findsNothing);
    expect(find.text('1 profissional em Jaboticabal, SP'), findsOneWidget);
    expect(search.searches.last.$1.cityIbgeCode, 3524303);
  });

  testWidgets('a search with no matches offers to clear the filters', (tester) async {
    phone(tester);
    final harness = await pumpVitrio(tester, preferences: nearMe, device: gps());
    harness.go('/search');
    await tester.pumpAndSettle();

    await tapChip(tester, 'MEI verificado');
    expect(find.text('Nenhum profissional encontrado com esses filtros.'), findsOneWidget);

    await tester.tap(find.text('Limpar filtros'));
    await tester.pumpAndSettle();
    expect(await chipSelected(tester, 'MEI verificado'), isFalse);
  });

  testWidgets('a failed search says so and tries again', (tester) async {
    phone(tester);
    final search = FakeSearchRepository()..failure = const ProblemDetails(status: 503, code: 'unavailable');
    final harness = await pumpVitrio(tester, preferences: nearMe, device: gps(), search: search);
    harness.go('/search');
    await tester.pumpAndSettle();
    expect(find.text('Não conseguimos carregar os profissionais. Verifique a conexão e tente de novo.'), findsOneWidget);

    search
      ..failure = null
      ..listings = [sampleListing()];
    await tester.tap(find.text('Tentar de novo'));
    await tester.pumpAndSettle();
    expect(find.text('Carlos Menezes'), findsOneWidget);
  });

  testWidgets('the filters sheet applies rating, badges and price together', (tester) async {
    phone(tester);
    final search = FakeSearchRepository()..listings = [sampleListing()];
    final harness = await pumpVitrio(tester, preferences: nearMe, device: gps(), search: search);
    harness.go('/search');
    await tester.pumpAndSettle();

    await tester.tap(find.byTooltip('Filtros'));
    await tester.pumpAndSettle();
    await tester.tap(find.descendant(of: find.byType(BottomSheet), matching: find.text('4+')));
    await tester.tap(find.descendant(of: find.byType(BottomSheet), matching: find.text('Até ${formatMoney(100)}')));
    await tester.pumpAndSettle();
    await tester.tap(find.text('Ver resultados'));
    await tester.pumpAndSettle();

    final filters = search.searches.last.$2;
    expect(filters.minRating, 4.0);
    expect(filters.maxPriceFrom, 100);
  });

  testWidgets('a signed in client saves a provider from the profile', (tester) async {
    phone(tester);
    final provider = FakeProviderRepository()..published = samplePublicProvider;
    final favorites = FakeFavoritesRepository();
    final harness = await pumpVitrio(
      tester,
      preferences: nearMe,
      account: FakeAccountRepository()..meResult = const ApiSuccess(sampleMe),
      tokens: MemoryTokenStore()..tokens = sampleSession.tokens,
      provider: provider,
      favorites: favorites,
    );
    harness.go('/providers/${samplePublicProvider.id}');
    await tester.pumpAndSettle();

    await tester.tap(find.byTooltip('Salvar nos favoritos'));
    await tester.pumpAndSettle();

    expect(favorites.saved, [samplePublicProvider.id]);
  });

  testWidgets('a visitor who taps the heart is asked to sign in', (tester) async {
    phone(tester);
    final provider = FakeProviderRepository()..published = samplePublicProvider;
    final favorites = FakeFavoritesRepository();
    final harness = await pumpVitrio(tester, preferences: nearMe, provider: provider, favorites: favorites);
    harness.go('/providers/${samplePublicProvider.id}');
    await tester.pumpAndSettle();

    await tester.tap(find.byTooltip('Salvar nos favoritos'));
    await tester.pumpAndSettle();

    expect(find.text('Esqueci a senha'), findsOneWidget);
    expect(favorites.saved, isEmpty);
  });

  testWidgets('favorites from the account list the saved providers', (tester) async {
    phone(tester);
    final favorites = FakeFavoritesRepository()
      ..known['listing-1'] = sampleListing()
      ..saved.add('listing-1');
    final harness = await pumpVitrio(
      tester,
      preferences: nearMe,
      account: FakeAccountRepository()..meResult = const ApiSuccess(sampleMe),
      tokens: MemoryTokenStore()..tokens = sampleSession.tokens,
      favorites: favorites,
    );
    harness.go('/account');
    await tester.pumpAndSettle();

    await tester.tap(find.text('Favoritos'));
    await tester.pumpAndSettle();

    expect(find.text('Carlos Menezes'), findsOneWidget);
  });

  testWidgets('empty favorites invite to search', (tester) async {
    phone(tester);
    final harness = await pumpVitrio(
      tester,
      preferences: nearMe,
      account: FakeAccountRepository()..meResult = const ApiSuccess(sampleMe),
      tokens: MemoryTokenStore()..tokens = sampleSession.tokens,
    );
    harness.go('/favorites');
    await tester.pumpAndSettle();

    expect(find.text('Você ainda não salvou nenhum profissional.'), findsOneWidget);
    expect(find.text('Buscar profissionais'), findsOneWidget);
  });
}
