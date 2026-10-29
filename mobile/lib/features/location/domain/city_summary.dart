final class CitySummary {
  const CitySummary({required this.ibgeCode, required this.name, required this.state});

  factory CitySummary.fromJson(Map<String, Object?> json) {
    final ibgeCode = json['ibgeCode'];
    final name = json['name'];
    final state = json['state'];
    if (ibgeCode is! int || name is! String || state is! String) {
      throw const FormatException('City without ibgeCode, name or state.');
    }

    return CitySummary(ibgeCode: ibgeCode, name: name, state: state);
  }

  final int ibgeCode;
  final String name;
  final String state;

  Map<String, Object?> toJson() => {'ibgeCode': ibgeCode, 'name': name, 'state': state};

  @override
  bool operator ==(Object other) => other is CitySummary && other.ibgeCode == ibgeCode;

  @override
  int get hashCode => ibgeCode.hashCode;
}
