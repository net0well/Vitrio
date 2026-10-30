import 'package:flutter/material.dart';

import '../tokens/vitrio_colors.dart';
import '../tokens/vitrio_radius.dart';
import '../tokens/vitrio_space.dart';
import '../tokens/vitrio_text.dart';
import 'vitrio_icon.dart';
import 'vitrio_icon_name.dart';

class VitrioAddPhotoTile extends StatelessWidget {
  const VitrioAddPhotoTile({super.key, required this.label, required this.onTap, this.busy = false});

  final String label;
  final VoidCallback? onTap;
  final bool busy;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    const radius = BorderRadius.all(VitrioRadius.xl);
    return Semantics(
      button: true,
      label: label,
      excludeSemantics: true,
      child: Material(
        color: c.surfaceRaised,
        borderRadius: radius,
        child: InkWell(
          borderRadius: radius,
          onTap: busy ? null : onTap,
          child: CustomPaint(
            painter: _DashedBorderPainter(color: c.lineStrong),
            child: Center(
              child: busy
                  ? const SizedBox.square(dimension: 22, child: CircularProgressIndicator(strokeWidth: 2.5))
                  : Column(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        VitrioIcon(VitrioIconName.camera, color: c.plum),
                        const SizedBox(height: VitrioSpace.sp4),
                        Text(label, textAlign: TextAlign.center, style: VitrioText.weighted(VitrioText.micro, 800).copyWith(color: c.plum)),
                      ],
                    ),
            ),
          ),
        ),
      ),
    );
  }
}

class _DashedBorderPainter extends CustomPainter {
  const _DashedBorderPainter({required this.color});

  final Color color;

  @override
  void paint(Canvas canvas, Size size) {
    const width = 1.5;
    final rect = RRect.fromRectAndRadius((Offset.zero & size).deflate(width / 2), VitrioRadius.xl);
    final paint = Paint()
      ..color = color
      ..style = PaintingStyle.stroke
      ..strokeWidth = width;
    for (final metric in (Path()..addRRect(rect)).computeMetrics()) {
      for (var distance = 0.0; distance < metric.length; distance += 9) {
        canvas.drawPath(metric.extractPath(distance, distance + 5), paint);
      }
    }
  }

  @override
  bool shouldRepaint(_DashedBorderPainter oldDelegate) => oldDelegate.color != color;
}
