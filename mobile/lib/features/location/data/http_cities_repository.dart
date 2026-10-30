import '../../../core/network/api_client.dart';
import '../../../core/network/api_result.dart';
import '../../../core/network/json_body.dart';
import '../domain/cep_address.dart';
import '../domain/city_summary.dart';
import 'cities_repository.dart';

final class HttpCitiesRepository implements CitiesRepository {
  const HttpCitiesRepository(this._api);

  final ApiClient _api;

  @override
  Future<ApiResult<CitySummary>> locate({required double latitude, required double longitude}) => _api.get(
        '/api/v1/cities/locate',
        query: {'lat': latitude.toString(), 'lng': longitude.toString()},
        decode: (json) => CitySummary.fromJson(jsonObject(jsonObject(json)['city'])),
      );

  @override
  Future<ApiResult<List<CitySummary>>> search(String term) => _api.get(
        '/api/v1/cities',
        query: {'search': term, 'limit': '20'},
        decode: (json) => jsonObjects(json).map(CitySummary.fromJson).toList(growable: false),
      );

  @override
  Future<ApiResult<CepAddress>> lookupCep(String cep) =>
      _api.get('/api/v1/ceps/$cep', decode: (json) => CepAddress.fromJson(jsonObject(json)));
}
