import 'package:flutter/foundation.dart';

String currentDeviceName() => switch (defaultTargetPlatform) {
      TargetPlatform.android => 'Android',
      TargetPlatform.iOS => 'iPhone',
      _ => 'Aparelho',
    };
