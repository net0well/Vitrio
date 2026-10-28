# Implementação em Flutter

O app é Flutter (Material 3). Os tokens deste sistema viram quatro classes (`VitrioColors`, `VitrioText`, `VitrioSpace`, `VitrioRadius`) e um `ThemeData`. Os componentes viram widgets próprios em `lib/core/design_system/`. Nenhum widget usa cor, tamanho ou raio literal: tudo vem dessas classes.

## Estrutura

```text
lib/core/design_system/
  tokens/
    vitrio_colors.dart
    vitrio_text.dart
    vitrio_space.dart
    vitrio_radius.dart
  vitrio_theme.dart
  widgets/
    vitrio_button.dart
    vitrio_chip.dart
    vitrio_badge.dart
    vitrio_avatar.dart
    vitrio_rating.dart
    provider_card.dart
    review_card.dart
    order_timeline.dart
    quote_card.dart
    payment_method_picker.dart
    payment_receipt_card.dart
    action_bar.dart
assets/
  fonts/
    BricolageGrotesque-Variable.ttf
    PlusJakartaSans-Variable.ttf
  icons/
    search.svg ...
  brand/
    vitrio-wordmark.svg ...
```

## Fontes

O Flutter não lê `.woff2`. Os arquivos `.ttf` já estão em `fonts/` (mesmo conteúdo dos `.woff2`, licença OFL): copie `fonts/*.ttf` para `mobile/assets/fonts/`, os SVGs de `assets/icons/` para `mobile/assets/icons/` e os de `assets/logos/` para `mobile/assets/brand/`, e registre no `pubspec.yaml`. Os PNGs `vitrio-app-icon-*.png` e `vitrio-app-icon-adaptive-1024.png` alimentam o `flutter_launcher_icons`.

```yaml
flutter:
  uses-material-design: true
  assets:
    - assets/icons/
    - assets/brand/
  fonts:
    - family: BricolageGrotesque
      fonts:
        - asset: assets/fonts/BricolageGrotesque-Variable.ttf
    - family: PlusJakartaSans
      fonts:
        - asset: assets/fonts/PlusJakartaSans-Variable.ttf
```

Os arquivos são o subconjunto latino, que cobre todo o português. Em fontes variáveis o peso precisa do eixo `wght`, por isso cada estilo define `fontWeight` e `fontVariations`.

## Cores

```dart
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
```

## Espaço e raios

```dart
import 'package:flutter/widgets.dart';

abstract final class VitrioSpace {
  static const sp4 = 4.0;
  static const sp6 = 6.0;
  static const sp8 = 8.0;
  static const sp10 = 10.0;
  static const sp12 = 12.0;
  static const sp14 = 14.0;
  static const sp16 = 16.0;
  static const sp20 = 20.0;
  static const sp24 = 24.0;
  static const sp28 = 28.0;
  static const sp32 = 32.0;
  static const tap = 44.0;
  static const controlLg = 54.0;
  static const cta = 56.0;
  static const chip = 38.0;
  static const badge = 26.0;
}

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
```

## Tipografia

```dart
import 'package:flutter/material.dart';

import 'vitrio_colors.dart';

abstract final class VitrioText {
  static const displayFamily = 'BricolageGrotesque';
  static const sansFamily = 'PlusJakartaSans';

  static TextStyle _style(String family, double size, double lineHeight, int weight, [double spacing = 0]) {
    return TextStyle(
      fontFamily: family,
      fontSize: size,
      height: lineHeight / size,
      fontWeight: FontWeight.values[weight ~/ 100 - 1],
      fontVariations: [FontVariation('wght', weight.toDouble())],
      letterSpacing: spacing,
    );
  }

  static final displayXl = _style(displayFamily, 34, 37, 800, -0.8);
  static final display = _style(displayFamily, 30, 33, 700, -0.7);
  static final title = _style(displayFamily, 22, 26, 700, -0.4);
  static final section = _style(displayFamily, 19, 24, 700, -0.3);
  static final priceLg = _style(displayFamily, 36, 40, 800, -1);
  static final price = _style(displayFamily, 18, 22, 700);
  static final labelLg = _style(sansFamily, 16, 22, 800);
  static final bodyStrong = _style(sansFamily, 15, 20, 800);
  static final label = _style(sansFamily, 14, 18, 800);
  static final body = _style(sansFamily, 14, 21, 400);
  static final meta = _style(sansFamily, 13, 18, 600);
  static final caption = _style(sansFamily, 12, 16, 800);
  static final micro = _style(sansFamily, 11, 14, 700);

  static TextTheme textTheme(VitrioColors c) {
    return TextTheme(
      displayLarge: displayXl.copyWith(color: c.ink),
      displayMedium: display.copyWith(color: c.ink),
      headlineSmall: title.copyWith(color: c.ink),
      titleLarge: section.copyWith(color: c.ink),
      titleMedium: bodyStrong.copyWith(color: c.ink),
      titleSmall: label.copyWith(color: c.ink),
      bodyLarge: body.copyWith(color: c.ink2),
      bodyMedium: body.copyWith(color: c.ink2),
      bodySmall: meta.copyWith(color: c.inkMuted),
      labelLarge: label.copyWith(color: c.ink),
      labelMedium: caption.copyWith(color: c.ink),
      labelSmall: micro.copyWith(color: c.inkMuted),
    );
  }
}
```

