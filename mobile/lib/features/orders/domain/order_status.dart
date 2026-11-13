enum OrderStatus {
  sent,
  quoted,
  scheduled,
  declined,
  expired,
  quoteDeclined,
  quoteExpired,
  cancelled,
  completed,
  paymentConfirmed,
  paymentDisputed,
  paymentVoided;

  static OrderStatus fromJson(Object? json) => OrderStatus.values.firstWhere((value) => value.name == json, orElse: () => OrderStatus.values.first);

  bool get isOpen => this == sent || this == quoted || this == scheduled;

  bool get offersSuggestions => this == declined || this == expired;
}
