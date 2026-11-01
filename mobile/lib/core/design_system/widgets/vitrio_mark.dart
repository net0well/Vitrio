import 'package:flutter/widgets.dart';
import 'package:flutter_svg/flutter_svg.dart';

class VitrioMark extends StatelessWidget {
  const VitrioMark({super.key, this.size = 56});

  final double size;

  @override
  Widget build(BuildContext context) =>
      SvgPicture.asset('assets/brand/vitrio-mark.svg', width: size, height: size, excludeFromSemantics: true);
}
