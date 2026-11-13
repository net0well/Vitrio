import 'package:flutter_test/flutter_test.dart';
import 'package:vitrio/core/design_system/widgets/vitrio_choice_list.dart';
import 'package:vitrio/core/design_system/widgets/vitrio_option.dart';

import '../../support/pump_design_system.dart';

void main() {
  const reasons = [
    VitrioOption(value: 'noAvailability', label: 'Sem horário disponível'),
    VitrioOption(value: 'outsideArea', label: 'Fora da minha área'),
    VitrioOption(value: 'other', label: 'Outro'),
  ];

  testWidgets('tapping a reason reports it', (tester) async {
    String? picked;
    await pumpDesignSystem(
      tester,
      VitrioChoiceList(label: 'Motivo', options: reasons, value: null, onChanged: (value) => picked = value),
    );

    await tester.tap(find.text('Fora da minha área'));

    expect(picked, 'outsideArea');
  });

  testWidgets('only the chosen reason is announced as checked', (tester) async {
    await pumpDesignSystem(
      tester,
      VitrioChoiceList(label: 'Motivo', options: reasons, value: 'other', onChanged: (_) {}),
    );

    expect(tester.getSemantics(find.text('Outro')), isSemantics(isChecked: true, isInMutuallyExclusiveGroup: true));
    expect(tester.getSemantics(find.text('Sem horário disponível')), isSemantics(isChecked: false));
  });
}
