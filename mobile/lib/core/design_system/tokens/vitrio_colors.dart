import 'package:flutter/material.dart';

class VitrioColors {
  const VitrioColors._({
    required this.surface,
    required this.surfaceRaised,
    required this.surfaceInset,
    required this.track,
    required this.lineSoft,
    required this.line,
    required this.lineStrong,
    required this.ink,
    required this.ink2,
    required this.inkMuted,
    required this.placeholder,
    required this.plum,
    required this.plumDeep,
    required this.onPlum,
    required this.plumAccent,
    required this.plumSoft,
    required this.lime,
    required this.onLime,
    required this.hero,
    required this.onHero,
    required this.onHeroMuted,
    required this.heroGlass,
    required this.green,
    required this.greenSoft,
    required this.amber,
    required this.amberEdge,
    required this.amberSoft,
    required this.amberInk,
    required this.info,
    required this.infoSoft,
    required this.danger,
    required this.dangerSoft,
    required this.alert,
    required this.focus,
  });

  final Color surface;
  final Color surfaceRaised;
  final Color surfaceInset;
  final Color track;
  final Color lineSoft;
  final Color line;
  final Color lineStrong;
  final Color ink;
  final Color ink2;
  final Color inkMuted;
  final Color placeholder;
  final Color plum;
  final Color plumDeep;
  final Color onPlum;
  final Color plumAccent;
  final Color plumSoft;
  final Color lime;
  final Color onLime;
  final Color hero;
  final Color onHero;
  final Color onHeroMuted;
  final Color heroGlass;
  final Color green;
  final Color greenSoft;
  final Color amber;
  final Color amberEdge;
  final Color amberSoft;
  final Color amberInk;
  final Color info;
  final Color infoSoft;
  final Color danger;
  final Color dangerSoft;
  final Color alert;
  final Color focus;

  static const light = VitrioColors._(
    surface: Color(0xFFF6F4F8),
    surfaceRaised: Color(0xFFFFFFFF),
    surfaceInset: Color(0xFFF6F4F8),
    track: Color(0xFFEAE5EF),
    lineSoft: Color(0xFFEEEAF2),
    line: Color(0xFFE6E1EC),
    lineStrong: Color(0xFF8C8499),
    ink: Color(0xFF17121F),
    ink2: Color(0xFF4F4960),
    inkMuted: Color(0xFF6B6578),
    placeholder: Color(0xFF767080),
    plum: Color(0xFF3E1F57),
    plumDeep: Color(0xFF2A1340),
    onPlum: Color(0xFFFFFFFF),
    plumAccent: Color(0xFFC8F169),
    plumSoft: Color(0xFFEFE7F6),
    lime: Color(0xFFC8F169),
    onLime: Color(0xFF2A1340),
    hero: Color(0xFF3E1F57),
    onHero: Color(0xFFFFFFFF),
    onHeroMuted: Color(0xCCFFFFFF),
    heroGlass: Color(0x1AFFFFFF),
    green: Color(0xFF1C7A4B),
    greenSoft: Color(0xFFE2F3E9),
    amber: Color(0xFFE59E2C),
    amberEdge: Color(0xFFA86A00),
    amberSoft: Color(0xFFFCEFD6),
    amberInk: Color(0xFF8A5300),
    info: Color(0xFF1F4E8C),
    infoSoft: Color(0xFFE3ECFA),
    danger: Color(0xFFB42318),
    dangerSoft: Color(0xFFFDEAEA),
    alert: Color(0xFFC2410C),
    focus: Color(0xFF3E1F57),
  );

  static const dark = VitrioColors._(
    surface: Color(0xFF141019),
    surfaceRaised: Color(0xFF1E1826),
    surfaceInset: Color(0xFF262030),
    track: Color(0xFF2B2436),
    lineSoft: Color(0xFF2A2333),
    line: Color(0xFF352C40),
    lineStrong: Color(0xFF6E6580),
    ink: Color(0xFFF3EFF7),
    ink2: Color(0xFFCFC7DA),
    inkMuted: Color(0xFFA79EB4),
    placeholder: Color(0xFF8F87A0),
    plum: Color(0xFFB994DD),
    plumDeep: Color(0xFFCDB2E8),
    onPlum: Color(0xFF1A1022),
    plumAccent: Color(0xFF1A1022),
    plumSoft: Color(0xFF2E2140),
    lime: Color(0xFFC8F169),
    onLime: Color(0xFF2A1340),
    hero: Color(0xFF2A1640),
    onHero: Color(0xFFF3EFF7),
    onHeroMuted: Color(0xCCF3EFF7),
    heroGlass: Color(0x14FFFFFF),
    green: Color(0xFF5FD39A),
    greenSoft: Color(0xFF13301F),
    amber: Color(0xFFF2B552),
    amberEdge: Color(0xFFF2B552),
    amberSoft: Color(0xFF3A2A0C),
    amberInk: Color(0xFFF2C36B),
    info: Color(0xFF8DB6F0),
    infoSoft: Color(0xFF16243A),
    danger: Color(0xFFFF8A7A),
    dangerSoft: Color(0xFF3A1512),
    alert: Color(0xFFFF9B6B),
    focus: Color(0xFFC8F169),
  );
}

extension VitrioThemeContext on BuildContext {
  VitrioColors get vitrio =>
      Theme.of(this).brightness == Brightness.dark ? VitrioColors.dark : VitrioColors.light;
}
