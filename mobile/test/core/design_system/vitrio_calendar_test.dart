import 'package:flutter/services.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:vitrio/core/design_system/widgets/vitrio_calendar.dart';
import 'package:vitrio/core/design_system/widgets/vitrio_calendar_labels.dart';
import 'package:vitrio/core/design_system/widgets/vitrio_calendar_mark.dart';
import 'package:vitrio/core/design_system/widgets/vitrio_icon_name.dart';

import '../../support/pump_design_system.dart';

void main() {
  final labels = VitrioCalendarLabels(
    month: 'Outubro 2026',
    weekdays: const ['dom', 'seg', 'ter', 'qua', 'qui', 'sex', 'sáb'],
    previousMonth: 'Mês anterior',
    nextMonth: 'Próximo mês',
    scheduled: 'Agendado',
    quoted: 'Aguardando o cliente',
    done: 'Feito',
    day: (day, items) => '${day.day} de outubro, $items itens',
  );

  Future<void> pumpOctober(
    WidgetTester tester, {
    ValueChanged<DateTime>? onSelect,
    VoidCallback? onPrevious,
    VoidCallback? onNext,
  }) =>
      pumpDesignSystem(
        tester,
        VitrioCalendar(
          month: DateTime(2026, 10),
          today: DateTime(2026, 10, 28),
          selected: DateTime(2026, 10, 29),
          marks: {
            DateTime(2026, 10, 29): const [VitrioCalendarMark.scheduled, VitrioCalendarMark.scheduled],
            DateTime(2026, 10, 30): const [VitrioCalendarMark.quoted],
          },
          labels: labels,
          onSelect: onSelect ?? (_) {},
          onPrevious: onPrevious,
          onNext: onNext,
        ),
      );

  testWidgets('shows the month, the week days, every day and the legend', (tester) async {
    await pumpOctober(tester);

    expect(find.text('Outubro 2026'), findsOneWidget);
    expect(find.text('qui'), findsOneWidget);
    expect(find.text('31'), findsOneWidget);
    expect(find.text('Agendado'), findsOneWidget);
    expect(find.text('Aguardando o cliente'), findsOneWidget);
    expect(find.text('Feito'), findsOneWidget);
  });

  testWidgets('each day announces how many items it has and the chosen one is selected', (tester) async {
    await pumpOctober(tester);

    expect(find.bySemanticsLabel('29 de outubro, 2 itens'), findsOneWidget);
    expect(tester.getSemantics(find.bySemanticsLabel('29 de outubro, 2 itens')), isSemantics(isSelected: true));
    expect(tester.getSemantics(find.bySemanticsLabel('30 de outubro, 1 itens')), isSemantics(isSelected: false));
  });

  testWidgets('tapping a day and the arrows call back', (tester) async {
    DateTime? picked;
    var moves = 0;
    await pumpOctober(tester, onSelect: (day) => picked = day, onPrevious: () => moves--, onNext: () => moves += 10);

    await tester.tap(find.bySemanticsLabel('30 de outubro, 1 itens'));
    await tester.tap(find.byTooltip('Próximo mês'));

    expect(picked, DateTime(2026, 10, 30));
    expect(moves, 10);
  });

  testWidgets('the phone icon ships with the app', (tester) async {
    final icon = await rootBundle.loadString(VitrioIconName.phone.assetPath);

    expect(icon, contains('<svg'));
  });
}
