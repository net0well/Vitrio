using Vitrio.Application.Accounts.Repositories;
using Vitrio.Domain.Accounts;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal sealed class SessionWritableRepository(AppDbContext db) : ISessionWritableRepository
{
    public void Add(Session session) => db.Sessions.Add(session);
}
