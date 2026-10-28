import 'problem_details.dart';

part 'api_failure.dart';
part 'api_success.dart';

sealed class ApiResult<T> {
  const ApiResult();
}
