import 'package:flutter_test/flutter_test.dart';
import 'package:vitrio/core/design_system/widgets/vitrio_option.dart';
import 'package:vitrio/core/design_system/widgets/vitrio_select_field.dart';

import '../../support/pump_design_system.dart';

void main() {
  const durations = [VitrioOption(value: 60, label: '1 h'), VitrioOption(value: 90, label: '1 h 30'), VitrioOption(value: 120, label: '2 h')];

  testWidgets('shows the label above the chosen value', (tester) async {
    await pumpDesignSystem(tester, VitrioSelectField<int>(label: 'Duração', value: 90, options: durations, onChanged: (_) {}));

    expect(tester.getRect(find.text('Duração')).bottom, lessThan(tester.getRect(find.text('1 h 30')).top));
  });

  testWidgets('choosing an option calls back with its value', (tester) async {
    int? chosen;
    await pumpDesignSystem(tester, VitrioSelectField<int>(label: 'Duração', value: 60, options: durations, onChanged: (value) => chosen = value));

    await tester.tap(find.text('1 h'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('2 h').last);
    await tester.pumpAndSettle();

    expect(chosen, 120);
  });
}
