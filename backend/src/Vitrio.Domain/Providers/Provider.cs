using System.Globalization;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;
using Vitrio.Domain.Locations;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Domain.Providers;

public sealed class Provider : AggregateRoot
{
    public const int HeadlineMinLength = 2;
    public const int HeadlineMaxLength = 60;
    public const int AboutMinLength = 40;
    public const int AboutMaxLength = 600;
    public const int MaxYearsOfExperience = 60;
    public const int MaxCategories = 3;
    public const int MaxServices = 20;
    public const int MaxNeighborhoods = 30;
    public const int MinRadiusKm = 1;
    public const int MaxRadiusKm = 50;
    public const int MaxPhotos = 30;

    private const int MetersPerKm = 1000;

    private readonly List<ProviderCategory> _categories = [];
    private readonly List<ProviderService> _services = [];
    private readonly List<ServedNeighborhood> _neighborhoods = [];
    private readonly List<AvailabilitySlot> _availability = [];
    private readonly List<PortfolioPhoto> _photos = [];

    private Provider()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string DisplayName { get; private set; } = string.Empty;

    public string Headline { get; private set; } = string.Empty;

    public string? About { get; private set; }

    public int? YearsOfExperience { get; private set; }

    public string? AvatarKey { get; private set; }

    public string? AvatarUploadKey { get; private set; }

    public string? ContactPhone { get; private set; }

    public bool WhatsAppEnabled { get; private set; }

    public ProviderStatus Status { get; private set; }

    public DateTimeOffset? PublishedAt { get; private set; }

    public int? CityIbgeCode { get; private set; }

    public string? BaseCep { get; private set; }

    public string? BaseStreet { get; private set; }

    public string? BaseNumber { get; private set; }

    public string? BaseComplement { get; private set; }

    public string? BaseNeighborhood { get; private set; }

    public string? BaseLocation { get; private set; }

    public double? PublicLatitude { get; private set; }

    public double? PublicLongitude { get; private set; }

    public bool LocationApproximate { get; private set; }

    public int ServiceRadiusMeters { get; private set; }

    public bool AvailableToday { get; private set; }

    public DateTimeOffset? AvailableTodaySetAt { get; private set; }

    public bool HandlesEmergencies { get; private set; }

    public decimal? RatingScore { get; private set; }

    public int ReviewCount { get; private set; }

    public int CompletedOrders { get; private set; }

    public string? ResponseTimeBucket { get; private set; }

    public int ResponseSampleSize { get; private set; }

