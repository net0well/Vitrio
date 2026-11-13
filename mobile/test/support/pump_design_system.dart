import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:vitrio/core/design_system/tokens/vitrio_space.dart';
import 'package:vitrio/core/design_system/vitrio_theme.dart';

Future<void> pumpDesignSystem(WidgetTester tester, Widget child) => tester.pumpWidget(
      MaterialApp(
        theme: vitrioTheme(Brightness.light),
        home: Scaffold(
          body: SingleChildScrollView(
            padding: const EdgeInsets.all(VitrioSpace.sp20),
            child: child,
          ),
        ),
      ),
    );
