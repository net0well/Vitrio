import 'package:flutter/widgets.dart';
import 'package:flutter_svg/flutter_svg.dart';

class VitrioWordmark extends StatelessWidget {
  const VitrioWordmark({super.key, this.onDark = false, this.height = 32});

  final bool onDark;
  final double height;

  @override
  Widget build(BuildContext context) {
    return SvgPicture.asset(
      onDark ? 'assets/brand/vitrio-wordmark-on-dark.svg' : 'assets/brand/vitrio-wordmark.svg',
      height: height,
      semanticsLabel: 'Vitrio',
    );
  }
}
