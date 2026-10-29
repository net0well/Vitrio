import 'package:flutter/material.dart';

import '../../../../core/design_system/tokens/vitrio_space.dart';
import '../../../../core/design_system/tokens/vitrio_text.dart';

class FormPage extends StatelessWidget {
  const FormPage({
    super.key,
    required this.title,
    required this.children,
    required this.actionLabel,
    required this.onAction,
    this.busy = false,
    this.message,
  });

  final String title;
  final List<Widget> children;
  final String actionLabel;
  final VoidCallback? onAction;
  final bool busy;
  final String? message;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text(title)),
      body: SafeArea(
        child: AutofillGroup(
          child: ListView(
            padding: const EdgeInsets.all(VitrioSpace.sp20),
            children: [
              ...children,
              if (message != null) ...[
                const SizedBox(height: VitrioSpace.sp16),
                Semantics(
                  liveRegion: true,
                  child: Text(message!, style: VitrioText.meta.copyWith(color: Theme.of(context).colorScheme.error)),
                ),
              ],
            ],
          ),
        ),
      ),
      bottomNavigationBar: SafeArea(
        child: Padding(
          padding: const EdgeInsets.fromLTRB(VitrioSpace.sp20, VitrioSpace.sp8, VitrioSpace.sp20, VitrioSpace.sp16),
          child: SizedBox(
            height: VitrioSpace.cta,
            child: FilledButton(
              onPressed: busy ? null : onAction,
              child: busy
                  ? const SizedBox.square(dimension: 22, child: CircularProgressIndicator(strokeWidth: 2.5))
                  : Text(actionLabel),
            ),
          ),
        ),
      ),
    );
  }
}
