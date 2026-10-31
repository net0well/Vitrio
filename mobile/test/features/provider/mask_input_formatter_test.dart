import 'package:flutter_test/flutter_test.dart';
import 'package:vitrio/features/provider/presentation/mask_input_formatter.dart';
import 'package:vitrio/features/provider/presentation/provider_labels.dart';

void main() {
  String typed(String Function(String) mask, String text) =>
      MaskInputFormatter(mask).formatEditUpdate(TextEditingValue.empty, TextEditingValue(text: text)).text;

  test('a stored cnpj and cep show with the mask', () {
    expect(cnpjText('11222333000181'), '11.222.333/0001-81');
    expect(cepText('14870010'), '14870-010');
    expect(cnpjText(null), '');
  });

  test('typing a cnpj fills the mask as the digits come', () {
    expect(typed(cnpjText, '112'), '11.2');
    expect(typed(cnpjText, '11222333'), '11.222.333');
    expect(typed(cnpjText, '112223330001'), '11.222.333/0001');
    expect(typed(cnpjText, '11.222.333/0001-81'), '11.222.333/0001-81');
    expect(typed(cnpjText, '112223330001819'), '11.222.333/0001-81');
  });

  test('typing a cep adds the hyphen after five digits', () {
    expect(typed(cepText, '14870'), '14870');
    expect(typed(cepText, '148700'), '14870-0');
    expect(typed(cepText, '14870-0109'), '14870-010');
  });
}
