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

String formatDistance(double km) => km < 10 ? _oneDecimal.format(km) : _whole.format(km);

String formatRating(double score) => _oneDecimal.format(score);
