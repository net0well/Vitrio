namespace Vitrio.Api.IntegrationTests.Infrastructure;

// The jobs change every row that is due, including the ones other tests are still asserting on.
[CollectionDefinition(nameof(SharedDataJobsCollection), DisableParallelization = true)]
public sealed class SharedDataJobsCollection;
