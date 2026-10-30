import '../../../core/network/api_result.dart';
import '../domain/cep_address.dart';
import '../domain/city_summary.dart';

abstract interface class CitiesRepository {
  Future<ApiResult<CitySummary>> locate({required double latitude, required double longitude});

  Future<ApiResult<List<CitySummary>>> search(String term);

  Future<ApiResult<CepAddress>> lookupCep(String cep);
}
