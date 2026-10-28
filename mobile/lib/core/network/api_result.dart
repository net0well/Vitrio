import 'problem_details.dart';

sealed class ApiResult<T> {
  const ApiResult();
}

final class ApiSuccess<T> extends ApiResult<T> {
  const ApiSuccess(this.data);

  final T data;
}

final class ApiFailure<T> extends ApiResult<T> {
  const ApiFailure(this.problem);

  final ProblemDetails problem;
}
