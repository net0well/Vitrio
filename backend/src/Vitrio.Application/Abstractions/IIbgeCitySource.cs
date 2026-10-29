namespace Vitrio.Application.Abstractions;

public interface IIbgeCitySource
{
    Task<IReadOnlyList<IbgeMunicipality>> ListStateAsync(string state, CancellationToken cancellationToken);
}
