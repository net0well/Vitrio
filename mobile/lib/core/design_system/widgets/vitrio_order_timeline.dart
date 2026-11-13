import 'package:flutter/material.dart';

import '../tokens/vitrio_colors.dart';
import '../tokens/vitrio_space.dart';
import '../tokens/vitrio_text.dart';
import 'vitrio_icon.dart';
import 'vitrio_icon_name.dart';
import 'vitrio_timeline_step.dart';
import 'vitrio_timeline_step_state.dart';

class VitrioOrderTimeline extends StatelessWidget {
  const VitrioOrderTimeline({super.key, required this.steps});

  final List<VitrioTimelineStep> steps;

  @override
  Widget build(BuildContext context) => Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          for (final (index, step) in steps.indexed) _StepRow(step: step, last: index == steps.length - 1),
        ],
      );
}

class _StepRow extends StatelessWidget {
  const _StepRow({required this.step, required this.last});

  static const _dot = 28.0;

  final VitrioTimelineStep step;
  final bool last;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    final pending = step.state == VitrioTimelineStepState.pending;
    return MergeSemantics(
      child: Semantics(
        value: step.stateLabel,
        child: IntrinsicHeight(
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              SizedBox(
                width: _dot,
                child: Column(
                  children: [
                    _Dot(state: step.state),
                    if (!last)
                      Expanded(
                        child: Container(
                          width: 2,
                          constraints: const BoxConstraints(minHeight: 18),
                          margin: const EdgeInsets.symmetric(vertical: VitrioSpace.sp4),
                          color: c.line,
                        ),
                      ),
                  ],
                ),
              ),
              const SizedBox(width: VitrioSpace.sp12),
              Expanded(
                child: Padding(
                  padding: EdgeInsets.only(top: 3, bottom: last ? 0 : VitrioSpace.sp16),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        step.title,
                        style: VitrioText.weighted(VitrioText.label, pending ? 700 : 800).copyWith(color: pending ? c.inkMuted : c.ink),
                      ),
                      if (step.subtitle != null) ...[
                        const SizedBox(height: 2),
                        Text(step.subtitle!, style: VitrioText.weighted(VitrioText.caption, 600).copyWith(color: c.inkMuted)),
                      ],
                    ],
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _Dot extends StatelessWidget {
  const _Dot({required this.state});

  final VitrioTimelineStepState state;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    final (Color fill, Border? border) = switch (state) {
      VitrioTimelineStepState.done => (c.green, null),
      VitrioTimelineStepState.current => (c.lime, Border.all(color: c.plum, width: 3)),
      VitrioTimelineStepState.pending => (c.surfaceRaised, Border.all(color: c.lineStrong, width: 2)),
    };
    return Container(
      width: _StepRow._dot,
      height: _StepRow._dot,
      alignment: Alignment.center,
      decoration: BoxDecoration(color: fill, shape: BoxShape.circle, border: border),
      child: state == VitrioTimelineStepState.done
          ? VitrioIcon(VitrioIconName.check, size: 14, color: c.surfaceRaised, strokeWidth: 3)
          : null,
    );
  }
}
