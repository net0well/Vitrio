enum AgendaItemKind {
  scheduled,
  quoted,
  done;

  static AgendaItemKind fromJson(Object? json) =>
      AgendaItemKind.values.firstWhere((value) => value.name == json, orElse: () => AgendaItemKind.done);
}
