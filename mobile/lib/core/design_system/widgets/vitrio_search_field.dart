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
    this.controller,
    this.onClear,
    this.clearLabel,
  });

  final String label;
  final String submitLabel;
  final String? placeholder;
  final String? initialValue;
  final TextEditingController? controller;
  final ValueChanged<String> onSubmit;
  final VoidCallback? onClear;
  final String? clearLabel;

  bool get _active => onClear != null;

  @override
  State<VitrioSearchField> createState() => _VitrioSearchFieldState();
}

class _VitrioSearchFieldState extends State<VitrioSearchField> {
  late final TextEditingController _ownController = TextEditingController(
    text: widget.initialValue,
  );

  TextEditingController get _controller => widget.controller ?? _ownController;

  @override
  void dispose() {
    _ownController.dispose();
    super.dispose();
  }

  void _submit() => widget.onSubmit(_controller.text.trim());

  void _clear() {
    _controller.clear();
    widget.onClear!();
  }

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    final active = widget._active;
    return Container(
      height: active ? 48 : VitrioSpace.controlLg,
      padding: EdgeInsetsDirectional.only(
        start: active ? VitrioSpace.sp12 : VitrioSpace.sp16,
        end: active ? VitrioSpace.sp6 : 7,
      ),
      decoration: BoxDecoration(
        color: c.surfaceRaised,
        borderRadius: BorderRadius.all(
          active ? VitrioRadius.lg : VitrioRadius.xl,
        ),
        border: active ? Border.all(color: c.plum, width: 1.5) : Border.all(color: c.lineStrong),
        boxShadow: active ? null : VitrioShadow.sm(context),
      ),
      child: Row(
        children: [
          VitrioIcon(
            VitrioIconName.search,
            size: active ? 18 : 20,
            color: active ? c.plum : c.inkMuted,
          ),
          SizedBox(width: active ? VitrioSpace.sp8 : VitrioSpace.sp10),
          Expanded(
            child: Semantics(
              label: widget.label,
              textField: true,
              child: TextField(
                controller: _controller,
                textInputAction: TextInputAction.search,
                onSubmitted: (_) => _submit(),
                style: VitrioText.weighted(
                  VitrioText.bodyStrong,
                  active ? 700 : 600,
                ).copyWith(color: c.ink),
                decoration: InputDecoration(
                  hintText: widget.placeholder,
                  hintStyle: VitrioText.weighted(
                    VitrioText.bodyStrong,
                    500,
                  ).copyWith(color: c.placeholder),
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
          if (active)
            Material(
              color: c.surfaceInset,
              borderRadius: const BorderRadius.all(VitrioRadius.sm),
              child: InkWell(
                borderRadius: const BorderRadius.all(VitrioRadius.sm),
                onTap: _clear,
                child: Tooltip(
                  message: widget.clearLabel ?? widget.submitLabel,
                  child: SizedBox.square(
                    dimension: 36,
                    child: Center(
                      child: VitrioIcon(
                        VitrioIconName.x,
                        size: 16,
                        color: c.inkMuted,
                      ),
                    ),
                  ),
                ),
              ),
            )
          else
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
                    child: Center(
                      child: VitrioIcon(
                        VitrioIconName.arrowRight,
                        size: 18,
                        color: c.onPlum,
                      ),
                    ),
                  ),
                ),
              ),
            ),
        ],
      ),
    );
  }
}
