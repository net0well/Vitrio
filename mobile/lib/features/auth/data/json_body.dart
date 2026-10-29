Map<String, Object?> jsonObject(Object? json) {
  if (json is! Map<String, Object?>) {
    throw const FormatException('Expected a JSON object.');
  }

  return json;
}

List<Map<String, Object?>> jsonObjects(Object? json) {
  if (json is! List<Object?>) {
    throw const FormatException('Expected a JSON array.');
  }

  return json.map(jsonObject).toList(growable: false);
}
