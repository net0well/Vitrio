import 'mei_status.dart';

final class MeiCheck {
  const MeiCheck({required this.status, this.companyName, this.reason});

  factory MeiCheck.fromJson(Map<String, Object?> json) => MeiCheck(
        status: MeiStatus.fromJson(json['status']),
        companyName: json['companyName'] as String?,
        reason: json['reason'] as String?,
      );

  final MeiStatus status;
  final String? companyName;
  final String? reason;
}
