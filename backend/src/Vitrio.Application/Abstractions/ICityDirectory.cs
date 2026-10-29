using Vitrio.Domain.Common;

namespace Vitrio.Application.Abstractions;

public interface ICityDirectory
{
    Task<Result<IReadOnlyList<IbgeCity>>> ListAsync(string state, CancellationToken cancellationToken);
}
