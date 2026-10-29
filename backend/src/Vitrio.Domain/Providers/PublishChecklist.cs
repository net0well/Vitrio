namespace Vitrio.Domain.Providers;

public static class PublishChecklist
{
    public const string About = "about";
    public const string Categories = "categories";
    public const string ServiceArea = "serviceArea";
    public const string Services = "services";
    public const string ContactPhone = "contactPhone";
    public const string Availability = "availability";
    public const string Publish = "publish";
    public const string Share = "share";

    public static IReadOnlyList<string> Missing(bool hasAbout, int categoryCount, bool hasServiceArea, int serviceCount, bool hasContactPhone)
    {
        var missing = new List<string>();
        if (!hasAbout)
        {
            missing.Add(About);
        }

        if (categoryCount == 0)
        {
            missing.Add(Categories);
        }

        if (!hasServiceArea)
        {
            missing.Add(ServiceArea);
        }

        if (serviceCount == 0)
        {
            missing.Add(Services);
        }

        if (!hasContactPhone)
        {
            missing.Add(ContactPhone);
        }

        return missing;
    }

    public static int Completeness(IReadOnlyList<string> missing, bool hasAvailability)
    {
        const int Items = 6;
        var done = Items - missing.Count - (hasAvailability ? 0 : 1);
        return (int)Math.Round(done * 100.0 / Items);
    }

    public static string NextStep(IReadOnlyList<string> missing, ProviderStatus status, bool hasAvailability) =>
        missing.Count > 0 ? missing[0]
        : status is ProviderStatus.Draft or ProviderStatus.Paused ? Publish
        : hasAvailability ? Share
        : Availability;
}
