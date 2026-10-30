import 'package:flutter_test/flutter_test.dart';

Future<void> pumpUntil(WidgetTester tester, Finder finder, {Duration timeout = const Duration(seconds: 30), bool absent = false}) async {
  final deadline = DateTime.now().add(timeout);
  while (DateTime.now().isBefore(deadline)) {
    await tester.pump(const Duration(milliseconds: 200));
    if (finder.evaluate().isNotEmpty != absent) {
      return;
    }
  }

  throw TestFailure('Timed out waiting for $finder');
}
