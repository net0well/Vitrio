final class SearchArea {
  const SearchArea.nearMe({
    required double this.latitude,
    required double this.longitude,
    this.cityIbgeCode,
  });

  const SearchArea.city(int this.cityIbgeCode)
      : latitude = null,
        longitude = null;

  final double? latitude;
  final double? longitude;
  final int? cityIbgeCode;

  bool get hasPoint => latitude != null && longitude != null;

  Map<String, String> toQuery() => {
        if (latitude case final lat?) 'lat': lat.toString(),
        if (longitude case final lng?) 'lng': lng.toString(),
        if (cityIbgeCode case final city? when !hasPoint) 'cityIbgeCode': city.toString(),
      };
}
