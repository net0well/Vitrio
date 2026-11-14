import 'package:flutter/material.dart';

import '../tokens/vitrio_colors.dart';
import '../tokens/vitrio_radius.dart';
import '../tokens/vitrio_space.dart';
import '../tokens/vitrio_text.dart';
import 'vitrio_icon.dart';
import 'vitrio_icon_name.dart';
import 'vitrio_option.dart';

class VitrioSelectField<T> extends StatelessWidget {
  const VitrioSelectField({super.key, required this.label, required this.value, required this.options, required this.onChanged});

  final String label;
  final T value;
  final List<VitrioOption<T>> options;
  final ValueChanged<T> onChanged;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    final selected = options.any((option) => option.value == value) ? value : null;
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        Text(label, style: VitrioText.label.copyWith(color: c.ink)),
        const SizedBox(height: VitrioSpace.sp6),
        Container(
          height: VitrioSpace.controlLg,
          padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp14),
          decoration: BoxDecoration(
            color: c.surfaceRaised,
            borderRadius: const BorderRadius.all(VitrioRadius.lg),
            border: Border.all(color: c.lineStrong),
          ),
          child: Theme(
            data: Theme.of(context).copyWith(focusColor: c.plumSoft, highlightColor: c.plumSoft),
            child: DropdownButtonHideUnderline(
              child: DropdownButton<T>(
                value: selected,
                isExpanded: true,
                borderRadius: const BorderRadius.all(VitrioRadius.lg),
                dropdownColor: c.surfaceRaised,
                menuMaxHeight: 360,
                icon: VitrioIcon(VitrioIconName.chevronDown, size: 16, color: c.inkMuted),
                style: VitrioText.weighted(VitrioText.label, 700).copyWith(color: c.ink),
                items: [for (final option in options) DropdownMenuItem(value: option.value, child: Text(option.label))],
                onChanged: (choice) {
                  if (choice != null) {
                    onChanged(choice);
                  }
                },
              ),
            ),
          ),
        ),
      ],
    );
  }
}
