import 'package:flutter/widgets.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:vitrio/core/design_system/tokens/vitrio_colors.dart';
import 'package:vitrio/core/design_system/widgets/vitrio_icon.dart';
import 'package:vitrio/core/design_system/widgets/vitrio_icon_name.dart';
import 'package:vitrio/core/design_system/widgets/vitrio_order_timeline.dart';
import 'package:vitrio/core/design_system/widgets/vitrio_timeline_step.dart';
import 'package:vitrio/core/design_system/widgets/vitrio_timeline_step_state.dart';

import '../../support/pump_design_system.dart';

void main() {
  const steps = [
    VitrioTimelineStep(title: 'Pedido enviado', subtitle: 'Hoje, 09:12', state: VitrioTimelineStepState.done),
    VitrioTimelineStep(title: 'Orçamento recebido', subtitle: 'Hoje, 09:30', state: VitrioTimelineStepState.done),
    VitrioTimelineStep(
      title: 'Serviço agendado',
      subtitle: 'Aguardando sua resposta',
      state: VitrioTimelineStepState.current,
      stateLabel: 'Etapa atual',
    ),
    VitrioTimelineStep(title: 'Serviço concluído', state: VitrioTimelineStepState.pending),
  ];

  testWidgets('shows every step with a check only on the finished ones', (tester) async {
    await pumpDesignSystem(tester, const VitrioOrderTimeline(steps: steps));

    for (final step in steps) {
      expect(find.text(step.title), findsOneWidget);
    }
    expect(find.text('Aguardando sua resposta'), findsOneWidget);
    expect(find.byWidgetPredicate((widget) => widget is VitrioIcon && widget.icon == VitrioIconName.check), findsNWidgets(2));
  });

  testWidgets('pending steps are muted and the current one announces its state', (tester) async {
    await pumpDesignSystem(tester, const VitrioOrderTimeline(steps: steps));

    final pending = tester.widget<Text>(find.text('Serviço concluído'));
    expect(pending.style?.color, VitrioColors.light.inkMuted);
    expect(tester.getSemantics(find.text('Serviço agendado')), isSemantics(value: 'Etapa atual'));
  });
}
