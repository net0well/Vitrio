import 'package:flutter/material.dart';

import 'tokens/vitrio_colors.dart';
import 'tokens/vitrio_radius.dart';
import 'tokens/vitrio_space.dart';
import 'tokens/vitrio_text.dart';

ThemeData vitrioTheme(Brightness brightness) {
  final c = brightness == Brightness.dark ? VitrioColors.dark : VitrioColors.light;
  final scheme = ColorScheme(
    brightness: brightness,
    primary: c.plum,
    onPrimary: c.onPlum,
    primaryContainer: c.plumSoft,
    onPrimaryContainer: c.plum,
    secondary: c.lime,
    onSecondary: c.onLime,
    tertiary: c.green,
    onTertiary: c.surfaceRaised,
    error: c.danger,
    onError: c.surfaceRaised,
    errorContainer: c.dangerSoft,
    onErrorContainer: c.danger,
    surface: c.surface,
    onSurface: c.ink,
    onSurfaceVariant: c.inkMuted,
    surfaceContainerLowest: c.surfaceRaised,
    surfaceContainerLow: c.surfaceRaised,
    surfaceContainer: c.surfaceInset,
    surfaceContainerHigh: c.track,
    outline: c.lineStrong,
    outlineVariant: c.line,
  );
  OutlineInputBorder border(Color color, [double width = 1]) => OutlineInputBorder(
        borderRadius: const BorderRadius.all(VitrioRadius.lg),
        borderSide: BorderSide(color: color, width: width),
      );
  return ThemeData(
    useMaterial3: true,
    colorScheme: scheme,
    scaffoldBackgroundColor: c.surface,
    appBarTheme: AppBarTheme(
      backgroundColor: c.surface,
      foregroundColor: c.ink,
      surfaceTintColor: Colors.transparent,
      centerTitle: true,
      toolbarHeight: VitrioSpace.tap,
      titleTextStyle: VitrioText.label.copyWith(fontSize: 17, color: c.ink),
    ),
    fontFamily: VitrioText.sansFamily,
    textTheme: VitrioText.textTheme(c),
    filledButtonTheme: FilledButtonThemeData(
      style: FilledButton.styleFrom(
        backgroundColor: c.plum,
        foregroundColor: c.onPlum,
        minimumSize: const Size.fromHeight(VitrioSpace.cta),
        shape: const RoundedRectangleBorder(borderRadius: BorderRadius.all(VitrioRadius.xl)),
        textStyle: VitrioText.labelLg,
      ),
    ),
    outlinedButtonTheme: OutlinedButtonThemeData(
      style: OutlinedButton.styleFrom(
        foregroundColor: c.ink,
        backgroundColor: c.surfaceRaised,
        minimumSize: const Size(VitrioSpace.tap, VitrioSpace.controlLg),
        side: BorderSide(color: c.line),
        shape: const RoundedRectangleBorder(borderRadius: BorderRadius.all(VitrioRadius.xl)),
        textStyle: VitrioText.label,
      ),
    ),
    textButtonTheme: TextButtonThemeData(
      style: TextButton.styleFrom(foregroundColor: c.plum, textStyle: VitrioText.label),
    ),
    inputDecorationTheme: InputDecorationTheme(
      filled: true,
      fillColor: c.surfaceRaised,
      hintStyle: VitrioText.body.copyWith(color: c.placeholder),
      contentPadding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp14, vertical: VitrioSpace.sp16),
      enabledBorder: border(c.lineStrong),
      focusedBorder: border(c.plum, 1.5),
      errorBorder: border(c.danger),
      focusedErrorBorder: border(c.danger, 1.5),
      errorStyle: VitrioText.weighted(VitrioText.caption, 600).copyWith(color: c.danger),
    ),
    switchTheme: SwitchThemeData(
      thumbColor: WidgetStateProperty.resolveWith(
        (states) => states.contains(WidgetState.selected) ? c.plumAccent : c.surfaceRaised,
      ),
      trackColor: WidgetStateProperty.resolveWith(
        (states) => states.contains(WidgetState.selected) ? c.plum : c.lineStrong,
      ),
      trackOutlineColor: const WidgetStatePropertyAll(Colors.transparent),
    ),
    navigationBarTheme: NavigationBarThemeData(
      backgroundColor: c.surfaceRaised,
      indicatorColor: c.plumSoft,
      surfaceTintColor: Colors.transparent,
      labelTextStyle: WidgetStateProperty.resolveWith(
        (states) => VitrioText.weighted(VitrioText.micro, states.contains(WidgetState.selected) ? 800 : 700)
            .copyWith(color: states.contains(WidgetState.selected) ? c.plum : c.inkMuted),
      ),
      iconTheme: WidgetStateProperty.resolveWith(
        (states) => IconThemeData(size: 20, color: states.contains(WidgetState.selected) ? c.plum : c.inkMuted),
      ),
    ),
    bottomSheetTheme: BottomSheetThemeData(
      backgroundColor: c.surfaceRaised,
      surfaceTintColor: Colors.transparent,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: VitrioRadius.hero)),
    ),
    dividerTheme: DividerThemeData(color: c.lineSoft, thickness: 1, space: 1),
    dialogTheme: DialogThemeData(
      backgroundColor: c.surfaceRaised,
      surfaceTintColor: Colors.transparent,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.all(VitrioRadius.card)),
      titleTextStyle: VitrioText.title.copyWith(color: c.ink),
      contentTextStyle: VitrioText.body.copyWith(color: c.ink2),
    ),
    checkboxTheme: CheckboxThemeData(
      fillColor: WidgetStateProperty.resolveWith((states) => states.contains(WidgetState.selected) ? c.plum : Colors.transparent),
      checkColor: WidgetStatePropertyAll(c.plumAccent),
      side: BorderSide(color: c.lineStrong, width: 1.5),
    ),
    snackBarTheme: SnackBarThemeData(
      backgroundColor: c.ink,
      contentTextStyle: VitrioText.weighted(VitrioText.meta, 700).copyWith(color: c.surfaceRaised),
      behavior: SnackBarBehavior.floating,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.all(VitrioRadius.md)),
    ),
    progressIndicatorTheme: ProgressIndicatorThemeData(color: c.plum, linearTrackColor: c.track),
    sliderTheme: SliderThemeData(
      tickMarkShape: SliderTickMarkShape.noTickMark,
      padding: EdgeInsets.zero,
      trackHeight: 6,
      activeTrackColor: c.plum,
      inactiveTrackColor: c.track,
      thumbColor: c.plum,
      overlayColor: c.plum.withValues(alpha: 0.12),
      valueIndicatorColor: c.plum,
      valueIndicatorTextStyle: VitrioText.caption.copyWith(color: c.onPlum),
    ),
  );
}
