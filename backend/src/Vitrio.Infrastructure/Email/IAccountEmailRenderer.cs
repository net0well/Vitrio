using Vitrio.Application.Accounts.Emails;

namespace Vitrio.Infrastructure.Email;

public interface IAccountEmailRenderer
{
    byte[] Logo { get; }

    RenderedEmail Render(AccountEmail email);
}
