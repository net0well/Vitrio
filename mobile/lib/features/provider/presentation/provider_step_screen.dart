import 'package:flutter/material.dart';

import '../../../app/routes.dart';
import 'provider_step.dart';
import 'steps/about_step.dart';
import 'steps/photos_step.dart';
import 'steps/review_step.dart';
import 'steps/showcase_step.dart';

class ProviderStepScreen extends StatelessWidget {
  const ProviderStepScreen({super.key, required this.step, required this.editing});

  final ProviderStep step;
  final bool editing;

  void _done(BuildContext context) {
    if (step == ProviderStep.review) {
      const ProviderDashboardRoute().go(context);
    } else if (editing) {
      Navigator.of(context).pop();
    } else {
      ProviderOnboardingRoute(step: ProviderStep.values[step.index + 1]).go(context);
    }
  }

  @override
  Widget build(BuildContext context) {
    void onDone() => _done(context);
    return switch (step) {
      ProviderStep.about => AboutStep(editing: editing, onDone: onDone),
      ProviderStep.showcase => ShowcaseStep(editing: editing, onDone: onDone),
      ProviderStep.photos => PhotosStep(editing: editing, onDone: onDone),
      ProviderStep.review => ReviewStep(editing: editing, onDone: onDone),
    };
  }
}
