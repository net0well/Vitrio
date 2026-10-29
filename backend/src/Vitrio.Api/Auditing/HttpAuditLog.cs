using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Auditing.Repositories;
using Vitrio.Domain.Auditing;
using Vitrio.Domain.Common;

namespace Vitrio.Api.Auditing;

internal sealed class HttpAuditLog(
    IAuditEventWritableRepository auditEvents,
    IHttpContextAccessor httpContextAccessor,
    IClock clock) : IAuditLog
{
    private const int IPv4PrefixLength = 24;
    private const int IPv6PrefixLength = 48;

    public void Record(string action, Guid? actorId, string targetType, Guid? targetId)
    {
        var context = httpContextAccessor.HttpContext;
        var traceId = Activity.Current?.TraceId.ToHexString() ?? context?.TraceIdentifier;
        auditEvents.Add(AuditEvent.Record(
            action,
            actorId,
            targetType,
            targetId,
            TruncateAddress(context?.Connection.RemoteIpAddress),
            traceId,
            clock));
    }

    public static string? TruncateAddress(IPAddress? address)
    {
        if (address is null)
        {
            return null;
        }

        if (address.IsIPv4MappedToIPv6)
        {
            address = address.MapToIPv4();
        }

        var prefixLength = address.AddressFamily == AddressFamily.InterNetwork ? IPv4PrefixLength : IPv6PrefixLength;
        return new IPNetwork(Mask(address, prefixLength), prefixLength).ToString();
    }

    private static IPAddress Mask(IPAddress address, int prefixLength)
    {
        var bytes = address.GetAddressBytes();
        for (var bit = prefixLength; bit < bytes.Length * 8; bit++)
        {
            bytes[bit / 8] &= (byte)~(0x80 >> (bit % 8));
        }

        return new IPAddress(bytes);
    }
}
