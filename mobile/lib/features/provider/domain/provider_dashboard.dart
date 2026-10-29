import 'mei_status.dart';
import 'provider_status.dart';

final class ProviderDashboard {
  const ProviderDashboard({
    required this.status,
    required this.profileCompleteness,
    required this.nextStep,
    required this.availableToday,
    this.meiStatus,
  });

  factory ProviderDashboard.fromJson(Map<String, Object?> json) => ProviderDashboard(
        status: ProviderStatus.fromJson(json['status']),
        profileCompleteness: json['profileCompleteness'] as int,
        nextStep: json['nextStep'] as String,
        meiStatus: json['meiStatus'] == null ? null : MeiStatus.fromJson(json['meiStatus']),
        availableToday: json['availableToday'] == true,
      );

  final ProviderStatus status;
  final int profileCompleteness;
  final String nextStep;
  final MeiStatus? meiStatus;
  final bool availableToday;
}
