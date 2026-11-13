import 'package:flutter/widgets.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:vitrio/core/network/api_result.dart';
import 'package:vitrio/features/location/domain/device_permission.dart';
import 'package:vitrio/features/provider/domain/provider_status.dart';

import '../../support/fake_account_repository.dart';
import '../../support/fake_device_location.dart';
import '../../support/fake_provider_repository.dart';
import '../../support/fake_search_repository.dart';
import '../../support/memory_token_store.dart';
import '../../support/pump_vitrio.dart';
import '../../support/sample_account.dart';
import '../../support/sample_listing.dart';
import '../../support/sample_provider.dart';

void main() {
  const signedInClient = {'welcome_seen': true, 'location_mode': 'nearMe'};

  Future<VitrioHarness> asClientWithShowcase(WidgetTester tester, {FakeSearchRepository? search}) {
    tester.view.physicalSize = const Size(1080, 2400);
    tester.view.devicePixelRatio = 2.6;
    addTearDown(tester.view.reset);
    return pumpVitrio(
      tester,
      preferences: signedInClient,
      device: FakeDeviceLocation()..current = DevicePermission.granted,
      account: FakeAccountRepository()..meResult = const ApiSuccess(sampleProviderMe),
      tokens: MemoryTokenStore()..tokens = sampleSession.tokens,
      provider: FakeProviderRepository()
        ..seedComplete()
        ..status = ProviderStatus.published
        ..published = samplePublicProvider,
      search: search,
    );
  }

  testWidgets('the own showcase is marked and does not offer a quote to oneself', (tester) async {
    final harness = await asClientWithShowcase(tester);

    harness.go('/providers/${samplePublicProvider.id}');
    await tester.pumpAndSettle();

    expect(find.text('Você'), findsOneWidget);
    expect(find.text('Esta é a sua vitrine. Você não pode pedir orçamento para você mesmo.'), findsOneWidget);
    expect(find.text('Editar minha vitrine'), findsOneWidget);
    expect(find.text('Pedir orçamento'), findsNothing);
    expect(find.bySemanticsLabel('Salvar nos favoritos'), findsNothing);
  });

  testWidgets('the preview of the own showcase shows it the way customers see it', (tester) async {
    final harness = await asClientWithShowcase(tester);

    harness.go('/providers/${samplePublicProvider.id}?preview=true');
    await tester.pumpAndSettle();

    expect(find.text('Você'), findsNothing);
    expect(find.text('Pedir orçamento'), findsOneWidget);
    expect(find.text('WhatsApp'), findsOneWidget);
  });

  testWidgets('search marks the own showcase among the results', (tester) async {
    final search = FakeSearchRepository()
      ..listings = [
        sampleListing(id: samplePublicProvider.id, displayName: 'Carlos Eletricista'),
        sampleListing(id: 'other', displayName: 'Sandra Lopes'),
      ];
    final harness = await asClientWithShowcase(tester, search: search);

    harness.go('/search');
    await tester.pumpAndSettle();

    expect(find.text('Você'), findsOneWidget);
  });

  testWidgets('requesting a quote from the own showcase explains it is not possible', (tester) async {
    final harness = await asClientWithShowcase(tester);

    harness.go('/request-quote/${samplePublicProvider.id}');
    await tester.pumpAndSettle();

    expect(find.text('Esta é a sua vitrine. Você não pode pedir orçamento para você mesmo.'), findsOneWidget);
    expect(find.text('Enviar pedido'), findsNothing);
  });
}
