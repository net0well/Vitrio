import 'package:flutter/material.dart';

import '../tokens/vitrio_colors.dart';
import '../tokens/vitrio_radius.dart';
import '../tokens/vitrio_shadow.dart';
import '../tokens/vitrio_space.dart';
import '../tokens/vitrio_text.dart';
import 'vitrio_icon.dart';
import 'vitrio_icon_name.dart';

class VitrioSearchField extends StatefulWidget {
  const VitrioSearchField({
    super.key,
    required this.label,
    required this.submitLabel,
    required this.onSubmit,
    this.placeholder,
    this.initialValue,
  });

  final String label;
  final String submitLabel;
  final String? placeholder;
  final String? initialValue;
  final ValueChanged<String> onSubmit;

  @override
  State<VitrioSearchField> createState() => _VitrioSearchFieldState();
}

class _VitrioSearchFieldState extends State<VitrioSearchField> {
  late final TextEditingController _controller = TextEditingController(text: widget.initialValue);

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  void _submit() => widget.onSubmit(_controller.text.trim());

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    return Container(
      height: VitrioSpace.controlLg,
      padding: const EdgeInsetsDirectional.only(start: VitrioSpace.sp16, end: 7),
      decoration: BoxDecoration(
        color: c.surfaceRaised,
        borderRadius: const BorderRadius.all(VitrioRadius.xl),
        border: Border.all(color: c.lineStrong),
        boxShadow: VitrioShadow.sm(context),
      ),
      child: Row(
        children: [
          VitrioIcon(VitrioIconName.search, size: 20, color: c.inkMuted),
          const SizedBox(width: VitrioSpace.sp10),
          Expanded(
            child: Semantics(
              label: widget.label,
              textField: true,
              child: TextField(
                controller: _controller,
                textInputAction: TextInputAction.search,
                onSubmitted: (_) => _submit(),
                style: VitrioText.weighted(VitrioText.bodyStrong, 600).copyWith(color: c.ink),
                decoration: InputDecoration(
                  hintText: widget.placeholder,
                  hintStyle: VitrioText.weighted(VitrioText.bodyStrong, 500).copyWith(color: c.placeholder),
                  filled: false,
                  isDense: true,
                  contentPadding: EdgeInsets.zero,
                  border: InputBorder.none,
                  enabledBorder: InputBorder.none,
                  focusedBorder: InputBorder.none,
                ),
              ),
            ),
          ),
          const SizedBox(width: VitrioSpace.sp10),
          Material(
            color: c.plum,
            borderRadius: const BorderRadius.all(VitrioRadius.md),
            child: InkWell(
              borderRadius: const BorderRadius.all(VitrioRadius.md),
              onTap: _submit,
              child: Tooltip(
                message: widget.submitLabel,
                child: SizedBox.square(
                  dimension: 40,
                  child: Center(child: VitrioIcon(VitrioIconName.arrowRight, size: 18, color: c.onPlum)),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }
}
