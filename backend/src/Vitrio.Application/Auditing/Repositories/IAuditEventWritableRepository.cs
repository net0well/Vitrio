using Vitrio.Domain.Auditing;

namespace Vitrio.Application.Auditing.Repositories;

public interface IAuditEventWritableRepository
{
    void Add(AuditEvent auditEvent);
}
