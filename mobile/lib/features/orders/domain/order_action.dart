enum OrderAction {
  acceptQuote,
  declineQuote,
  cancel,
  complete,
  sendQuote,
  replaceQuote,
  decline,
  reschedule;

  static OrderAction fromJson(Object? json) => OrderAction.values.firstWhere((value) => value.name == json, orElse: () => OrderAction.values.first);
}
