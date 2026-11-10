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

  test('phones with the country code show the area code and the hyphen', () {
    expect(formatPhone('+5516988887777'), '(16) 98888-7777');
    expect(formatPhone('1633221100'), '(16) 3322-1100');
  });

  test('phones in an unknown shape stay as typed', () => expect(formatPhone('123'), '123'));

  test('ceps get the hyphen', () => expect(formatCep('14870180'), '14870-180'));
}
