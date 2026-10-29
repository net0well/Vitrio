using Vitrio.Application.Auditing.Repositories;
using Vitrio.Domain.Auditing;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal sealed class AuditEventWritableRepository(AppDbContext db) : IAuditEventWritableRepository
{
    public void Add(AuditEvent auditEvent) => db.AuditEvents.Add(auditEvent);
}
