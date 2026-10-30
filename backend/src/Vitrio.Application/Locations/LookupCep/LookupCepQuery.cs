using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Locations.LookupCep;

public sealed record LookupCepQuery(string Cep) : IQuery<Result<CepResponse>>;
