import { copyFile, mkdir, readFile, writeFile } from 'node:fs/promises';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { Resvg } from '@resvg/resvg-js';
import * as fontkit from 'fontkit';

const root = resolve(dirname(fileURLToPath(import.meta.url)), '../..');
const logos = 'docs/design-system/assets/logos';
const store = 'docs/design-system/assets/store';
const fontFiles = {
  'Bricolage Grotesque': 'mobile/assets/fonts/BricolageGrotesque-Variable.ttf',
  'Plus Jakarta Sans': 'mobile/assets/fonts/PlusJakartaSans-Variable.ttf',
};

const renders = [
  { source: `${logos}/vitrio-app-icon.svg`, width: 1024, targets: [`${logos}/vitrio-app-icon-1024.png`, 'mobile/assets/launcher/vitrio-app-icon-1024.png'] },
  { source: `${logos}/vitrio-app-icon.svg`, width: 512, targets: [`${logos}/vitrio-app-icon-512.png`, `${store}/google-play-icon-512.png`] },
  { source: `${logos}/vitrio-app-icon.svg`, width: 192, targets: [`${logos}/vitrio-app-icon-192.png`] },
  {
    source: `${logos}/vitrio-app-icon-adaptive.svg`,
    width: 1024,
    targets: [`${logos}/vitrio-app-icon-adaptive-1024.png`, 'mobile/assets/launcher/vitrio-app-icon-adaptive-1024.png'],
  },
  {
    source: `${logos}/vitrio-app-icon-monochrome.svg`,
    width: 1024,
    targets: [`${logos}/vitrio-app-icon-monochrome-1024.png`, 'mobile/assets/launcher/vitrio-app-icon-monochrome-1024.png'],
  },
  {
    source: `${logos}/vitrio-wordmark-on-dark.svg`,
    width: 400,
    targets: [`${logos}/vitrio-wordmark-on-dark@2x.png`, 'backend/src/Vitrio.Infrastructure/Email/Templates/vitrio-wordmark.png'],
  },
  { source: `${logos}/vitrio-mark.svg`, width: 288, targets: ['mobile/android/app/src/main/res/drawable-xxxhdpi/launch_mark.png'] },
  { source: `${store}/google-play-feature.svg`, width: 1024, targets: [`${store}/google-play-feature-1024x500.png`] },
];

const brandFiles = [
  'vitrio-mark.svg',
  'vitrio-mark-on-dark.svg',
  'vitrio-mark-mono.svg',
  'vitrio-wordmark.svg',
  'vitrio-wordmark-on-dark.svg',
  'vitrio-wordmark-mono.svg',
];

// O resvg ignora o peso das fontes variáveis; o texto vira contorno com o peso pedido antes de desenhar.
function fontFor(family, weight, size) {
  const font = fontkit.openSync(join(root, fontFiles[family]));
  const opsz = font.variationAxes.opsz;
  return font.getVariation({ wght: weight, ...(opsz ? { opsz: Math.min(opsz.max, Math.max(opsz.min, size)) } : {}) });
}

function outline(attributes, value) {
  const size = Number(attributes['font-size']);
  const font = fontFor(attributes['font-family'], Number(attributes['font-weight']), size);
  const scale = size / font.unitsPerEm;
  const spacing = Number(attributes['letter-spacing'] ?? 0);
  const run = font.layout(value);
  const width = run.positions.reduce((sum, position) => sum + position.xAdvance * scale + spacing, 0) - spacing;
  let x = Number(attributes.x) - (attributes['text-anchor'] === 'middle' ? width / 2 : 0);
  const y = Number(attributes.y);
  const glyphs = run.glyphs.map((glyph, index) => {
    const position = run.positions[index];
    const left = x + position.xOffset * scale;
    x += position.xAdvance * scale + spacing;
    const path = glyph.path.toSVG();
    return path ? `<path transform="translate(${left.toFixed(2)} ${(y - position.yOffset * scale).toFixed(2)}) scale(${scale} ${-scale})" d="${path}"/>` : '';
  });
  return `<g fill="${attributes.fill}">${glyphs.join('')}</g>`;
}

function outlineTexts(svg) {
  return svg.replace(/<text ([^>]*)>([^<]*)<\/text>/g, (_, attributeText, value) => {
    const attributes = Object.fromEntries([...attributeText.matchAll(/([\w-]+)="([^"]*)"/g)].map(([, name, attribute]) => [name, attribute]));
    return outline(attributes, value.replaceAll('&lt;', '<').replaceAll('&gt;', '>').replaceAll('&amp;', '&'));
  });
}

for (const { source, width, targets } of renders) {
  const svg = outlineTexts(await readFile(join(root, source), 'utf8'));
  const png = new Resvg(svg, { fitTo: { mode: 'width', value: width }, font: { loadSystemFonts: false } })
    .render()
    .asPng();
  for (const target of targets) {
    await mkdir(dirname(join(root, target)), { recursive: true });
    await writeFile(join(root, target), png);
    console.log(`${source} -> ${target} (${width}px)`);
  }
}

for (const file of brandFiles) {
  await copyFile(join(root, logos, file), join(root, 'mobile/assets/brand', file));
  console.log(`${logos}/${file} -> mobile/assets/brand/${file}`);
}
