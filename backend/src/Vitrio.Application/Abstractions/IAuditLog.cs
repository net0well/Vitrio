namespace Vitrio.Application.Abstractions;

public interface IAuditLog
{
    void Record(string action, Guid? actorId, string targetType, Guid? targetId);
}
