import 'package:flutter/material.dart';

import '../../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../../core/design_system/tokens/vitrio_radius.dart';
import '../../../../core/design_system/tokens/vitrio_space.dart';
import '../../../../core/design_system/tokens/vitrio_text.dart';
import '../../../../core/design_system/widgets/vitrio_button.dart';
import '../../../../core/design_system/widgets/vitrio_button_size.dart';
import '../../../../core/design_system/widgets/vitrio_button_variant.dart';
import '../../../../core/design_system/widgets/vitrio_icon.dart';
import '../../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../../l10n/app_localizations.dart';
import '../../../orders/domain/order_action.dart';
import '../../../orders/presentation/order_labels.dart';
import '../../domain/provider_order.dart';
import '../provider_order_labels.dart';

class ScheduledServiceCard extends StatelessWidget {
  const ScheduledServiceCard({
    super.key,
    required this.order,
    required this.now,
    required this.busy,
    required this.onReschedule,
    required this.onComplete,
  });

  static const _check = 32.0;
  static const _rescheduleWidth = 150.0;

  final ProviderOrder order;
  final DateTime now;
  final bool busy;
  final VoidCallback onReschedule;
  final VoidCallback onComplete;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final schedule = order.schedule!;
    final quote = order.quote;
    final canComplete = order.allows(OrderAction.complete);
    final details = quote?.materialDetails;
    final summary = [
      if (quote != null) l10n.providerAgreedValue(formatQuoteAmount(quote.amount)),
      if (quote != null && quote.materialIncluded)
        details == null ? l10n.providerMaterialIncludedPlain : l10n.providerMaterialIncluded(details),
    ].join(' ');
    return Container(
      padding: const EdgeInsets.all(VitrioSpace.sp20),
      decoration: BoxDecoration(
        color: c.surfaceRaised,
        borderRadius: const BorderRadius.all(VitrioRadius.hero),
        border: Border.all(color: c.green, width: 1.5),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            children: [
              Container(
                width: _check,
                height: _check,
                alignment: Alignment.center,
                decoration: BoxDecoration(color: c.greenSoft, shape: BoxShape.circle),
                child: VitrioIcon(VitrioIconName.check, size: 16, color: c.green, strokeWidth: 3),
              ),
              const SizedBox(width: VitrioSpace.sp10),
              Text(l10n.orderScheduledTitle, style: VitrioText.bodyStrong.copyWith(color: c.green)),
            ],
          ),
          const SizedBox(height: VitrioSpace.sp12),
          Text(
            l10n.orderScheduledWhen(dayOption(l10n, schedule.date, now), serviceRange(schedule)),
            style: VitrioText.title.copyWith(color: c.ink),
          ),
          if (summary.isNotEmpty) ...[
            const SizedBox(height: VitrioSpace.sp8),
            Text(summary, style: VitrioText.body.copyWith(height: 1.5, color: c.ink2)),
          ],
          const SizedBox(height: VitrioSpace.sp16),
          Row(
            children: [
              if (order.allows(OrderAction.reschedule)) ...[
                SizedBox(
                  width: _rescheduleWidth,
                  child: VitrioButton(
                    label: l10n.providerReschedule,
                    variant: VitrioButtonVariant.secondary,
                    size: VitrioButtonSize.decision,
                    onPressed: busy ? null : onReschedule,
                  ),
                ),
                const SizedBox(width: VitrioSpace.sp10),
              ],
              Expanded(
                child: VitrioButton(
                  label: l10n.orderComplete,
                  size: VitrioButtonSize.decision,
                  busy: busy,
                  onPressed: canComplete ? onComplete : null,
                ),
              ),
            ],
          ),
          if (!canComplete) ...[
            const SizedBox(height: VitrioSpace.sp10),
            Text(
              l10n.providerCompleteFrom(schedule.start, shortDate(l10n, schedule.date)),
              style: VitrioText.weighted(VitrioText.caption, 600).copyWith(height: 1.5, color: c.inkMuted),
            ),
          ],
        ],
      ),
    );
  }
}
