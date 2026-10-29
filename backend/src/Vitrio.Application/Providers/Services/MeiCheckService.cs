using Vitrio.Application.Abstractions;
using Vitrio.Application.Providers.Common;
using Vitrio.Domain.Providers;

namespace Vitrio.Application.Providers.Services;

internal sealed class MeiCheckService(ICnpjLookup cnpjLookup) : IMeiCheckService
{
    public async Task<MeiCheck> CheckAsync(Cnpj cnpj, CancellationToken cancellationToken)
    {
        var lookup = await cnpjLookup.FindAsync(cnpj, cancellationToken);
        return lookup switch
        {
            { IsFailure: true } => new MeiCheck(MeiCheckOutcome.Unavailable, null),
            { Value: null } => new MeiCheck(MeiCheckOutcome.NotFound, null),
            { Value: { IsActive: false } found } => new MeiCheck(MeiCheckOutcome.Inactive, found.CompanyName),
            { Value: { IsMei: false } found } => new MeiCheck(MeiCheckOutcome.NotMei, found.CompanyName),
            { Value: { } found } => new MeiCheck(MeiCheckOutcome.ActiveMei, found.CompanyName)
        };
    }
}
