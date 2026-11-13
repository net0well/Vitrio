enum CancellationReason {
  resolvedAnotherWay,
  timeNoLongerWorks,
  newTimeDoesNotWork,
  foundAnotherProfessional,
  unforeseen,
  cannotMakeIt,
  customerAskedToCancel,
  other,
  accountDeleted,
  accountSuspended;

  static CancellationReason fromJson(Object? json) => CancellationReason.values.firstWhere((value) => value.name == json, orElse: () => CancellationReason.values.first);

  static const customerReasons = [resolvedAnotherWay, timeNoLongerWorks, newTimeDoesNotWork, foundAnotherProfessional, other];

  static const providerReasons = [unforeseen, cannotMakeIt, customerAskedToCancel, other];
}
