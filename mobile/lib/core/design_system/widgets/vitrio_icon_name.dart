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
