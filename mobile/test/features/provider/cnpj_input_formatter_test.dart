import 'package:flutter_test/flutter_test.dart';
import 'package:vitrio/features/provider/presentation/cnpj_input_formatter.dart';
import 'package:vitrio/features/provider/presentation/provider_labels.dart';

void main() {
  String typed(String text) => const CnpjInputFormatter().formatEditUpdate(TextEditingValue.empty, TextEditingValue(text: text)).text;

  test('a stored cnpj shows with the mask', () {
    expect(cnpjText('11222333000181'), '11.222.333/0001-81');
    expect(cnpjText(null), '');
  });

  test('typing a cnpj fills the mask as the digits come', () {
    expect(typed('112'), '11.2');
    expect(typed('11222333'), '11.222.333');
    expect(typed('112223330001'), '11.222.333/0001');
    expect(typed('11.222.333/0001-81'), '11.222.333/0001-81');
    expect(typed('112223330001819'), '11.222.333/0001-81');
  });
}
