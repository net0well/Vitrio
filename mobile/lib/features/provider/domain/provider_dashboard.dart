import '../../../core/network/json_body.dart';
import '../../provider_orders/domain/provider_order_summary.dart';
import 'mei_status.dart';
import 'provider_status.dart';

final class ProviderDashboard {
  const ProviderDashboard({
    required this.status,
    required this.profileCompleteness,
    required this.nextStep,
    required this.availableToday,
    this.meiStatus,
    this.newOrders = 0,
    this.servicesThisMonth = 0,
    this.latestNewOrders = const [],
  });

  factory ProviderDashboard.fromJson(Map<String, Object?> json) => ProviderDashboard(
        status: ProviderStatus.fromJson(json['status']),
        profileCompleteness: json['profileCompleteness'] as int,
        nextStep: json['nextStep'] as String,
        meiStatus: json['meiStatus'] == null ? null : MeiStatus.fromJson(json['meiStatus']),
        availableToday: json['availableToday'] == true,
        newOrders: json['newOrders'] as int? ?? 0,
        servicesThisMonth: json['servicesThisMonth'] as int? ?? 0,
        latestNewOrders: jsonObjects(json['latestNewOrders'] ?? const <Object?>[]).map(ProviderOrderSummary.fromJson).toList(growable: false),
      );

  final ProviderStatus status;
  final int profileCompleteness;
  final String nextStep;
  final MeiStatus? meiStatus;
  final bool availableToday;
  final int newOrders;
  final int servicesThisMonth;
  final List<ProviderOrderSummary> latestNewOrders;
}
