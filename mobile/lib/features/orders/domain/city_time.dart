final _offsetSuffix = RegExp(r'(Z|[+-]\d{2}:\d{2})$');

// The API sends order instants with the offset of the order city, so the wall clock in the text is already the
// city time; DateTime.parse would move it to the device zone instead.
DateTime parseCityTime(String value) => DateTime.parse(value.replaceFirst(_offsetSuffix, ''));

Duration parseCityOffset(String value) {
  final suffix = _offsetSuffix.firstMatch(value)?.group(0);
  if (suffix == null || suffix == 'Z') {
    return Duration.zero;
  }

  final sign = suffix.startsWith('-') ? -1 : 1;
  final hours = int.parse(suffix.substring(1, 3));
  final minutes = int.parse(suffix.substring(4, 6));
  return Duration(hours: hours, minutes: minutes) * sign;
}

DateTime cityNow(DateTime now, Duration offset) {
  final city = now.toUtc().add(offset);
  return DateTime(city.year, city.month, city.day, city.hour, city.minute, city.second);
}
