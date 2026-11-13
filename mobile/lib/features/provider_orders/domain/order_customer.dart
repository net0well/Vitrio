final class OrderCustomer {
  const OrderCustomer({required this.displayName, required this.memberSince, required this.orderCount, this.fullName, this.phone});

  factory OrderCustomer.fromJson(Map<String, Object?> json) => OrderCustomer(
    displayName: json['displayName'] as String,
    fullName: json['fullName'] as String?,
    phone: json['phone'] as String?,
    memberSince: DateTime.parse(json['memberSince'] as String),
    orderCount: json['orderCount'] as int? ?? 0,
  );

  final String displayName;
  final String? fullName;
  final String? phone;
  final DateTime memberSince;
  final int orderCount;

  String get name => fullName ?? displayName;

  String get firstName => displayName.split(' ').first;
}
