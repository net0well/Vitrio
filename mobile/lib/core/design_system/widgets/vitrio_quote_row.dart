import 'vitrio_icon_name.dart';

class VitrioQuoteRow {
  const VitrioQuoteRow({required this.icon, required this.text, this.positive = false});

  final VitrioIconName icon;
  final String text;
  final bool positive;
}
