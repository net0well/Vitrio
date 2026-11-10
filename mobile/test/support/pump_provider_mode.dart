import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:vitrio/core/network/api_result.dart';
import 'package:vitrio/features/provider/domain/provider_status.dart';

import 'fake_account_repository.dart';
import 'fake_provider_orders_repository.dart';
import 'fake_provider_repository.dart';
import 'memory_token_store.dart';
import 'pump_vitrio.dart';
import 'sample_account.dart';
import 'sample_order.dart';
import 'sample_provider.dart';

FakeProviderRepository publishedProvider() => FakeProviderRepository()
  ..seedComplete()
  ..status = ProviderStatus.published;

Future<VitrioHarness> pumpProviderMode(WidgetTester tester, {FakeProviderOrdersRepository? orders, FakeProviderRepository? provider}) {
  tester.view.physicalSize = const Size(1080, 2400);
  tester.view.devicePixelRatio = 2.6;
  addTearDown(tester.view.reset);
  return pumpVitrio(
    tester,
    preferences: const {'welcome_seen': true, 'location_mode': 'nearMe', 'app_mode': 'provider'},
    account: FakeAccountRepository()..meResult = const ApiSuccess(sampleProviderMe),
    provider: provider ?? publishedProvider(),
    tokens: MemoryTokenStore()..tokens = sampleSession.tokens,
    providerOrders: orders,
    now: sampleNow,
  );
}
