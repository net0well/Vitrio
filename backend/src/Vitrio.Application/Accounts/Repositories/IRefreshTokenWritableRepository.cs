using Vitrio.Domain.Accounts;

namespace Vitrio.Application.Accounts.Repositories;

public interface IRefreshTokenWritableRepository
{
    void Add(RefreshToken refreshToken);
}
