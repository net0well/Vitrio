import 'dart:ui';

import 'package:flutter_test/flutter_test.dart';
import 'package:vitrio/core/design_system/tokens/vitrio_text.dart';

void main() {
  double? axis(List<FontVariation>? variations, String tag) =>
      variations?.where((variation) => variation.axis == tag).map((variation) => variation.value).firstOrNull;

  test('display styles follow the font size on the optical size axis', () {
    expect(axis(VitrioText.headline.fontVariations, 'opsz'), 24);
    expect(axis(VitrioText.title.fontVariations, 'opsz'), 22);
    expect(axis(VitrioText.section.fontVariations, 'opsz'), 19);
    expect(axis(VitrioText.priceLg.fontVariations, 'opsz'), 36);
  });

  test('the sans styles have no optical size axis', () => expect(axis(VitrioText.body.fontVariations, 'opsz'), isNull));

  test('a resized display style moves the optical size with it', () {
    final style = VitrioText.sized(VitrioText.title, 26);

    expect(style.fontSize, 26);
    expect(axis(style.fontVariations, 'opsz'), 26);
    expect(axis(style.fontVariations, 'wght'), 700);
  });

  test('a reweighted display style keeps its optical size', () {
    final style = VitrioText.weighted(VitrioText.section, 800);

    expect(axis(style.fontVariations, 'wght'), 800);
    expect(axis(style.fontVariations, 'opsz'), 19);
  });
}
