import 'package:flutter/material.dart';

import '../design_system/tokens/vitrio_colors.dart';
import '../design_system/tokens/vitrio_space.dart';
import '../design_system/tokens/vitrio_text.dart';
import '../design_system/widgets/vitrio_action_bar.dart';
import '../design_system/widgets/vitrio_button.dart';
import '../design_system/widgets/vitrio_icon_name.dart';
import '../design_system/widgets/vitrio_notice.dart';
import '../design_system/widgets/vitrio_top_bar.dart';

class FormPage extends StatelessWidget {
  const FormPage({
    super.key,
    required this.title,
    required this.children,
    required this.actionLabel,
    required this.onAction,
    this.busy = false,
    this.message,
    this.heading,
    this.subtitle,
    this.note,
    this.actionIcon,
    this.topBarCenter,
    this.topBarTrailing,
    this.onBack,
    this.header,
  });

  final String title;
  final List<Widget> children;
  final String actionLabel;
  final VoidCallback? onAction;
  final bool busy;
  final String? message;
  final String? heading;
  final String? subtitle;
  final String? note;
  final VitrioIconName? actionIcon;
  final Widget? topBarCenter;
  final Widget? topBarTrailing;
  final VoidCallback? onBack;
  final Widget? header;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    return Scaffold(
      appBar: VitrioTopBar(title: title, center: topBarCenter, trailing: topBarTrailing, onBack: onBack),
      body: AutofillGroup(
        child: ListView(
          padding: const EdgeInsets.fromLTRB(VitrioSpace.sp20, VitrioSpace.sp16, VitrioSpace.sp20, VitrioSpace.sp24),
          children: [
            ?header,
            if (heading != null) ...[
              Semantics(header: true, child: Text(heading!, style: VitrioText.display.copyWith(color: c.ink))),
              const SizedBox(height: VitrioSpace.sp6),
            ],
            if (subtitle != null) ...[
              Text(subtitle!, style: VitrioText.body.copyWith(color: c.inkMuted)),
              const SizedBox(height: VitrioSpace.sp24),
            ],
            ...children,
          ],
        ),
      ),
      bottomNavigationBar: VitrioActionBar(
        note: note,
        top: message == null ? null : VitrioNotice(message: message!),
        children: [
          VitrioButton(label: actionLabel, onPressed: onAction, busy: busy, trailingIcon: actionIcon),
        ],
      ),
    );
  }
}
