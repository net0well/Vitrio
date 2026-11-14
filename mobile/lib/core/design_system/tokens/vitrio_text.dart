import 'package:flutter/material.dart';

import 'vitrio_colors.dart';

abstract final class VitrioText {
  static const displayFamily = 'BricolageGrotesque';
  static const sansFamily = 'PlusJakartaSans';

  static const _opticalSizes = (min: 12.0, max: 96.0);

  static TextStyle _style(String family, double size, double lineHeight, int weight, [double spacing = 0]) {
    return TextStyle(
      fontFamily: family,
      fontSize: size,
      height: lineHeight / size,
      fontWeight: FontWeight.values[weight ~/ 100 - 1],
      fontVariations: _variations(family, size, weight),
      letterSpacing: spacing,
    );
  }

  // The display font has an optical size axis that browsers follow automatically; without it Flutter draws every size with the 96pt cut.
  static List<FontVariation> _variations(String? family, double size, int weight) => [
        FontVariation('wght', weight.toDouble()),
        if (family == displayFamily) FontVariation('opsz', size.clamp(_opticalSizes.min, _opticalSizes.max)),
      ];

  static TextStyle weighted(TextStyle style, int weight) => style.copyWith(
        fontWeight: FontWeight.values[weight ~/ 100 - 1],
        fontVariations: _variations(style.fontFamily, style.fontSize ?? 14, weight),
      );

  static TextStyle sized(TextStyle style, double size) => style.copyWith(
        fontSize: size,
        fontVariations: _variations(style.fontFamily, size, style.fontWeight?.value ?? 400),
      );

  static final displayXl = _style(displayFamily, 34, 37, 800, -0.8);
  static final display = _style(displayFamily, 30, 33, 700, -0.7);
  static final headline = _style(displayFamily, 24, 29, 700, -0.4);
  static final title = _style(displayFamily, 22, 26, 700, -0.4);
  static final section = _style(displayFamily, 19, 24, 700, -0.3);
  static final priceLg = _style(displayFamily, 36, 40, 800, -1);
  static final price = _style(displayFamily, 18, 22, 700);
  static final labelLg = _style(sansFamily, 16, 22, 800);
  static final bodyStrong = _style(sansFamily, 15, 20, 800);
  static final label = _style(sansFamily, 14, 18, 800);
  static final body = _style(sansFamily, 14, 21, 400);
  static final meta = _style(sansFamily, 13, 18, 600);
  static final caption = _style(sansFamily, 12, 16, 800);
  static final micro = _style(sansFamily, 11, 14, 700);

  static TextTheme textTheme(VitrioColors c) {
    return TextTheme(
      displayLarge: displayXl.copyWith(color: c.ink),
      displayMedium: display.copyWith(color: c.ink),
      headlineSmall: title.copyWith(color: c.ink),
      titleLarge: section.copyWith(color: c.ink),
      titleMedium: bodyStrong.copyWith(color: c.ink),
      titleSmall: label.copyWith(color: c.ink),
      bodyLarge: body.copyWith(color: c.ink2),
      bodyMedium: body.copyWith(color: c.ink2),
      bodySmall: meta.copyWith(color: c.inkMuted),
      labelLarge: label.copyWith(color: c.ink),
      labelMedium: caption.copyWith(color: c.ink),
      labelSmall: micro.copyWith(color: c.inkMuted),
    );
  }
}
