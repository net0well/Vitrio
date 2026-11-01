import 'dart:math' as math;

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../app/routes.dart';
import '../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../core/design_system/tokens/vitrio_radius.dart';
import '../../../core/design_system/tokens/vitrio_shadow.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/design_system/widgets/vitrio_avatar.dart';
import '../../../core/design_system/widgets/vitrio_avatar_tone.dart';
import '../../../core/design_system/widgets/vitrio_badge.dart';
import '../../../core/design_system/widgets/vitrio_badge_tone.dart';
import '../../../core/design_system/widgets/vitrio_button.dart';
import '../../../core/design_system/widgets/vitrio_button_variant.dart';
import '../../../core/design_system/widgets/vitrio_icon.dart';
import '../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../core/design_system/widgets/vitrio_star.dart';
import '../../../core/design_system/widgets/vitrio_wordmark.dart';
import '../../../core/presentation/free_for_everyone_line.dart';
import '../../../l10n/app_localizations.dart';
import '../../auth/domain/auth_intent.dart';
import '../../auth/presentation/pending_auth_intent.dart';
import '../../auth/presentation/session_controller.dart';
import '../../location/presentation/location_controller.dart';
import '../../provider/presentation/provider_step.dart';
import 'welcome_controller.dart';

class WelcomeScreen extends ConsumerWidget {
  const WelcomeScreen({super.key});

  Future<void> _findProfessional(BuildContext context, WidgetRef ref) async {
    await ref.read(welcomeControllerProvider.notifier).complete();
    if (context.mounted) {
      const HomeRoute().go(context);
    }
  }

  Future<void> _offerServices(BuildContext context, WidgetRef ref) async {
    await ref.read(welcomeControllerProvider.notifier).complete();
    if (!context.mounted) {
      return;
    }

    final me = ref.read(sessionControllerProvider).value;
    if (me == null) {
      ref.read(pendingAuthIntentProvider.notifier).remember(AuthIntent.offerServices);
      const SignUpRoute().go(context);
    } else if (me.hasProviderProfile) {
      const ProviderDashboardRoute().go(context);
    } else {
      const ProviderOnboardingRoute(step: ProviderStep.about).go(context);
    }
  }

