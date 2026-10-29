using Vitrio.Domain.Common;

namespace Vitrio.Application.Abstractions;

public interface IAccountDeletionBlocker
{
    Task<Error?> FindBlockerAsync(Guid userId, CancellationToken cancellationToken);
}
