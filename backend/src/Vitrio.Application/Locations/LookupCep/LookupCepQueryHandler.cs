using Mediator;
using Vitrio.Application.Accounts.Services;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;
using Vitrio.Domain.Locations;

namespace Vitrio.Application.Locations.LookupCep;

public sealed class LookupCepQueryHandler(IAddressLookupService addressLookup) : IQueryHandler<LookupCepQuery, Result<CepResponse>>
{
    public async ValueTask<Result<CepResponse>> Handle(LookupCepQuery query, CancellationToken cancellationToken)
    {
        var cep = Cep.Create(query.Cep).Value;
        var details = await addressLookup.LookupAsync(cep, cancellationToken);
        if (details.IsFailure)
        {
            return details.Error;
        }

        return details.Value is { } found
            ? new CepResponse(cep.Value, found.Street, found.Neighborhood, found.City)
            : LocationErrors.CepNotFound;
    }
}
