import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:share_plus/share_plus.dart';

import '../../../core/network/api_result.dart';
import '../../../l10n/app_localizations.dart';
import '../data/provider_providers.dart';

Future<void> shareProviderProfile(BuildContext context, WidgetRef ref, String providerId) async {
  final l10n = AppLocalizations.of(context);
  final profile = await ref.read(providerRepositoryProvider).publicProfile(providerId);
  if (profile case ApiSuccess(:final data)) {
    await SharePlus.instance.share(ShareParams(text: l10n.providerShareText(data.shareUrl)));
  }
}
