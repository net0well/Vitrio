namespace Vitrio.Application.Abstractions;

public sealed record IbgeMunicipality(int IbgeCode, string Name, string State, string BoundaryGeoJson);
