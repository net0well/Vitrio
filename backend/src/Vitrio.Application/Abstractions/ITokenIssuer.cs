using Vitrio.Domain.Accounts;

namespace Vitrio.Application.Abstractions;

public interface ITokenIssuer
{
    AccessToken Issue(User user, Session session, IReadOnlyCollection<string> roles);
}
