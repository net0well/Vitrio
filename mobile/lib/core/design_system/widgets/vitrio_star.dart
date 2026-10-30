import 'package:flutter/widgets.dart';

import '../tokens/vitrio_colors.dart';

class VitrioStar extends StatelessWidget {
  const VitrioStar({super.key, this.size = 15, this.empty = false});

  final double size;
  final bool empty;

  @override
  Widget build(BuildContext context) {
    final c = context.vitrio;
    return ExcludeSemantics(
      child: CustomPaint(
        size: Size.square(size),
        painter: _StarPainter(fill: empty ? null : c.amber, edge: empty ? c.lineStrong : c.amberEdge),
      ),
    );
  }
}

class _StarPainter extends CustomPainter {
  const _StarPainter({required this.fill, required this.edge});

  static const _points = [
    Offset(12, 3.5),
    Offset(14.6, 8.8),
    Offset(20.4, 9.6),
    Offset(16.2, 13.7),
    Offset(17.2, 19.5),
    Offset(12, 16.8),
    Offset(6.8, 19.5),
    Offset(7.8, 13.7),
    Offset(3.6, 9.6),
    Offset(9.4, 8.8),
  ];

  final Color? fill;
  final Color edge;

  @override
  void paint(Canvas canvas, Size size) {
    final scale = size.width / 24;
    final path = Path()..addPolygon([for (final point in _points) point * scale], true);
    if (fill != null) {
      canvas.drawPath(path, Paint()..color = fill!);
    }

    canvas.drawPath(
      path,
      Paint()
        ..color = edge
        ..style = PaintingStyle.stroke
        ..strokeWidth = 1.2 * scale
        ..strokeJoin = StrokeJoin.round,
    );
  }

  @override
  bool shouldRepaint(_StarPainter oldDelegate) => oldDelegate.fill != fill || oldDelegate.edge != edge;
}
