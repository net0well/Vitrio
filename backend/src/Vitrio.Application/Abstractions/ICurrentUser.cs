namespace Vitrio.Application.Abstractions;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    Guid Id { get; }

    Guid SessionId { get; }
}