## Tema

```dart
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
```

## Exemplo de widget

```dart
import 'package:flutter/material.dart';

import '../tokens/vitrio_colors.dart';
import '../tokens/vitrio_space.dart';
import '../tokens/vitrio_text.dart';

enum VitrioBadgeTone { verified, brand, neutral, warning, accent, danger, info }

class VitrioBadge extends StatelessWidget {
  const VitrioBadge({super.key, required this.label, this.tone = VitrioBadgeTone.brand, this.icon});

  final String label;
  final VitrioBadgeTone tone;
  final Widget? icon;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    final (background, foreground) = switch (tone) {
      VitrioBadgeTone.verified => (c.greenSoft, c.green),
      VitrioBadgeTone.brand => (c.plumSoft, c.plum),
      VitrioBadgeTone.neutral => (c.surfaceInset, c.ink2),
      VitrioBadgeTone.warning => (c.amberSoft, c.amberInk),
      VitrioBadgeTone.accent => (c.lime, c.onLime),
      VitrioBadgeTone.danger => (c.dangerSoft, c.danger),
      VitrioBadgeTone.info => (c.infoSoft, c.info),
    };
    return Container(
      height: VitrioSpace.badge,
      padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp10),
      decoration: ShapeDecoration(color: background, shape: const StadiumBorder()),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          if (icon != null) ...[
            IconTheme(data: IconThemeData(size: 14, color: foreground), child: icon!),
            const SizedBox(width: VitrioSpace.sp6),
          ],
          Text(label, style: VitrioText.caption.copyWith(color: foreground)),
        ],
      ),
    );
  }
}
```

## Ícones

Use os SVGs de `assets/Icons/` com `flutter_svg` e recolora com `ColorFilter.mode(cor, BlendMode.srcIn)`. Tamanho padrão 20; 14–18 em badges e metadados (traço visual mais grosso, como no web). A estrela de nota é desenhada preenchida com `amber` e contorno `amberEdge`.

## Componentes → widgets

| Componente do sistema | Widget no app | Observação |
| --- | --- | --- |
| `Button` | `FilledButton` (primary), `OutlinedButton` (secondary), `TextButton` (ghost), `VitrioButton` para accent e on-hero | Temas acima; 56px na barra de ação |
| `IconButton` | `IconButton.outlined` / `IconButton.filledTonal` com `minimumSize` 44 | Sempre com `tooltip` (vira rótulo acessível) |
| `Chip` | `VitrioChip` | `Semantics(selected:)`, `AnimatedContainer` 180ms |
| `Badge` | `VitrioBadge` | Exemplo acima |
| `Avatar` | `VitrioAvatar(kind: provider/client)` | Prestador: `VitrioRadius.providerAvatar(size)`; cliente: círculo |
| `Rating` / `RatingInput` | `VitrioRating` / `VitrioRatingInput` | Semântica "Nota 4,9 de 5" |
| `SearchField` | `VitrioSearchField` | `TextField` + botão de envio de 40px |
| `TextField` / `TextArea` | `TextFormField` com `inputDecorationTheme` | `maxLength` mostra o contador |
| `SegmentedControl` | `VitrioSegmented` | `SegmentedButton` não tem o visual de trilho |
| `Switch` | `SwitchListTile` com `switchTheme` | Linha inteira tocável |
| `CategoryTile` | `CategoryTile` | Grade de 4 colunas |
| `ProviderCard` | `ProviderCard` | Toda a área é tocável (`InkWell`) |
| `ReviewCard` | `ReviewCard` | Resposta do prestador em `surfaceInset` |
| `StatusTimeline` | `OrderTimeline` | Recebe a lista de etapas do pedido |
| `QuoteCard` | `QuoteCard` | Único card com `shadow-md` na tela |
| `PaymentMethodPicker` | `PaymentMethodPicker` | `Semantics(inMutuallyExclusiveGroup: true)` |
| `PaymentReceipt` | `PaymentReceiptCard` | Borda 1.5 `green` |
| `BottomNav` | `NavigationBar` | `navigationBarTheme` acima |
| `TopBar` | `AppBar(centerTitle: true)` | Título em `label` 17px |
| `ActionBar` | `bottomNavigationBar` com `SafeArea` | Nunca junto com `NavigationBar` |
| `HeroCard` | `HeroCard` | Fundo `hero`, ícone decorativo a 30% |
