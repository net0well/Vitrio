import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:share_plus/share_plus.dart';
import 'package:url_launcher/url_launcher.dart';

import '../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../core/design_system/tokens/vitrio_radius.dart';
import '../../../core/design_system/tokens/vitrio_space.dart';
import '../../../core/design_system/tokens/vitrio_text.dart';
import '../../../core/design_system/widgets/vitrio_avatar.dart';
import '../../../core/design_system/widgets/vitrio_badge.dart';
import '../../../core/design_system/widgets/vitrio_badge_tone.dart';
import '../../../core/design_system/widgets/vitrio_chip.dart';
import '../../../core/design_system/widgets/vitrio_chip_style.dart';
import '../../../core/design_system/widgets/vitrio_empty_state.dart';
import '../../../core/design_system/widgets/vitrio_icon.dart';
import '../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../l10n/app_localizations.dart';
import '../domain/public_provider.dart';
import 'provider_labels.dart';
import 'provider_queries.dart';

class PublicProviderScreen extends ConsumerWidget {
  const PublicProviderScreen({super.key, required this.providerId});

  final String providerId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final provider = ref.watch(publicProviderProvider(providerId));
    final loaded = provider.value;
    return Scaffold(
      appBar: AppBar(
        actions: [
          if (loaded != null)
            IconButton(
              tooltip: l10n.providerShare,
              icon: const VitrioIcon(VitrioIconName.share),
              onPressed: () => SharePlus.instance.share(ShareParams(text: l10n.providerShareText(loaded.shareUrl))),
            ),
        ],
      ),
      body: switch (provider) {
        AsyncData(value: final PublicProvider value) => _Profile(provider: value),
        AsyncData() => Center(child: VitrioEmptyState(icon: VitrioIconName.user, message: l10n.providerNotFound)),
        AsyncError() => Center(
            child: TextButton(onPressed: () => ref.invalidate(publicProviderProvider(providerId)), child: Text(l10n.commonRetry)),
          ),
        _ => const Center(child: CircularProgressIndicator()),
      },
      bottomNavigationBar: loaded?.whatsAppPhone == null
          ? null
          : SafeArea(
              child: Padding(
                padding: const EdgeInsets.fromLTRB(VitrioSpace.sp20, VitrioSpace.sp8, VitrioSpace.sp20, VitrioSpace.sp16),
                child: SizedBox(
                  height: VitrioSpace.cta,
                  child: FilledButton.icon(
                    icon: const VitrioIcon(VitrioIconName.message),
                    label: Text(l10n.providerWhatsAppAction),
                    onPressed: () => launchUrl(
                      Uri.parse('https://wa.me/${loaded!.whatsAppPhone!.replaceAll(RegExp(r'\D'), '')}'),
                      mode: LaunchMode.externalApplication,
                    ),
                  ),
                ),
              ),
            ),
    );
  }
}

class _Profile extends StatelessWidget {
  const _Profile({required this.provider});

  final PublicProvider provider;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final theme = Theme.of(context);
    return ListView(
      padding: const EdgeInsets.fromLTRB(VitrioSpace.sp20, 0, VitrioSpace.sp20, VitrioSpace.sp32),
      children: [
        Center(child: VitrioAvatar(name: provider.displayName, imageUrl: provider.avatarUrl, size: 96)),
        const SizedBox(height: VitrioSpace.sp16),
        Text(provider.displayName, style: VitrioText.title, textAlign: TextAlign.center),
        Text(provider.headline, style: theme.textTheme.bodyLarge, textAlign: TextAlign.center),
        const SizedBox(height: VitrioSpace.sp4),
        Text(
          l10n.providerServes('${provider.city.name}, ${provider.city.state}', provider.radiusKm),
          style: theme.textTheme.bodySmall,
          textAlign: TextAlign.center,
        ),
        const SizedBox(height: VitrioSpace.sp12),
        Wrap(
          alignment: WrapAlignment.center,
          spacing: VitrioSpace.sp8,
          runSpacing: VitrioSpace.sp8,
          children: [
            if (provider.meiVerified)
              VitrioBadge(label: l10n.providerBadgeMei, tone: VitrioBadgeTone.verified, icon: const VitrioIcon(VitrioIconName.shieldCheck)),
            if (provider.availableToday) VitrioBadge(label: l10n.providerBadgeToday, tone: VitrioBadgeTone.accent),
            if (provider.handlesEmergencies) VitrioBadge(label: l10n.providerEmergencies, tone: VitrioBadgeTone.warning),
            if (provider.isNew)
              VitrioBadge(label: l10n.providerNew)
            else if (provider.ratingScore case final score?)
              VitrioBadge(
                label: l10n.providerRating(score.toStringAsFixed(1).replaceAll('.', ','), provider.reviewCount ?? 0),
                icon: const VitrioIcon(VitrioIconName.star),
              ),
          ],
        ),
        if (provider.about case final about?) ...[
          _Title(l10n.providerAboutTitle),
          Text(about, style: theme.textTheme.bodyMedium),
          if (provider.yearsOfExperience case final years?)
            Padding(
              padding: const EdgeInsets.only(top: VitrioSpace.sp8),
              child: Text(l10n.providerYearsExperience(years), style: theme.textTheme.bodySmall),
            ),
        ],
        if (provider.services.isNotEmpty) ...[
          _Title(l10n.providerServices),
          for (final service in provider.services)
            ListTile(
              contentPadding: EdgeInsets.zero,
              title: Text(service.title),
              subtitle: service.description == null ? null : Text(service.description!),
              trailing: Text(priceText(l10n, service), style: VitrioText.label.copyWith(color: c.plum)),
            ),
        ],
        if (provider.photos.isNotEmpty) ...[
          _Title(l10n.providerWorks),
          GridView.count(
            crossAxisCount: 3,
            shrinkWrap: true,
            physics: const NeverScrollableScrollPhysics(),
            mainAxisSpacing: VitrioSpace.sp8,
            crossAxisSpacing: VitrioSpace.sp8,
            children: [
              for (final photo in provider.photos)
                ClipRRect(
                  borderRadius: const BorderRadius.all(VitrioRadius.lg),
                  child: Semantics(
                    image: true,
                    label: photo.caption,
                    child: Image.network(photo.thumbnailUrl, fit: BoxFit.cover, errorBuilder: (_, _, _) => ColoredBox(color: c.surfaceInset)),
                  ),
                ),
            ],
          ),
        ],
        if (provider.availability.isNotEmpty) ...[
          _Title(l10n.providerHours),
          for (final slot in provider.availability) Text('${dayLabel(l10n, slot.dayOfWeek)}: ${slot.start} – ${slot.end}'),
        ],
        if (provider.neighborhoods.isNotEmpty) ...[
          _Title(l10n.providerNeighborhoods),
          Wrap(
            spacing: VitrioSpace.sp8,
            runSpacing: VitrioSpace.sp8,
            children: [
              for (final name in provider.neighborhoods) VitrioChip(label: name, selected: false, style: VitrioChipStyle.soft),
            ],
          ),
        ],
      ],
    );
  }
}

class _Title extends StatelessWidget {
  const _Title(this.text);

  final String text;

  @override
  Widget build(BuildContext context) => Padding(
        padding: const EdgeInsets.only(top: VitrioSpace.sp24, bottom: VitrioSpace.sp8),
        child: Text(text, style: Theme.of(context).textTheme.titleMedium),
      );
}
