namespace Vitrio.Application.Providers.Common;

public sealed record CategoryResponse(int Id, string Slug, string Name, string Icon, string ProfessionalSingular, string ProfessionalPlural);
