using Vitrio.Domain.Accounts;

namespace Vitrio.Application.Accounts.Repositories;

public interface IRefreshTokenWritableRepository
{
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken);

    Task<IReadOnlyList<RefreshToken>> ListUnrevokedForSessionsAsync(
        IReadOnlyCollection<Guid> sessionIds,
        CancellationToken cancellationToken);

    void Add(RefreshToken refreshToken);
}