    public int LateCancellations90d { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyList<ProviderCategory> Categories => _categories;

    public IReadOnlyList<ProviderService> Services => _services;

    public IReadOnlyList<ServedNeighborhood> Neighborhoods => _neighborhoods;

    public IReadOnlyList<AvailabilitySlot> Availability => _availability;

    public IReadOnlyList<PortfolioPhoto> Photos => _photos;

    public bool CanReceivePhoto => _photos.Count(photo => photo.Status != PhotoStatus.Rejected) < MaxPhotos;

    public bool HasServiceArea => CityIbgeCode is not null;

    public IReadOnlyList<string> MissingForPublication =>
        PublishChecklist.Missing(About is not null, _categories.Count, HasServiceArea, _services.Count, ContactPhone is not null);

    public static Result<Provider> Create(Guid userId, DisplayName displayName, string? headline, PhoneNumber? contactPhone, IClock clock)
    {
        var normalizedHeadline = NormalizeHeadline(headline);
        if (normalizedHeadline.IsFailure)
        {
            return normalizedHeadline.Error;
        }

        var now = clock.UtcNow;
        return new Provider
        {
            Id = Guid.CreateVersion7(now),
            UserId = userId,
            DisplayName = displayName.Value,
            Headline = normalizedHeadline.Value,
            ContactPhone = contactPhone?.Value,
            Status = ProviderStatus.Draft,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public Result UpdateProfile(ProfileChanges changes, IClock clock)
    {
        string? headline = null;
        if (changes.Headline is not null)
        {
            var normalized = NormalizeHeadline(changes.Headline);
            if (normalized.IsFailure)
            {
                return normalized.Error;
            }

            headline = normalized.Value;
        }

        string? about = null;
        if (changes.About is not null)
        {
            about = FreeText.Normalize(changes.About);
            if (about.Length is < AboutMinLength or > AboutMaxLength)
            {
                return ProviderErrors.InvalidAbout;
            }

            if (FreeText.ContainsContactInformation(about))
            {
                return ProviderErrors.ContactInText;
            }
        }

        if (changes.YearsOfExperience is < 0 or > MaxYearsOfExperience)
        {
            return ProviderErrors.InvalidYearsOfExperience;
        }

        DisplayName = changes.DisplayName?.Value ?? DisplayName;
        Headline = headline ?? Headline;
        About = about ?? About;
        YearsOfExperience = changes.YearsOfExperience ?? YearsOfExperience;
        ContactPhone = changes.ContactPhone?.Value ?? ContactPhone;
        WhatsAppEnabled = changes.WhatsAppEnabled ?? WhatsAppEnabled;
        HandlesEmergencies = changes.HandlesEmergencies ?? HandlesEmergencies;
        Touch(clock);
        return Result.Success();
    }

    public Result SetCategories(IReadOnlyList<int> categoryIds, int primaryCategoryId, IClock clock)
    {
        var distinct = categoryIds.Distinct().ToList();
        if (distinct.Count is 0 or > MaxCategories || distinct.Count != categoryIds.Count || !distinct.Contains(primaryCategoryId))
        {
            return ProviderErrors.InvalidCategories;
        }

        _categories.Clear();
        _categories.AddRange(distinct.Select(id => ProviderCategory.Create(Id, id, id == primaryCategoryId)));
        Touch(clock);
        return Result.Success();
    }

    public Result SetServiceArea(
        BaseAddress address,
        Coordinates basePoint,
        bool approximate,
        int radiusKm,
        IReadOnlyList<string> neighborhoods,
        IClock clock)
    {
        if (radiusKm is < MinRadiusKm or > MaxRadiusKm)
        {
            return ProviderErrors.InvalidRadius;
        }

        var served = neighborhoods.Select(name => ServedNeighborhood.Create(Id, name)).ToList();
        if (served.Count > MaxNeighborhoods || served.Any(neighborhood => neighborhood is null))
        {
            return ProviderErrors.InvalidNeighborhoods;
        }

        var publicPoint = basePoint.ToPublicCell();
        CityIbgeCode = address.CityIbgeCode;
        BaseCep = address.Cep;
        BaseStreet = address.Street;
        BaseNumber = address.Number;
        BaseComplement = address.Complement;
        BaseNeighborhood = address.Neighborhood;
        BaseLocation = string.Create(CultureInfo.InvariantCulture, $"{basePoint.Latitude},{basePoint.Longitude}");
        PublicLatitude = publicPoint.Latitude;
        PublicLongitude = publicPoint.Longitude;
        LocationApproximate = approximate;
        ServiceRadiusMeters = radiusKm * MetersPerKm;
        _neighborhoods.Clear();
        _neighborhoods.AddRange(served.OfType<ServedNeighborhood>().DistinctBy(neighborhood => neighborhood.NameNormalized));
        Touch(clock);
        return Result.Success();
    }

    public Result SetServices(IReadOnlyList<ServiceDraft> drafts, IClock clock)
    {
        if (drafts.Count > MaxServices)
        {
            return ProviderErrors.InvalidServices;
        }

        var services = new List<ProviderService>(drafts.Count);
        foreach (var (draft, index) in drafts.Select((draft, index) => (draft, index)))
        {
            var service = ProviderService.Create(Id, draft, index);
            if (service.IsFailure)
            {
                return service.Error;
            }

            services.Add(service.Value);
        }

        _services.Clear();
        _services.AddRange(services);
        Touch(clock);
        return Result.Success();
    }

    public Result SetAvailability(IReadOnlyList<AvailabilityDraft> drafts, bool handlesEmergencies, IClock clock)
    {
        if (!drafts.All(AvailabilitySlot.IsValid) || drafts.GroupBy(draft => draft.DayOfWeek).Any(Overlaps))
        {
            return ProviderErrors.InvalidAvailability;
        }

        _availability.Clear();
        _availability.AddRange(drafts
            .OrderBy(draft => draft.DayOfWeek)
            .ThenBy(draft => draft.StartMinute)
            .Select(draft => AvailabilitySlot.Create(Id, draft)));
        HandlesEmergencies = handlesEmergencies;
        Touch(clock);
        return Result.Success();
    }

    public Result SetAvailableToday(bool value, IClock clock)
    {
        if (Status != ProviderStatus.Published)
        {
            return ProviderErrors.NotPublished;
        }

        AvailableToday = value;
        AvailableTodaySetAt = value ? clock.UtcNow : AvailableTodaySetAt;
        Touch(clock);
        return Result.Success();
    }

    public Result Publish(IClock clock)
    {
        if (Status is ProviderStatus.Suspended or ProviderStatus.Removed)
        {
            return ProviderErrors.Suspended;
        }

        var missing = MissingForPublication;
        if (missing.Count > 0)
        {
            return ProviderErrors.Incomplete(missing);
        }

        if (Status == ProviderStatus.Published)
        {
            return Result.Success();
        }

        Status = ProviderStatus.Published;
        PublishedAt ??= clock.UtcNow;
        UpdatedAt = clock.UtcNow;
        Raise(new ProviderPublished(Id));
        return Result.Success();
    }

    public Result Pause(IClock clock)
    {
        if (Status != ProviderStatus.Published)
        {
            return ProviderErrors.NotPublished;
        }

        Status = ProviderStatus.Paused;
        AvailableToday = false;
        UpdatedAt = clock.UtcNow;
        Raise(new ProviderPaused(Id));
        return Result.Success();
    }

    public Result<PortfolioPhoto> AddPhoto(string uploadKey, string? caption, IClock clock)
    {
        if (!CanReceivePhoto)
        {
            return ProviderErrors.TooManyPhotos;
        }

        var normalized = PortfolioPhoto.NormalizeCaption(caption, out var valid);
        if (!valid)
        {
            return ProviderErrors.InvalidCaption;
        }

        var photo = PortfolioPhoto.Create(Id, uploadKey, normalized, _photos.Count == 0 ? 0 : _photos.Max(existing => existing.SortOrder) + 1, clock.UtcNow);
        _photos.Add(photo);
        UpdatedAt = clock.UtcNow;
        return photo;
    }

    public Result CompletePhoto(Guid photoId, string objectKey, string thumbnailKey, IClock clock)
    {
        if (FindPhoto(photoId) is not { Status: PhotoStatus.Processing } photo)
        {
            return ProviderErrors.PhotoNotFound;
        }

        photo.Activate(objectKey, thumbnailKey);
        Touch(clock);
        return Result.Success();
    }

    public Result RejectPhoto(Guid photoId, string reason, IClock clock)
    {
        if (FindPhoto(photoId) is not { Status: PhotoStatus.Processing } photo)
        {
            return ProviderErrors.PhotoNotFound;
        }

        photo.Reject(reason);
        Touch(clock);
        return Result.Success();
    }

    public Result RenamePhoto(Guid photoId, string? caption, IClock clock)
    {
        if (FindPhoto(photoId) is not { } photo)
        {
            return ProviderErrors.PhotoNotFound;
        }

        var normalized = PortfolioPhoto.NormalizeCaption(caption, out var valid);
        if (!valid)
        {
            return ProviderErrors.InvalidCaption;
        }

        photo.Rename(normalized);
        Touch(clock);
        return Result.Success();
    }

    public Result ReorderPhotos(IReadOnlyList<Guid> photoIds, IClock clock)
    {
        if (photoIds.Distinct().Count() != photoIds.Count || photoIds.Any(id => FindPhoto(id) is null))
        {
            return ProviderErrors.InvalidPhotoOrder;
        }

        var ordered = photoIds.Select(id => FindPhoto(id)!)
            .Concat(_photos.Where(photo => !photoIds.Contains(photo.Id)).OrderBy(photo => photo.SortOrder))
            .ToList();
        foreach (var (photo, index) in ordered.Select((photo, index) => (photo, index)))
        {
            photo.MoveTo(index);
        }

        Touch(clock);
        return Result.Success();
    }

    public Result<PortfolioPhoto> DeletePhoto(Guid photoId, IClock clock)
    {
        if (FindPhoto(photoId) is not { } photo)
        {
            return ProviderErrors.PhotoNotFound;
        }

        _photos.Remove(photo);
        Touch(clock);
        return photo;
    }

    public void RequestAvatar(string uploadKey, IClock clock)
    {
        AvatarUploadKey = uploadKey;
        UpdatedAt = clock.UtcNow;
    }

    public Result<string?> CompleteAvatar(string uploadKey, string avatarKey, IClock clock)
    {
        if (AvatarUploadKey != uploadKey)
        {
            return ProviderErrors.UploadNotFound;
        }

        var previous = AvatarKey;
        AvatarKey = avatarKey;
        AvatarUploadKey = null;
        Touch(clock);
        return previous;
    }

    public void RejectAvatar(string uploadKey, IClock clock)
    {
        if (AvatarUploadKey == uploadKey)
        {
            AvatarUploadKey = null;
            UpdatedAt = clock.UtcNow;
        }
    }

    public string? RemoveAvatar(IClock clock)
    {
        var previous = AvatarKey;
        AvatarKey = null;
        Touch(clock);
        return previous;
    }

    public void Remove(IClock clock)
    {
        Status = ProviderStatus.Removed;
        AvailableToday = false;
        UpdatedAt = clock.UtcNow;
    }

    private static Result<string> NormalizeHeadline(string? headline)
    {
        var normalized = FreeText.Normalize(headline);
        if (normalized.Length is < HeadlineMinLength or > HeadlineMaxLength)
        {
            return ProviderErrors.InvalidHeadline;
        }

        if (FreeText.ContainsContactInformation(normalized))
        {
            return ProviderErrors.ContactInText;
        }

        return normalized;
    }

    private static bool Overlaps(IGrouping<DayOfWeek, AvailabilityDraft> day)
    {
        var ordered = day.OrderBy(draft => draft.StartMinute).ToList();
        return ordered.Count > AvailabilitySlot.MaxPerDay
            || ordered.Zip(ordered.Skip(1)).Any(pair => pair.Second.StartMinute < pair.First.EndMinute);
    }

    private PortfolioPhoto? FindPhoto(Guid photoId) => _photos.Find(photo => photo.Id == photoId);

    private void Touch(IClock clock)
    {
        UpdatedAt = clock.UtcNow;
        if (Status == ProviderStatus.Published)
        {
            Raise(new ProviderProfileUpdated(Id));
        }
    }
}
