using Vitrio.Domain.Accounts;

namespace Vitrio.Application.Accounts.Repositories;

public interface ISessionWritableRepository
{
    void Add(Session session);
}
