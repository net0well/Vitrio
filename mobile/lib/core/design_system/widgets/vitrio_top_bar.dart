import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import '../tokens/vitrio_colors.dart';
import '../tokens/vitrio_space.dart';
import '../tokens/vitrio_text.dart';
import 'vitrio_icon_button.dart';
import 'vitrio_icon_name.dart';

class VitrioTopBar extends StatelessWidget implements PreferredSizeWidget {
  const VitrioTopBar({
    super.key,
    this.title,
    this.onBack,
    this.backIcon = VitrioIconName.chevronLeft,
    this.backLabel,
    this.trailing,
    this.center,
  });

  final String? title;
  final VoidCallback? onBack;
  final VitrioIconName backIcon;
  final String? backLabel;
  final Widget? trailing;
  final Widget? center;

  @override
  Size get preferredSize => const Size.fromHeight(VitrioSpace.tap + VitrioSpace.sp16);

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    final canPop = ModalRoute.of(context)?.impliesAppBarDismissal ?? false;
    final back = onBack ?? (canPop ? () => Navigator.of(context).maybePop() : null);
    return AnnotatedRegion<SystemUiOverlayStyle>(
      value: Theme.of(context).brightness == Brightness.dark ? SystemUiOverlayStyle.light : SystemUiOverlayStyle.dark,
      child: ColoredBox(
        color: c.surface,
        child: SafeArea(
          bottom: false,
          child: Padding(
            padding: const EdgeInsets.fromLTRB(VitrioSpace.sp20, VitrioSpace.sp16, VitrioSpace.sp20, 0),
            child: SizedBox(
              height: VitrioSpace.tap,
              child: Row(
                children: [
                  if (back != null)
                    VitrioIconButton(
                      icon: backIcon,
                      label: backLabel ?? MaterialLocalizations.of(context).backButtonTooltip,
                      onPressed: back,
                    )
                  else
                    const SizedBox(width: VitrioSpace.tap),
                  const SizedBox(width: VitrioSpace.sp10),
                  Expanded(
                    child:
                        center ??
                        Semantics(
                          header: true,
                          child: Text(
                            title ?? '',
                            textAlign: TextAlign.center,
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                            style: VitrioText.label.copyWith(fontSize: 17, color: c.ink),
                          ),
                        ),
                  ),
                  const SizedBox(width: VitrioSpace.sp10),
                  trailing ?? const SizedBox(width: VitrioSpace.tap),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
