import 'mei_status.dart';

final class MeiInfo {
  const MeiInfo({required this.status, this.cnpj, this.companyName, this.reason});

  factory MeiInfo.fromJson(Map<String, Object?> json) => MeiInfo(
        cnpj: json['cnpj'] as String?,
        status: MeiStatus.fromJson(json['status']),
        companyName: json['companyName'] as String?,
        reason: json['reason'] as String?,
      );

  final String? cnpj;
  final MeiStatus status;
  final String? companyName;
  final String? reason;
}
