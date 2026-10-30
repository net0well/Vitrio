using Vitrio.Shared.Time;

namespace Vitrio.Domain.Favorites;

public sealed class Favorite
{
    public const int MaxPerUser = 200;

    private Favorite()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid ProviderId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public static Favorite Create(Guid userId, Guid providerId, IClock clock)
    {
        var now = clock.UtcNow;
        return new Favorite { Id = Guid.CreateVersion7(now), UserId = userId, ProviderId = providerId, CreatedAt = now };
    }
}
