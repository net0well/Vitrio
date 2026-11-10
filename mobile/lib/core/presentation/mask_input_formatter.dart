import 'package:flutter/services.dart';

class MaskInputFormatter extends TextInputFormatter {
  const MaskInputFormatter(this.mask);

  final String Function(String text) mask;

  @override
  TextEditingValue formatEditUpdate(TextEditingValue oldValue, TextEditingValue newValue) {
    final text = mask(newValue.text);
    return TextEditingValue(text: text, selection: TextSelection.collapsed(offset: text.length));
  }
}
