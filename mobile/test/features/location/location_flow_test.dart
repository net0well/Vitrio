import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:vitrio/core/network/problem_details.dart';
import 'package:vitrio/features/location/domain/device_permission.dart';
import 'package:vitrio/features/location/domain/device_position.dart';

import '../../support/fake_cities_repository.dart';
import '../../support/fake_device_location.dart';
import '../../support/pump_vitrio.dart';

void main() {
  const nearMe = {'welcome_seen': true, 'location_mode': 'nearMe'};

  Future<void> chooseInPicker(WidgetTester tester, String term, String city) async {
    await tester.enterText(find.descendant(of: find.byType(BottomSheet), matching: find.byType(TextField)), term);
    await tester.pump(const Duration(milliseconds: 400));
    await tester.pumpAndSettle();
    await tester.tap(find.text(city));
    await tester.pumpAndSettle();
  }

  testWidgets('without permission the person chooses a city and the home shows it', (tester) async {
    final harness = await pumpVitrio(tester);
    harness.go('/home');
    await tester.pumpAndSettle();
    expect(find.text('Para mostrar quem atende perto de você'), findsOneWidget);

    await tester.tap(find.widgetWithText(FilledButton, 'Usar minha localização'));
    await tester.pumpAndSettle();
    expect(find.text('Sem acesso à localização. Escolha a cidade.'), findsOneWidget);

    await chooseInPicker(tester, 'jab', 'Jaboticabal · SP');

    expect(find.text('Mostrando profissionais de Jaboticabal, SP.'), findsOneWidget);
    expect(harness.device.settingsOpened, 0);
  });

  testWidgets('near me shows the neighborhood and a chosen city wins until near me again', (tester) async {
    final device = FakeDeviceLocation()..current = DevicePermission.granted;
    final harness = await pumpVitrio(tester, preferences: nearMe, device: device);
    harness.go('/home');
    await tester.pumpAndSettle();
    expect(find.text('Centro, Jaboticabal'), findsOneWidget);
    expect(find.textContaining('Mostrando profissionais'), findsNothing);

    await tester.tap(find.text('Centro, Jaboticabal'));
    await tester.pumpAndSettle();
    await chooseInPicker(tester, 'paulo', 'São Paulo · SP');
    expect(find.text('Mostrando profissionais de São Paulo, SP.'), findsOneWidget);

    await tester.tap(find.text('São Paulo'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('Usar minha localização'));
    await tester.pumpAndSettle();

    expect(find.text('Centro, Jaboticabal'), findsOneWidget);
    expect(find.textContaining('Mostrando profissionais'), findsNothing);
  });

  testWidgets('while near me is still locating the home shows progress instead of asking for a city', (tester) async {
    final device = FakeDeviceLocation()
      ..current = DevicePermission.granted
      ..positionGate = Completer<void>();
    final harness = await pumpVitrio(tester, preferences: nearMe, device: device, settle: false);
    harness.go('/home');
    await tester.pump();
    await tester.pump();

    expect(find.byType(CircularProgressIndicator), findsOneWidget);
    expect(find.widgetWithText(OutlinedButton, 'Escolher cidade'), findsNothing);

    device.positionGate!.complete();
    await tester.pumpAndSettle();
    expect(find.text('Centro, Jaboticabal'), findsOneWidget);
  });

  testWidgets('an imprecise position shows only the city', (tester) async {
    final device = FakeDeviceLocation()
      ..current = DevicePermission.granted
      ..position = const DevicePosition(latitude: -21.255, longitude: -48.322, accuracyMeters: 2500);
    final harness = await pumpVitrio(tester, preferences: nearMe, device: device);
    harness.go('/home');
    await tester.pumpAndSettle();

    expect(find.text('Jaboticabal'), findsOneWidget);
    expect(find.text('Centro, Jaboticabal'), findsNothing);
  });

  testWidgets('chosen cities show up as recent in the picker', (tester) async {
    final harness = await pumpVitrio(tester);
    harness.go('/home');
    await tester.pumpAndSettle();
    await tester.tap(find.widgetWithText(OutlinedButton, 'Escolher cidade'));
    await tester.pumpAndSettle();
    await chooseInPicker(tester, 'paulo', 'São Paulo · SP');

    await tester.tap(find.text('São Paulo'));
    await tester.pumpAndSettle();

    expect(find.text('Recentes'), findsOneWidget);
    expect(find.text('São Paulo · SP'), findsOneWidget);
  });

  testWidgets('a search without connection says so instead of showing no cities', (tester) async {
    final cities = FakeCitiesRepository()..searchFailure = ProblemDetails.network();
    final harness = await pumpVitrio(tester, cities: cities);
    harness.go('/home');
    await tester.pumpAndSettle();
    await tester.tap(find.widgetWithText(OutlinedButton, 'Escolher cidade'));
    await tester.pumpAndSettle();

    await tester.enterText(find.byType(TextField), 'jab');
    await tester.pump(const Duration(milliseconds: 400));
    await tester.pumpAndSettle();

    expect(find.text('Sem conexão. Confira a internet e tente de novo.'), findsOneWidget);
    expect(find.text('Nenhuma cidade encontrada.'), findsNothing);
  });

  testWidgets('a permission denied forever opens the system settings', (tester) async {
    final device = FakeDeviceLocation()..current = DevicePermission.deniedForever;
    final harness = await pumpVitrio(tester, device: device);
    harness.go('/home');
    await tester.pumpAndSettle();

    await tester.tap(find.widgetWithText(FilledButton, 'Usar minha localização'));
    await tester.pumpAndSettle();

    expect(device.settingsOpened, 1);
  });
}
