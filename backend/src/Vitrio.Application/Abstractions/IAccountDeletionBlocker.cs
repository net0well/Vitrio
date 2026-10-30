using Vitrio.Shared.Results;

namespace Vitrio.Application.Abstractions;

public interface IAccountDeletionBlocker
{
    Task<Error?> FindBlockerAsync(Guid userId, CancellationToken cancellationToken);
}