  Future<void> _signIn(BuildContext context, WidgetRef ref) async {
    await ref.read(welcomeControllerProvider.notifier).complete();
    if (context.mounted) {
      ref.read(pendingAuthIntentProvider.notifier).remember(AuthIntent.browse);
      const SignInRoute().go(context);
    }
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final c = context.vitrio;
    final l10n = AppLocalizations.of(context);
    final city = ref.watch(locationControllerProvider).value?.city;
    final signedIn = ref.watch(sessionControllerProvider).value != null;

    return AnnotatedRegion<SystemUiOverlayStyle>(
      value: SystemUiOverlayStyle.light,
      child: Scaffold(
        backgroundColor: c.hero,
        body: SafeArea(
          child: LayoutBuilder(
            builder: (context, constraints) => SingleChildScrollView(
              child: ConstrainedBox(
                constraints: BoxConstraints(minHeight: constraints.maxHeight),
                child: IntrinsicHeight(
                  child: Padding(
                    padding: const EdgeInsets.fromLTRB(VitrioSpace.sp24, VitrioSpace.sp24, VitrioSpace.sp24, VitrioSpace.sp32),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.stretch,
                      children: [
                        SizedBox(
                          height: 40,
                          child: Row(
                            children: [
                              const VitrioWordmark(onDark: true, height: 36),
                              const Spacer(),
                              if (city != null) _CityPill(label: '${city.name}, ${city.state}'),
                            ],
                          ),
                        ),
                        const SizedBox(height: VitrioSpace.sp20),
                        const _Showcase(),
                        const Spacer(),
                        const SizedBox(height: VitrioSpace.sp24),
                        Text(l10n.welcomeTitle, style: VitrioText.displayXl.copyWith(color: c.onHero)),
                        const SizedBox(height: VitrioSpace.sp12),
                        Text(l10n.welcomeMessage, style: VitrioText.body.copyWith(color: c.onHeroMuted)),
                        const SizedBox(height: VitrioSpace.sp14),
                        const FreeForEveryoneLine(),
                        const SizedBox(height: VitrioSpace.sp28),
                        VitrioButton(
                          label: l10n.welcomeFindProfessional,
                          variant: VitrioButtonVariant.accent,
                          trailingIcon: VitrioIconName.arrowRight,
                          onPressed: () => _findProfessional(context, ref),
                        ),
                        const SizedBox(height: VitrioSpace.sp12),
                        VitrioButton(
                          label: l10n.welcomeOfferServices,
                          variant: VitrioButtonVariant.onHero,
                          onPressed: () => _offerServices(context, ref),
                        ),
                        if (!signedIn) ...[
                          const SizedBox(height: VitrioSpace.sp16),
                          Row(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: [
                              Text(l10n.welcomeHaveAccount, style: VitrioText.body.copyWith(color: c.onHeroMuted)),
                              TextButton(
                                onPressed: () => _signIn(context, ref),
                                style: TextButton.styleFrom(
                                  foregroundColor: c.lime,
                                  padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp4),
                                  minimumSize: const Size(VitrioSpace.tap, VitrioSpace.tap),
                                ),
                                child: Text(l10n.authSignIn, style: VitrioText.label.copyWith(color: c.lime)),
                              ),
                            ],
                          ),
                        ],
                      ],
                    ),
                  ),
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}

class _CityPill extends StatelessWidget {
  const _CityPill({required this.label});

  final String label;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    return Container(
      height: 32,
      padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp12),
      decoration: BoxDecoration(color: c.heroGlass, borderRadius: const BorderRadius.all(VitrioRadius.full)),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          VitrioIcon(VitrioIconName.pin, size: 15, color: c.lime),
          const SizedBox(width: VitrioSpace.sp6),
          Text(label, style: VitrioText.weighted(VitrioText.meta, 700).copyWith(color: c.onHero)),
        ],
      ),
    );
  }
}

class _Showcase extends StatelessWidget {
  const _Showcase();

  static const _width = 342.0;
  static const _height = 300.0;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    final l10n = AppLocalizations.of(context);
    double degrees(double value) => value * math.pi / 180;
    return ExcludeSemantics(
      child: SizedBox(
        height: _height,
        child: FittedBox(
          fit: BoxFit.scaleDown,
          child: SizedBox(
            width: _width,
            height: _height,
            child: Stack(
              clipBehavior: Clip.none,
              children: [
                Positioned(
                  left: 2,
                  top: 34,
                  width: 214,
                  child: Transform.rotate(
                    angle: degrees(-6),
                    child: _SmallCard(
                      name: l10n.welcomeSampleAnaName,
                      role: l10n.welcomeSampleAnaRole,
                      rating: l10n.welcomeSampleAnaRating,
                      tone: VitrioAvatarTone.amber,
                    ),
                  ),
                ),
                Positioned(
                  right: 0,
                  top: 0,
                  width: 196,
                  child: Transform.rotate(
                    angle: degrees(5),
                    child: _SmallCard(
                      name: l10n.welcomeSampleJoseName,
                      role: l10n.welcomeSampleJoseRole,
                      rating: l10n.welcomeSampleJoseRating,
                      tone: VitrioAvatarTone.green,
                    ),
                  ),
                ),
                Positioned(
                  left: 20,
                  top: 118,
                  width: 302,
                  child: Container(
                    padding: const EdgeInsets.all(VitrioSpace.sp16),
                    decoration: BoxDecoration(
                      color: c.surfaceRaised,
                      borderRadius: const BorderRadius.all(VitrioRadius.hero),
                      boxShadow: VitrioShadow.lg(context),
                    ),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          children: [
                            VitrioAvatar(name: l10n.welcomeSampleCarlosName, size: 52, tone: VitrioAvatarTone.plum),
                            const SizedBox(width: VitrioSpace.sp12),
                            Expanded(
                              child: Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: [
                                  Row(
                                    children: [
                                      Flexible(child: Text(l10n.welcomeSampleCarlosName, overflow: TextOverflow.ellipsis, style: VitrioText.labelLg.copyWith(color: c.ink))),
                                      const SizedBox(width: VitrioSpace.sp6),
                                      VitrioIcon(VitrioIconName.shieldCheck, size: 16, color: c.green),
                                    ],
                                  ),
                                  const SizedBox(height: 2),
                                  Text(l10n.welcomeSampleCarlosRole, style: VitrioText.meta.copyWith(color: c.inkMuted)),
                                ],
                              ),
                            ),
                          ],
                        ),
                        const SizedBox(height: VitrioSpace.sp12),
                        Row(
                          children: [
                            const VitrioStar(),
                            const SizedBox(width: VitrioSpace.sp6),
                            Text(l10n.welcomeSampleCarlosRating, style: VitrioText.weighted(VitrioText.meta, 800).copyWith(color: c.ink)),
                            const SizedBox(width: VitrioSpace.sp6),
                            Flexible(child: Text(l10n.welcomeSampleCarlosMeta, overflow: TextOverflow.ellipsis, style: VitrioText.weighted(VitrioText.meta, 400).copyWith(color: c.inkMuted))),
                          ],
                        ),
                        const SizedBox(height: VitrioSpace.sp12),
                        Wrap(
                          spacing: VitrioSpace.sp8,
                          runSpacing: VitrioSpace.sp8,
                          children: [
                            VitrioBadge(label: l10n.providerBadgeMei, tone: VitrioBadgeTone.verified),
                            VitrioBadge(label: l10n.providerBadgeToday),
                          ],
                        ),
                      ],
                    ),
                  ),
                ),
                Positioned(
                  right: 6,
                  top: 220,
                  child: Transform.rotate(
                    angle: degrees(10),
                    child: Container(
                      width: 88,
                      height: 88,
                      alignment: Alignment.center,
                      decoration: BoxDecoration(color: c.lime, shape: BoxShape.circle, boxShadow: VitrioShadow.lg(context)),
                      child: Text(
                        l10n.welcomeNearYou,
                        textAlign: TextAlign.center,
                        style: VitrioText.caption.copyWith(color: c.onLime, height: 1.15),
                      ),
                    ),
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

class _SmallCard extends StatelessWidget {
  const _SmallCard({required this.name, required this.role, required this.rating, required this.tone});

  final String name;
  final String role;
  final String rating;
  final VitrioAvatarTone tone;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    return Container(
      padding: const EdgeInsets.all(VitrioSpace.sp14),
      decoration: BoxDecoration(
        color: c.surfaceRaised,
        borderRadius: const BorderRadius.all(VitrioRadius.card),
        boxShadow: VitrioShadow.lg(context),
      ),
      child: Row(
        children: [
          VitrioAvatar(name: name, tone: tone),
          const SizedBox(width: VitrioSpace.sp12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(name, style: VitrioText.label.copyWith(color: c.ink)),
                const SizedBox(height: 2),
                Text(role, style: VitrioText.weighted(VitrioText.caption, 600).copyWith(color: c.inkMuted)),
                const SizedBox(height: 2),
                Row(
                  children: [
                    const VitrioStar(size: 13),
                    const SizedBox(width: VitrioSpace.sp4),
                    Text(rating, style: VitrioText.caption.copyWith(color: c.ink)),
                  ],
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
