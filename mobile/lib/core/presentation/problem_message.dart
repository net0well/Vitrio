import '../../l10n/app_localizations.dart';
import '../network/problem_details.dart';

String problemMessage(AppLocalizations l10n, ProblemDetails problem) {
  if (problem.isConnectivity) {
    return l10n.errorConnection;
  }

  return problem.detail ?? l10n.errorGeneric;
}

String? fieldError(ProblemDetails? problem, String field) => problem?.errors[field]?.first;
