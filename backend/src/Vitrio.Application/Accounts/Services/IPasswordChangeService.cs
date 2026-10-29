using Vitrio.Domain.Accounts;

namespace Vitrio.Application.Accounts.Services;

public interface IPasswordChangeService
{
    Task<IReadOnlyCollection<Guid>> ChangeAsync(
        User user,
        string newPassword,
        string revocationReason,
        Guid? sessionToKeep,
        CancellationToken cancellationToken);

    Task NotifyAsync(User user, CancellationToken cancellationToken);
}
