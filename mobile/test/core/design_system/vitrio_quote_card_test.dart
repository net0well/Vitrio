import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:vitrio/core/design_system/widgets/vitrio_button.dart';
import 'package:vitrio/core/design_system/widgets/vitrio_icon_name.dart';
import 'package:vitrio/core/design_system/widgets/vitrio_quote_card.dart';
import 'package:vitrio/core/design_system/widgets/vitrio_quote_row.dart';

import '../../support/pump_design_system.dart';

void main() {
  const rows = [
    VitrioQuoteRow(icon: VitrioIconName.calendar, text: 'Visita amanhã, 14:00–15:00'),
    VitrioQuoteRow(icon: VitrioIconName.check, text: 'Material incluso (disjuntor 40 A)', positive: true),
  ];

  testWidgets('shows the value, the validity, the conditions and the message', (tester) async {
    await pumpDesignSystem(
      tester,
      const VitrioQuoteCard(
        title: 'Orçamento de Carlos',
        price: r'R$ 180,00',
        validity: 'Válido até qui 29/10, 13:00',
        rows: rows,
        message: 'Troco o disjuntor e reviso a fiação.',
      ),
    );

    expect(find.text('Orçamento de Carlos'), findsOneWidget);
    expect(find.text(r'R$ 180,00'), findsOneWidget);
    expect(find.text('Válido até qui 29/10, 13:00'), findsOneWidget);
    expect(find.text('Visita amanhã, 14:00–15:00'), findsOneWidget);
    expect(find.text('Material incluso (disjuntor 40 A)'), findsOneWidget);
    expect(find.text('Troco o disjuntor e reviso a fiação.'), findsOneWidget);
    expect(find.byType(VitrioButton), findsNothing);
  });

  testWidgets('the validity sits beside the title on a narrow phone', (tester) async {
    await pumpDesignSystem(
      tester,
      const Align(
        alignment: Alignment.topLeft,
        child: SizedBox(
          width: 320,
          child: VitrioQuoteCard(
            title: 'Orçamento de Carlos',
            price: r'R$ 180,00',
            validity: 'Válido até qui 29/10, 13:00',
            rows: rows,
          ),
        ),
      ),
    );

    final title = tester.getRect(find.text('Orçamento de Carlos'));
    final validity = tester.getRect(find.text('Válido até qui 29/10, 13:00'));

    expect(validity.left, greaterThan(title.right));
    expect(validity.center.dy, moreOrLessEquals(title.center.dy, epsilon: 1));
  });

  testWidgets('accept and decline call back', (tester) async {
    var accepted = false;
    var declined = false;
    await pumpDesignSystem(
      tester,
      VitrioQuoteCard(
        title: 'Orçamento de Carlos',
        price: r'R$ 180,00',
        rows: rows,
        acceptLabel: 'Aceitar orçamento',
        declineLabel: 'Recusar',
        onAccept: () => accepted = true,
        onDecline: () => declined = true,
      ),
    );

    await tester.tap(find.text('Recusar'));
    await tester.tap(find.text('Aceitar orçamento'));

    expect((accepted, declined), (true, true));
  });
}
