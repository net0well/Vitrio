import 'vitrio_timeline_step_state.dart';

class VitrioTimelineStep {
  const VitrioTimelineStep({required this.title, required this.state, this.subtitle, this.stateLabel});

  final String title;
  final VitrioTimelineStepState state;
  final String? subtitle;
  final String? stateLabel;
}
