import 'package:flutter/material.dart';

import '../tokens/vitrio_colors.dart';
import '../tokens/vitrio_radius.dart';
import '../tokens/vitrio_shadow.dart';
import '../tokens/vitrio_space.dart';
import '../tokens/vitrio_text.dart';
import 'vitrio_option.dart';

class VitrioSegmented<T> extends StatelessWidget {
  const VitrioSegmented({
    super.key,
    required this.label,
    required this.options,
    required this.value,
    required this.onChanged,
  });

  final String label;
  final List<VitrioOption<T>> options;
  final T value;
  final ValueChanged<T> onChanged;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    return Semantics(
      container: true,
      label: label,
      explicitChildNodes: true,
      child: Container(
        padding: const EdgeInsets.all(VitrioSpace.sp4),
        decoration: BoxDecoration(color: c.track, borderRadius: const BorderRadius.all(VitrioRadius.lg)),
        child: Row(
          children: [
            for (final (index, option) in options.indexed) ...[
              if (index > 0) const SizedBox(width: VitrioSpace.sp4),
              Expanded(
                child: _Segment(
                  label: option.label,
                  selected: option.value == value,
                  onTap: () => onChanged(option.value),
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }
}

class _Segment extends StatelessWidget {
  const _Segment({required this.label, required this.selected, required this.onTap});

  static const _height = 40.0;

  final String label;
  final bool selected;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    return Semantics(
      button: true,
      selected: selected,
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          borderRadius: const BorderRadius.all(VitrioRadius.sm),
          onTap: selected ? null : onTap,
          child: AnimatedContainer(
            duration: const Duration(milliseconds: 180),
            curve: Curves.easeOut,
            height: _height,
            alignment: Alignment.center,
            padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp8),
            decoration: BoxDecoration(
              color: selected ? c.surfaceRaised : Colors.transparent,
              borderRadius: const BorderRadius.all(VitrioRadius.sm),
              boxShadow: selected ? VitrioShadow.sm(context) : null,
            ),
            child: Text(
              label,
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
              style: VitrioText.weighted(VitrioText.label, selected ? 800 : 700).copyWith(color: selected ? c.ink : c.ink2),
            ),
          ),
        ),
      ),
    );
  }
}
