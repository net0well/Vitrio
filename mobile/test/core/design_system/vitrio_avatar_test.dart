import 'package:flutter_test/flutter_test.dart';
import 'package:vitrio/core/design_system/widgets/vitrio_avatar.dart';

import '../../support/pump_design_system.dart';

void main() {
  Future<void> expectInitials(WidgetTester tester, String name, String initials) async {
    await pumpDesignSystem(tester, VitrioAvatar(name: name));

    expect(find.text(initials), findsOneWidget);
  }

  testWidgets('uses the first name and the first surname', (tester) => expectInitials(tester, 'Ana Lúcia Prado', 'AL'));

  testWidgets('skips surname particles', (tester) => expectInitials(tester, 'Maria da Silva Souza', 'MS'));

  testWidgets('a single name gives one letter', (tester) => expectInitials(tester, 'Bruno', 'B'));

  testWidgets('explicit initials win over the name', (tester) async {
    await pumpDesignSystem(tester, const VitrioAvatar(name: 'Ana Lúcia Prado', initials: 'AP'));

    expect(find.text('AP'), findsOneWidget);
  });
}
