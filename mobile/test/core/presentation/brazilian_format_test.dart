import 'package:flutter_test/flutter_test.dart';
import 'package:vitrio/core/presentation/brazilian_format.dart';

void main() {
  test('distances below one kilometer show as less than one', () => expect(formatDistance(0.2), '< 1'));

  test('distances up to ten kilometers keep one decimal', () => expect(formatDistance(6.2), '6,2'));

  test('distances from ten kilometers are whole numbers', () => expect(formatDistance(12), '12'));

  test('whole amounts drop the cents and broken amounts keep them', () {
    expect(formatMoney(80), 'R\$ 80');
    expect(formatMoney(80.5), 'R\$ 80,50');
  });

  test('ratings show one decimal with a comma', () => expect(formatRating(4.9), '4,9'));
}
