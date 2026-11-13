import 'package:flutter/material.dart';
import 'package:url_launcher/url_launcher.dart';

import '../../../../core/design_system/tokens/vitrio_colors.dart';
import '../../../../core/design_system/tokens/vitrio_radius.dart';
import '../../../../core/design_system/tokens/vitrio_space.dart';
import '../../../../core/design_system/tokens/vitrio_text.dart';
import '../../../../core/design_system/widgets/vitrio_avatar.dart';
import '../../../../core/design_system/widgets/vitrio_avatar_kind.dart';
import '../../../../core/design_system/widgets/vitrio_avatar_tone.dart';
import '../../../../core/design_system/widgets/vitrio_button.dart';
import '../../../../core/design_system/widgets/vitrio_button_size.dart';
import '../../../../core/design_system/widgets/vitrio_button_variant.dart';
import '../../../../core/design_system/widgets/vitrio_icon.dart';
import '../../../../core/design_system/widgets/vitrio_icon_name.dart';
import '../../../../core/presentation/brazilian_format.dart';
import '../../../../l10n/app_localizations.dart';
import '../../../orders/presentation/order_labels.dart';
import '../../domain/order_address.dart';
import '../../domain/provider_order.dart';
import 'order_section_label.dart';

class OrderContactSections extends StatelessWidget {
  const OrderContactSections({super.key, required this.order});

  static const _avatar = 40.0;
  static const _brazil = '55';

  final ProviderOrder order;

  static String _digits(String phone) => phone.replaceAll(RegExp(r'\D'), '');

  static String _international(String phone) {
    final digits = _digits(phone);
    return digits.length <= 11 ? '$_brazil$digits' : digits;
  }

  static String _streetLine(OrderAddress address) => [
    address.street,
    address.number,
    if (address.complement case final String complement when complement.isNotEmpty) complement,
  ].join(', ');

  static String _cityLine(OrderAddress address) => '${address.neighborhood}, ${address.cityName}, ${address.state} · ${cepText(address.cep)}';

  static Future<void> _open(Uri uri) => launchUrl(uri, mode: LaunchMode.externalApplication);

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);
    final c = context.vitrio;
    final customer = order.customer;
    final phone = customer.phone;
    final address = order.address;
    final visibleUntil = order.contactVisibleUntil;
    final decoration = BoxDecoration(
      color: c.surfaceRaised,
      borderRadius: const BorderRadius.all(VitrioRadius.card),
      border: Border.all(color: c.lineSoft),
    );

    if (address == null && phone == null) {
      return Padding(
        padding: const EdgeInsets.only(top: 22),
        child: Text(
          l10n.providerContactHidden,
          style: VitrioText.weighted(VitrioText.caption, 600).copyWith(height: 1.5, color: c.inkMuted),
        ),
      );
    }

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        const SizedBox(height: 22),
        OrderSectionLabel(l10n.providerCustomerSection),
        const SizedBox(height: VitrioSpace.sp10),
        Container(
          padding: const EdgeInsets.all(VitrioSpace.sp16),
          decoration: decoration,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Row(
                children: [
                  VitrioAvatar(name: customer.name, size: _avatar, kind: VitrioAvatarKind.client, tone: VitrioAvatarTone.info),
                  const SizedBox(width: VitrioSpace.sp12),
                  Expanded(
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(customer.name, style: VitrioText.bodyStrong.copyWith(color: c.ink)),
                        if (phone != null) ...[const SizedBox(height: 2), Text(phoneText(phone), style: VitrioText.meta.copyWith(color: c.inkMuted))],
                      ],
                    ),
                  ),
                ],
              ),
              if (phone != null) ...[
                const SizedBox(height: VitrioSpace.sp14),
                Row(
                  children: [
                    Expanded(
                      child: VitrioButton(
                        label: l10n.providerCall,
                        variant: VitrioButtonVariant.secondary,
                        size: VitrioButtonSize.md,
                        leadingIcon: VitrioIconName.phone,
                        onPressed: () => _open(Uri(scheme: 'tel', path: '+${_international(phone)}')),
                      ),
                    ),
                    const SizedBox(width: VitrioSpace.sp10),
                    Expanded(
                      child: VitrioButton(
                        label: l10n.providerWhatsAppAction,
                        variant: VitrioButtonVariant.secondary,
                        size: VitrioButtonSize.md,
                        leadingIcon: VitrioIconName.message,
                        onPressed: () => _open(Uri.parse('https://wa.me/${_international(phone)}')),
                      ),
                    ),
                  ],
                ),
              ],
            ],
          ),
        ),
        if (address != null) ...[
          const SizedBox(height: 22),
          OrderSectionLabel(l10n.providerAddressSection),
          const SizedBox(height: VitrioSpace.sp10),
          Container(
            padding: const EdgeInsets.all(VitrioSpace.sp16),
            decoration: decoration,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                Text(_streetLine(address), style: VitrioText.bodyStrong.copyWith(color: c.ink)),
                const SizedBox(height: 2),
                Text(_cityLine(address), style: VitrioText.meta.copyWith(color: c.inkMuted)),
                const SizedBox(height: VitrioSpace.sp14),
                VitrioButton(
                  label: l10n.providerOpenMap,
                  variant: VitrioButtonVariant.secondary,
                  size: VitrioButtonSize.md,
                  leadingIcon: VitrioIconName.pin,
                  onPressed: () => _open(
                    Uri.https('www.google.com', '/maps/search/', {'api': '1', 'query': '${_streetLine(address)}, ${_cityLine(address)}'}),
                  ),
                ),
                if (visibleUntil != null) ...[
                  const SizedBox(height: VitrioSpace.sp12),
                  Row(
                    children: [
                      VitrioIcon(VitrioIconName.lock, size: 14, color: c.inkMuted),
                      const SizedBox(width: VitrioSpace.sp6),
                      Expanded(
                        child: Text(
                          l10n.providerContactVisibleUntil(shortDate(l10n, visibleUntil)),
                          style: VitrioText.weighted(VitrioText.caption, 600).copyWith(color: c.inkMuted),
                        ),
                      ),
                    ],
                  ),
                ],
              ],
            ),
          ),
        ],
      ],
    );
  }
}
