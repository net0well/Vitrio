using Vitrio.Shared.Results;

namespace Vitrio.Jobs;

internal static class JobResults
{
    // Hangfire only retries a job that throws, so a failed command must surface as an exception.
    public static void EnsureSucceeded(Result result)
    {
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"The job failed with '{result.Error!.Code}'.");
        }
    }
}
