import 'package:flutter/material.dart';

import '../tokens/vitrio_colors.dart';
import '../tokens/vitrio_radius.dart';
import '../tokens/vitrio_space.dart';
import '../tokens/vitrio_text.dart';
import 'vitrio_option.dart';

class VitrioChoiceList<T> extends StatelessWidget {
  const VitrioChoiceList({
    super.key,
    required this.label,
    required this.options,
    required this.value,
    required this.onChanged,
  });

  final String label;
  final List<VitrioOption<T>> options;
  final T? value;
  final ValueChanged<T> onChanged;

  @override
  Widget build(BuildContext context) => Semantics(
        container: true,
        label: label,
        explicitChildNodes: true,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            for (final (index, option) in options.indexed) ...[
              if (index > 0) const SizedBox(height: VitrioSpace.sp8),
              _Choice(label: option.label, selected: option.value == value, onTap: () => onChanged(option.value)),
            ],
          ],
        ),
      );
}

class _Choice extends StatelessWidget {
  const _Choice({required this.label, required this.selected, required this.onTap});

  static const _height = 52.0;
  static const _radio = 20.0;
  static const _radioDot = 10.0;

  final String label;
  final bool selected;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    return Semantics(
      inMutuallyExclusiveGroup: true,
      checked: selected,
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          borderRadius: const BorderRadius.all(VitrioRadius.lg),
          onTap: onTap,
          child: AnimatedContainer(
            duration: const Duration(milliseconds: 180),
            curve: Curves.easeOut,
            height: _height,
            padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp16),
            decoration: BoxDecoration(
              color: selected ? c.plumSoft : c.surfaceRaised,
              borderRadius: const BorderRadius.all(VitrioRadius.lg),
              border: Border.all(color: selected ? c.plum : c.line, width: selected ? 1.5 : 1),
            ),
            child: Row(
              children: [
                Container(
                  width: _radio,
                  height: _radio,
                  alignment: Alignment.center,
                  decoration: BoxDecoration(
                    shape: BoxShape.circle,
                    border: Border.all(color: selected ? c.plum : c.lineStrong, width: 2),
                  ),
                  child: selected
                      ? Container(
                          width: _radioDot,
                          height: _radioDot,
                          decoration: BoxDecoration(color: c.plum, shape: BoxShape.circle),
                        )
                      : null,
                ),
                const SizedBox(width: VitrioSpace.sp12),
                Expanded(
                  child: Text(
                    label,
                    style: VitrioText.weighted(VitrioText.bodyStrong, selected ? 800 : 700).copyWith(color: selected ? c.plum : c.ink),
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
