import 'package:flutter/services.dart';

import 'provider_labels.dart';

class CnpjInputFormatter extends TextInputFormatter {
  const CnpjInputFormatter();

  @override
  TextEditingValue formatEditUpdate(TextEditingValue oldValue, TextEditingValue newValue) {
    final text = cnpjText(newValue.text);
    return TextEditingValue(text: text, selection: TextSelection.collapsed(offset: text.length));
  }
}
