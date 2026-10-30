import 'package:flutter/material.dart';

import '../tokens/vitrio_colors.dart';
import '../tokens/vitrio_radius.dart';
import '../tokens/vitrio_space.dart';

class VitrioSwitch extends StatelessWidget {
  const VitrioSwitch({super.key, required this.value, required this.onChanged, this.onHero = false});

  static const _width = 52.0;
  static const _height = 32.0;
  static const _knob = 24.0;

  final bool value;
  final ValueChanged<bool>? onChanged;
  final bool onHero;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    final (track, knob) = switch ((onHero, value)) {
      (false, true) => (c.plum, c.plumAccent),
      (false, false) => (c.lineStrong, c.surfaceRaised),
      (true, true) => (c.lime, c.hero),
      (true, false) => (c.heroGlass, c.onHero),
    };
    return Semantics(
      toggled: value,
      enabled: onChanged != null,
      child: GestureDetector(
        onTap: onChanged == null ? null : () => onChanged!(!value),
        child: Opacity(
          opacity: onChanged == null ? 0.4 : 1,
          child: AnimatedContainer(
            duration: const Duration(milliseconds: 180),
            curve: Curves.easeOut,
            width: _width,
            height: _height,
            padding: const EdgeInsets.all(VitrioSpace.sp4),
            decoration: BoxDecoration(color: track, borderRadius: const BorderRadius.all(VitrioRadius.full)),
            child: AnimatedAlign(
              duration: const Duration(milliseconds: 180),
              curve: Curves.easeOut,
              alignment: value ? AlignmentDirectional.centerEnd : AlignmentDirectional.centerStart,
              child: AnimatedContainer(
                duration: const Duration(milliseconds: 180),
                width: _knob,
                height: _knob,
                decoration: BoxDecoration(color: knob, shape: BoxShape.circle),
              ),
            ),
          ),
        ),
      ),
    );
  }
}
