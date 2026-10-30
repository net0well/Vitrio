import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import '../tokens/vitrio_colors.dart';
import '../tokens/vitrio_radius.dart';
import '../tokens/vitrio_space.dart';
import '../tokens/vitrio_text.dart';
import 'vitrio_icon.dart';
import 'vitrio_icon_name.dart';

class VitrioTextField extends StatelessWidget {
  const VitrioTextField({
    super.key,
    required this.label,
    required this.controller,
    this.hint,
    this.error,
    this.placeholder,
    this.icon,
    this.prefix,
    this.suffix,
    this.maxLength,
    this.minLines,
    this.maxLines = 1,
    this.keyboardType,
    this.textCapitalization = TextCapitalization.none,
    this.textInputAction,
    this.autofillHints,
    this.inputFormatters,
    this.obscureText = false,
    this.autocorrect = true,
    this.enableSuggestions = true,
    this.enabled = true,
    this.showLabel = true,
    this.filledInset = false,
    this.onChanged,
    this.onSubmitted,
    this.textAlign = TextAlign.start,
    this.inputStyle,
    this.autofocus = false,
  });

  final String label;
  final TextEditingController controller;
  final String? hint;
  final String? error;
  final String? placeholder;
  final VitrioIconName? icon;
  final String? prefix;
  final Widget? suffix;
  final int? maxLength;
  final int? minLines;
  final int? maxLines;
  final TextInputType? keyboardType;
  final TextCapitalization textCapitalization;
  final TextInputAction? textInputAction;
  final Iterable<String>? autofillHints;
  final List<TextInputFormatter>? inputFormatters;
  final bool obscureText;
  final bool autocorrect;
  final bool enableSuggestions;
  final bool enabled;
  final bool showLabel;
  final bool filledInset;
  final ValueChanged<String>? onChanged;
  final ValueChanged<String>? onSubmitted;
  final TextAlign textAlign;
  final TextStyle? inputStyle;
  final bool autofocus;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    final multiline = maxLines != 1;
    final radius = BorderRadius.all(multiline ? VitrioRadius.xl : VitrioRadius.lg);
    OutlineInputBorder border(Color color, [double width = 1]) => OutlineInputBorder(
      borderRadius: radius,
      borderSide: BorderSide(color: color, width: width),
    );
    final message = error ?? hint;
    final money = prefix != null;
    final style =
        inputStyle?.copyWith(color: c.ink) ??
        (money
            ? VitrioText.title.copyWith(color: c.ink)
            : multiline
            ? VitrioText.body.copyWith(color: c.ink)
            : VitrioText.weighted(VitrioText.bodyStrong, 700).copyWith(color: c.ink));

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        if (showLabel) ...[Text(label, style: VitrioText.label.copyWith(color: c.ink)), const SizedBox(height: VitrioSpace.sp6)],
        TextField(
          controller: controller,
          enabled: enabled,
          minLines: multiline ? (minLines ?? 4) : null,
          maxLines: maxLines,
          maxLength: maxLength,
          keyboardType: keyboardType,
          textCapitalization: textCapitalization,
          textInputAction: textInputAction,
          autofillHints: autofillHints,
          inputFormatters: inputFormatters,
          obscureText: obscureText,
          autocorrect: autocorrect,
          enableSuggestions: enableSuggestions,
          onChanged: onChanged,
          onSubmitted: onSubmitted,
          style: style,
          textAlign: textAlign,
          autofocus: autofocus,
          decoration: InputDecoration(
            hintText: placeholder,
            semanticCounterText: '',
            counterText: '',
            fillColor: filledInset ? c.surfaceInset : c.surfaceRaised,
            contentPadding: multiline
                ? const EdgeInsets.all(VitrioSpace.sp14)
                : EdgeInsets.symmetric(horizontal: VitrioSpace.sp14, vertical: money ? VitrioSpace.sp12 : VitrioSpace.sp16),
            prefixIcon: switch ((icon, prefix)) {
              (final VitrioIconName icon, _) => Padding(
                padding: const EdgeInsetsDirectional.only(start: VitrioSpace.sp14, end: VitrioSpace.sp10),
                child: VitrioIcon(icon, size: 20, color: c.inkMuted),
              ),
              (_, final String prefix) => Padding(
                padding: const EdgeInsetsDirectional.only(start: VitrioSpace.sp14, end: VitrioSpace.sp8),
                child: Center(
                  widthFactor: 1,
                  child: Text(prefix, style: VitrioText.price.copyWith(fontSize: 20, color: c.inkMuted)),
                ),
              ),
              _ => null,
            },
            prefixIconConstraints: const BoxConstraints(minHeight: VitrioSpace.tap),
            suffixIcon: suffix,
            enabledBorder: border(error == null ? (filledInset ? c.line : c.lineStrong) : c.danger),
            focusedBorder: border(error == null ? c.plum : c.danger, 1.5),
            disabledBorder: border(c.line),
          ),
        ),
        if (message != null || maxLength != null) ...[
          const SizedBox(height: VitrioSpace.sp6),
          Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Expanded(
                child: Semantics(
                  liveRegion: error != null,
                  child: Text(
                    message ?? '',
                    style: VitrioText.weighted(VitrioText.caption, 600).copyWith(color: error == null ? c.inkMuted : c.danger),
                  ),
                ),
              ),
              if (maxLength case final max?)
                ListenableBuilder(
                  listenable: controller,
                  builder: (context, _) => Text(
                    '${controller.text.characters.length}/$max',
                    style: VitrioText.weighted(VitrioText.caption, 600).copyWith(color: c.inkMuted),
                  ),
                ),
            ],
          ),
        ],
      ],
    );
  }
}
