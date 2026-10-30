import 'package:vitrio/core/network/api_result.dart';
import 'package:vitrio/core/network/problem_details.dart';
import 'package:vitrio/features/location/data/cities_repository.dart';
import 'package:vitrio/features/location/domain/cep_address.dart';
import 'package:vitrio/features/location/domain/city_summary.dart';

const jaboticabal = CitySummary(ibgeCode: 3524303, name: 'Jaboticabal', state: 'SP');
const saoPaulo = CitySummary(ibgeCode: 3550308, name: 'São Paulo', state: 'SP');

final class FakeCitiesRepository implements CitiesRepository {
  CitySummary? located = jaboticabal;
  ProblemDetails? searchFailure;
  CepAddress? cepAddress = const CepAddress(cep: '14870-000', street: 'Rua Rui Barbosa', neighborhood: 'Centro', city: jaboticabal);

  @override
  Future<ApiResult<CitySummary>> locate({required double latitude, required double longitude}) async => located == null
      ? const ApiFailure(ProblemDetails(status: 404, code: 'city.not_nearby'))
      : ApiSuccess(located!);

  @override
  Future<ApiResult<List<CitySummary>>> search(String term) async => searchFailure != null
      ? ApiFailure(searchFailure!)
      : ApiSuccess(
          [jaboticabal, saoPaulo].where((city) => city.name.toLowerCase().contains(term.toLowerCase())).toList(),
        );

  @override
  Future<ApiResult<CepAddress>> lookupCep(String cep) async =>
      cepAddress == null ? const ApiFailure(ProblemDetails(status: 404, code: 'cep.not_found')) : ApiSuccess(cepAddress!);
}
