import 'package:flutter/widgets.dart';

import '../../../../core/design_system/tokens/vitrio_space.dart';
import '../../../../core/design_system/widgets/vitrio_mark.dart';

class AuthBrandMark extends StatelessWidget {
  const AuthBrandMark({super.key});

  @override
  Widget build(BuildContext context) => const Padding(
        padding: EdgeInsets.only(top: VitrioSpace.sp4, bottom: VitrioSpace.sp16),
        child: Align(alignment: AlignmentDirectional.centerStart, child: VitrioMark()),
      );
}
