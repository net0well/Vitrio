import 'package:flutter_test/flutter_test.dart';
import 'package:vitrio/features/orders/domain/city_time.dart';

void main() {
  test('keeps the wall clock of the order city whatever the device zone', () {
    expect(parseCityTime('2026-11-10T13:00:00-03:00'), DateTime(2026, 11, 10, 13));
    expect(parseCityTime('2026-11-10T13:00:00.1234567-03:00'), DateTime(2026, 11, 10, 13, 0, 0, 123, 456));
  });

  test('reads the offset of the order city', () {
    expect(parseCityOffset('2026-11-10T13:00:00-03:00'), const Duration(hours: -3));
    expect(parseCityOffset('2026-11-10T13:00:00+05:30'), const Duration(hours: 5, minutes: 30));
    expect(parseCityOffset('2026-11-10T13:00:00Z'), Duration.zero);
  });

  test('now in the order city comes from the instant, not from the device zone', () {
    expect(cityNow(DateTime.utc(2026, 11, 10, 2, 30), const Duration(hours: -3)), DateTime(2026, 11, 9, 23, 30));
  });
}
