import 'package:flutter/material.dart';

abstract final class VitrioShadow {
  static List<BoxShadow> sm(BuildContext context) => [
        BoxShadow(color: _dark(context) ? const Color(0x66000000) : const Color(0x0A17121F), blurRadius: 2, offset: const Offset(0, 1)),
      ];

  static List<BoxShadow> md(BuildContext context) => [
        BoxShadow(color: _dark(context) ? const Color(0x80000000) : const Color(0x1F3E1F57), blurRadius: 30, offset: const Offset(0, 10)),
      ];

  static List<BoxShadow> lg(BuildContext context) => [
        BoxShadow(color: _dark(context) ? const Color(0xA6000000) : const Color(0x59140820), blurRadius: 40, offset: const Offset(0, 18)),
      ];

  static bool _dark(BuildContext context) => Theme.of(context).brightness == Brightness.dark;
}
