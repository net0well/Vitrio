import 'package:intl/intl.dart';

final _money = NumberFormat.currency(locale: 'pt_BR', symbol: r'R$');
final _wholeMoney = NumberFormat.currency(
  locale: 'pt_BR',
  symbol: r'R$',
  decimalDigits: 0,
);
final _oneDecimal = NumberFormat('0.0', 'pt_BR');
final _whole = NumberFormat('0', 'pt_BR');

String formatMoney(double value) => value == value.roundToDouble() ? _wholeMoney.format(value) : _money.format(value);

String formatDistance(double km) => switch (km) {
      < 1 => '< 1',
      < 10 => _oneDecimal.format(km),
      _ => _whole.format(km),
    };

String formatRating(double score) => _oneDecimal.format(score);

String phoneText(String? phone) {
  var digits = (phone ?? '').replaceAll(RegExp(r'\D'), '');
  if (digits.length > _phoneDigits && digits.startsWith(_brazilCode)) {
    digits = digits.substring(_brazilCode.length);
  }

  return _masked(digits, _phoneDigits, (index) => switch (index) { 0 => '(', 2 => ') ', 7 => '-', _ => '' });
}

String cnpjText(String? cnpj) => _masked(cnpj, 14, (index) => switch (index) { 2 || 5 => '.', 8 => '/', 12 => '-', _ => '' });

String cepText(String? cep) => _masked(cep, 8, (index) => index == 5 ? '-' : '');

const _phoneDigits = 11;
const _brazilCode = '55';

String _masked(String? input, int length, String Function(int index) separatorBefore) {
  final digits = (input ?? '').replaceAll(RegExp(r'\D'), '');
  final text = StringBuffer();
  for (var index = 0; index < digits.length && index < length; index++) {
    text
      ..write(separatorBefore(index))
      ..write(digits[index]);
  }

  return text.toString();
}
