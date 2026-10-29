namespace Vitrio.Application.Abstractions;

public interface ISessionStatusCache
{
    Task ForgetAsync(IReadOnlyCollection<Guid> sessionIds, CancellationToken cancellationToken);
}
