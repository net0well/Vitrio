using Mediator;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Locations.LookupCep;

public sealed record LookupCepQuery(string Cep) : IQuery<Result<CepResponse>>;
