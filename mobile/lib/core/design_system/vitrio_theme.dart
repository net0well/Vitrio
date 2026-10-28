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
      errorStyle: VitrioText.caption.copyWith(color: c.danger, fontWeight: FontWeight.w600),
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
        (states) => VitrioText.micro.copyWith(
          color: states.contains(WidgetState.selected) ? c.plum : c.inkMuted,
          fontWeight: states.contains(WidgetState.selected) ? FontWeight.w800 : FontWeight.w700,
        ),
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
  );
}
