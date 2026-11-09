import 'package:flutter_test/flutter_test.dart';
import 'package:vitrio/core/design_system/widgets/vitrio_option.dart';
import 'package:vitrio/core/design_system/widgets/vitrio_segmented.dart';

import '../../support/pump_design_system.dart';

void main() {
  const options = [
    VitrioOption(value: 'new', label: 'Novos · 2'),
    VitrioOption(value: 'ongoing', label: 'Em andamento'),
    VitrioOption(value: 'closed', label: 'Finalizados'),
  ];

  testWidgets('tapping another option reports its value', (tester) async {
    String? picked;
    await pumpDesignSystem(
      tester,
      VitrioSegmented(label: 'Pedidos', options: options, value: 'new', onChanged: (value) => picked = value),
    );

    await tester.tap(find.text('Finalizados'));

    expect(picked, 'closed');
  });

  testWidgets('the chosen option is announced as selected', (tester) async {
    await pumpDesignSystem(
      tester,
      VitrioSegmented(label: 'Pedidos', options: options, value: 'ongoing', onChanged: (_) {}),
    );

    expect(tester.getSemantics(find.text('Em andamento')), isSemantics(isSelected: true, isButton: true));
    expect(tester.getSemantics(find.text('Novos · 2')), isSemantics(isSelected: false));
  });
}
