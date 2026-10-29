namespace Vitrio.Application.Abstractions;

public interface ISessionStatusChecker
{
    Task<bool> IsActiveAsync(Guid sessionId, CancellationToken cancellationToken);
}
