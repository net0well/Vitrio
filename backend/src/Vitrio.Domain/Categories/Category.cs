namespace Vitrio.Domain.Categories;

public sealed class Category
{
    public const int SlugMaxLength = 40;
    public const int NameMaxLength = 40;
    public const int IconMaxLength = 30;
    public const int ProfessionalMaxLength = 40;

    private Category()
    {
    }

    public int Id { get; private set; }

    public string Slug { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Icon { get; private set; } = string.Empty;

    public string ProfessionalSingular { get; private set; } = string.Empty;

    public string ProfessionalPlural { get; private set; } = string.Empty;

    public int SortOrder { get; private set; }

    public bool Active { get; private set; }

    public static Category Seed(int id, string slug, string name, string icon, string professionalSingular, string professionalPlural) => new()
    {
        Id = id,
        Slug = slug,
        Name = name,
        Icon = icon,
        ProfessionalSingular = professionalSingular,
        ProfessionalPlural = professionalPlural,
        SortOrder = id,
        Active = true
    };
}
