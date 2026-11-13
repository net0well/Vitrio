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
    expect(phoneText('+5516988887777'), '(16) 98888-7777');
    expect(phoneText('16988887777'), '(16) 98888-7777');
  });

  test('phones are masked while typed', () {
    expect(phoneText('1'), '(1');
    expect(phoneText('16'), '(16');
    expect(phoneText('169'), '(16) 9');
    expect(phoneText('1699814'), '(16) 99814');
    expect(phoneText('16998147'), '(16) 99814-7');
    expect(phoneText('169981471439'), '(16) 99814-7143');
    expect(phoneText(null), '');
  });

  test('ceps get the hyphen', () => expect(cepText('14870180'), '14870-180'));
}
