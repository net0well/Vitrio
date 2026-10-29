import 'dart:typed_data';

import 'package:flutter/material.dart';

import '../tokens/vitrio_colors.dart';
import '../tokens/vitrio_radius.dart';
import '../tokens/vitrio_text.dart';
import 'vitrio_avatar_kind.dart';

class VitrioAvatar extends StatelessWidget {
  const VitrioAvatar({
    super.key,
    required this.name,
    this.imageUrl,
    this.imageBytes,
    this.size = 44,
    this.kind = VitrioAvatarKind.provider,
  });

  final String name;
  final String? imageUrl;
  final Uint8List? imageBytes;
  final double size;
  final VitrioAvatarKind kind;

  String get _initials {
    final parts = name.trim().split(RegExp(r'\s+')).where((part) => part.isNotEmpty).toList();
    if (parts.isEmpty) {
      return '';
    }

    return (parts.first[0] + (parts.length > 1 ? parts.last[0] : '')).toUpperCase();
  }

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    final fallback = ColoredBox(
      color: c.plumSoft,
      child: Center(child: Text(_initials, style: VitrioText.label.copyWith(color: c.plum, fontSize: size * 0.34))),
    );
    final image = imageBytes != null
        ? Image.memory(imageBytes!, fit: BoxFit.cover)
        : imageUrl != null
            ? Image.network(imageUrl!, fit: BoxFit.cover, errorBuilder: (_, _, _) => fallback)
            : fallback;
    return Semantics(
      label: name,
      image: true,
      child: SizedBox.square(
        dimension: size,
        child: kind == VitrioAvatarKind.provider
            ? ClipRRect(borderRadius: VitrioRadius.providerAvatar(size), child: image)
            : ClipOval(child: image),
      ),
    );
  }
}
