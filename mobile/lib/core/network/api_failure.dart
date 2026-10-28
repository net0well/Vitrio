part of 'api_result.dart';

final class ApiFailure<T> extends ApiResult<T> {
  const ApiFailure(this.problem);

  final ProblemDetails problem;
}
