import 'package:flutter/material.dart';

import '../../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../../core/design_system/tokens/vitrio_space.dart';
import '../../../../core/design_system/tokens/vitrio_text.dart';
import '../../../../core/design_system/widgets/vitrio_badge.dart';
import '../../../../core/design_system/widgets/vitrio_badge_tone.dart';
import '../../../../l10n/app_localizations.dart';
import '../../domain/provider_service.dart';
import '../provider_labels.dart';

class ServicePriceRow extends StatelessWidget {
  const ServicePriceRow({super.key, required this.service, this.onTap});

  final ProviderService service;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final price = priceValue(l10n, service);
    return MergeSemantics(
      child: InkWell(
        onTap: onTap,
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: VitrioSpace.sp16, vertical: VitrioSpace.sp14),
          child: Row(
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(service.title, style: VitrioText.bodyStrong.copyWith(color: c.ink)),
                    if (service.description case final description?) ...[
                      const SizedBox(height: 2),
                      Text(description, style: VitrioText.weighted(VitrioText.caption, 400).copyWith(color: c.inkMuted)),
                    ],
                  ],
                ),
              ),
              const SizedBox(width: VitrioSpace.sp12),
              if (price == null)
                VitrioBadge(label: l10n.providerServiceOnQuote, tone: VitrioBadgeTone.neutral)
              else
                Column(
                  crossAxisAlignment: CrossAxisAlignment.end,
                  children: [
                    Text(l10n.providerFromLabel, style: VitrioText.weighted(VitrioText.micro, 600).copyWith(color: c.inkMuted)),
                    Text(price, style: VitrioText.bodyStrong.copyWith(color: c.ink)),
                  ],
                ),
            ],
          ),
        ),
      ),
    );
  }
}
