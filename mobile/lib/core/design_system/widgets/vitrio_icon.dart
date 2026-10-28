import 'package:flutter/widgets.dart';
import 'package:flutter_svg/flutter_svg.dart';

import '../tokens/vitrio_colors.dart';

enum VitrioIconName {
  arrowRight('arrow-right'),
  bell('bell'),
  bolt('bolt'),
  brick('brick'),
  broom('broom'),
  calendar('calendar'),
  camera('camera'),
  card('card'),
  cash('cash'),
  chart('chart'),
  check('check'),
  chevronDown('chevron-down'),
  chevronLeft('chevron-left'),
  chevronRight('chevron-right'),
  clock('clock'),
  droplet('droplet'),
  flag('flag'),
  grid('grid'),
  heart('heart'),
  home('home'),
  image('image'),
  info('info'),
  leaf('leaf'),
  lock('lock'),
  message('message'),
  orders('orders'),
  pin('pin'),
  plus('plus'),
  qr('qr'),
  roller('roller'),
  search('search'),
  send('send'),
  share('share'),
  shieldCheck('shield-check'),
  sliders('sliders'),
  star('star'),
  transfer('transfer'),
  user('user'),
  wallet('wallet'),
  wrench('wrench'),
  x('x');

  const VitrioIconName(this.fileName);

  final String fileName;

  String get assetPath => 'assets/icons/$fileName.svg';
}

class VitrioIcon extends StatelessWidget {
  const VitrioIcon(this.icon, {super.key, this.size, this.color, this.semanticLabel});

  final VitrioIconName icon;
  final double? size;
  final Color? color;
  final String? semanticLabel;

  @override
  Widget build(BuildContext context) {
    final iconTheme = IconTheme.of(context);
    final dimension = size ?? iconTheme.size ?? 20;
    return SvgPicture.asset(
      icon.assetPath,
      width: dimension,
      height: dimension,
      colorFilter: ColorFilter.mode(
        color ?? iconTheme.color ?? context.vitrio.ink,
        BlendMode.srcIn,
      ),
      semanticsLabel: semanticLabel,
      excludeFromSemantics: semanticLabel == null,
    );
  }
}
