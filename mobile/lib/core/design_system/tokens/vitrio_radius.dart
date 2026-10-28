import 'package:flutter/widgets.dart';

abstract final class VitrioRadius {
  static const sm = Radius.circular(10);
  static const md = Radius.circular(12);
  static const lg = Radius.circular(14);
  static const xl = Radius.circular(16);
  static const xxl = Radius.circular(18);
  static const card = Radius.circular(20);
  static const hero = Radius.circular(22);
  static const full = Radius.circular(9999);

  static BorderRadius providerAvatar(double size) => BorderRadius.circular(size * 0.31);
}
