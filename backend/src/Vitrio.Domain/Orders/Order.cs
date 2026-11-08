using Vitrio.Domain.Common;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Domain.Orders;

public sealed class Order : AggregateRoot
{
    public const int MaxPhotos = 5;
    public const int MaxReschedules = 2;
    public const int MaxDaysAhead = 30;
    public const int MaxReasonDetailsLength = 200;

    private const int MinTitleLength = 5;
    private const int MaxTitleLength = 80;
    private const int MinDescriptionLength = 20;
    private const int MaxDescriptionLength = 500;

    private static readonly TimeSpan ResponseLimit = TimeSpan.FromHours(48);
    private static readonly TimeSpan QuoteValidity = TimeSpan.FromHours(48);
    private static readonly TimeSpan MinimumNotice = TimeSpan.FromHours(1);
    private static readonly TimeSpan LateCancellationWindow = TimeSpan.FromHours(2);
    private static readonly TimeSpan ContactWindowAfterService = TimeSpan.FromDays(7);
    private static readonly TimeSpan AutomaticCompletionAfter = TimeSpan.FromDays(7);
    private static readonly TimeSpan PersonalDataKeptFor = TimeSpan.FromDays(90);

    private readonly List<OrderPhoto> _photos = [];

    private Order()
    {
    }

    public Guid Id { get; private set; }

    public Guid CustomerId { get; private set; }

    public Guid ProviderId { get; private set; }

    public Guid? ServiceId { get; private set; }

    public string ServiceTitle { get; private set; } = string.Empty;

    public int CategoryId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public DateOnly PreferredDay { get; private set; }

    public PreferredPeriod PreferredPeriod { get; private set; }

    public OrderAddress? Address { get; private set; }

    public OrderContact? Contact { get; private set; }

    public string CustomerDisplayName { get; private set; } = string.Empty;

    public OrderPlace Place { get; private set; } = null!;

    public OrderStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset RespondBy { get; private set; }

    public DateTimeOffset? FirstResponseAt { get; private set; }

    public DeclineReason? DeclineReason { get; private set; }

    public string? DeclineDetails { get; private set; }

    public Quote? Quote { get; private set; }

    public DateTimeOffset? QuoteValidUntil { get; private set; }

    public ServiceSchedule? Schedule { get; private set; }

    public int RescheduleCount { get; private set; }

    public DateTimeOffset? AddressReleasedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public OrderParty? CompletedBy { get; private set; }

    public DateTimeOffset? CancelledAt { get; private set; }

    public OrderParty? CancelledBy { get; private set; }

    public CancellationReason? CancellationReason { get; private set; }

    public string? CancellationDetails { get; private set; }

    public DateTimeOffset? ClosedAt { get; private set; }

    public DateTimeOffset? PersonalDataErasedAt { get; private set; }

    public IReadOnlyList<OrderPhoto> Photos => _photos;

    public bool IsOpen => Status is OrderStatus.Sent or OrderStatus.Quoted or OrderStatus.Scheduled;

    public bool ServiceWasDone =>
        Status is OrderStatus.Completed or OrderStatus.PaymentConfirmed or OrderStatus.PaymentDisputed or OrderStatus.PaymentVoided;

    public DateTimeOffset? ServiceEndedAt => ServiceWasDone ? Schedule?.EndsAt : ClosedAt;

    public static Result<Order> Create(Guid customerId, Guid providerId, OrderRequest request, IClock clock)
    {
        var now = clock.UtcNow;
        var serviceTitle = FreeText.Normalize(request.ServiceTitle);
        if (serviceTitle.Length is 0 or > MaxTitleLength)
        {
            return OrderErrors.MissingService;
        }

        var title = FreeText.Normalize(request.Title);
        if (title.Length is < MinTitleLength or > MaxTitleLength)
        {
            return OrderErrors.InvalidTitle;
        }

        var description = FreeText.Normalize(request.Description);
        if (description.Length is < MinDescriptionLength or > MaxDescriptionLength)
        {
            return OrderErrors.InvalidDescription;
        }

        if (request.PhotoUploadKeys.Count > MaxPhotos)
        {
            return OrderErrors.TooManyPhotos;
        }

        var zone = CityTime.Zone(request.Place.TimeZoneId);
        var today = CityTime.Today(now, zone);
        if (request.PreferredDay < today || request.PreferredDay > today.AddDays(MaxDaysAhead))
        {
            return OrderErrors.InvalidPreferredDay;
        }

        var periodEnd = CityTime.ToUtc(request.PreferredDay, PreferredPeriods.EndOf(request.PreferredPeriod), zone);
        if (periodEnd <= now)
        {
            return OrderErrors.PeriodAlreadyOver;
        }

        var order = new Order
        {
            Id = Guid.CreateVersion7(),
            CustomerId = customerId,
            ProviderId = providerId,
            ServiceId = request.ServiceId,
            ServiceTitle = serviceTitle,
            CategoryId = request.CategoryId,
            Title = title,
            Description = description,
            PreferredDay = request.PreferredDay,
            PreferredPeriod = request.PreferredPeriod,
            Address = request.Address,
            Contact = request.Contact,
            CustomerDisplayName = ShortPersonName.From(request.Contact.FullName),
            Place = request.Place,
            Status = OrderStatus.Sent,
            CreatedAt = now,
            RespondBy = periodEnd < now + ResponseLimit ? periodEnd : now + ResponseLimit
        };
        foreach (var uploadKey in request.PhotoUploadKeys)
        {
            order._photos.Add(OrderPhoto.Create(order.Id, uploadKey, now));
        }

        order.Raise(new OrderCreated(order.Id, customerId, providerId));
        return order;
    }

    public Result SendQuote(QuoteTerms terms, IClock clock)
    {
        var now = clock.UtcNow;
        if (Status is not (OrderStatus.Sent or OrderStatus.Quoted))
        {
            return OrderErrors.CannotQuote;
        }

        if (Status == OrderStatus.Sent && now >= RespondBy)
        {
            return OrderErrors.ResponseWindowClosed;
        }

        var quote = Quote.Create(terms, Place.TimeZoneId, (Quote?.Version ?? 0) + 1, now);
        if (quote.IsFailure)
        {
            return quote.Error!;
        }

        var lastMomentToAccept = quote.Value.Schedule.StartsAt - MinimumNotice;
        if (lastMomentToAccept <= now)
        {
            return OrderErrors.StartsTooSoon;
        }

        Quote = quote.Value;
        QuoteValidUntil = now + QuoteValidity < lastMomentToAccept ? now + QuoteValidity : lastMomentToAccept;
        FirstResponseAt ??= now;
        Status = OrderStatus.Quoted;
        Raise(new QuoteSent(Id, CustomerId, ProviderId, Quote.Version));
        return Result.Success();
    }

    public Result Decline(DeclineReason reason, string? details, IClock clock)
    {
        var now = clock.UtcNow;
        if (Status != OrderStatus.Sent)
        {
            return OrderErrors.CannotDecline;
        }

        if (now >= RespondBy)
        {
            return OrderErrors.ResponseWindowClosed;
        }

        string? normalizedDetails = null;
        if (reason == Orders.DeclineReason.Other)
        {
            normalizedDetails = FreeText.Normalize(details);
            if (normalizedDetails.Length is 0 or > MaxReasonDetailsLength)
            {
                return OrderErrors.DeclineDetailsRequired;
            }

            if (FreeText.ContainsContactInformation(normalizedDetails))
            {
                return OrderErrors.ContactInDetails;
            }
        }

        Status = OrderStatus.Declined;
        DeclineReason = reason;
        DeclineDetails = normalizedDetails;
        FirstResponseAt ??= now;
        ClosedAt = now;
        Raise(new OrderDeclined(Id, CustomerId, ProviderId, reason));
        return Result.Success();
    }

    public Result AcceptQuote(int quoteVersion, IClock clock)
    {
        var now = clock.UtcNow;
        if (Status != OrderStatus.Quoted || Quote is null)
        {
            return OrderErrors.CannotAnswerQuote;
        }

        if (quoteVersion != Quote.Version)
        {
            return OrderErrors.QuoteVersionMismatch;
        }

        if (now >= QuoteValidUntil)
        {
            return OrderErrors.QuoteExpired;
        }

        Status = OrderStatus.Scheduled;
        Schedule = Quote.Schedule.Copy();
        AddressReleasedAt = now;
        Raise(new QuoteAccepted(Id, CustomerId, ProviderId, Quote.Version));
        Raise(new AddressReleased(Id, ProviderId));
        return Result.Success();
    }

    public Result DeclineQuote(IClock clock)
    {
        if (Status != OrderStatus.Quoted)
        {
            return OrderErrors.CannotAnswerQuote;
        }

        Status = OrderStatus.QuoteDeclined;
        ClosedAt = clock.UtcNow;
        Raise(new QuoteDeclined(Id, CustomerId, ProviderId));
        return Result.Success();
    }

    public Result Reschedule(DateOnly date, TimeOnly start, int durationMinutes, IClock clock)
    {
        var now = clock.UtcNow;
        if (Status != OrderStatus.Scheduled || Schedule is null || now >= Schedule.StartsAt)
        {
            return OrderErrors.CannotReschedule;
        }

        if (RescheduleCount >= MaxReschedules)
        {
            return OrderErrors.RescheduleLimitReached;
        }

        var schedule = ServiceSchedule.Create(date, start, durationMinutes, Place.TimeZoneId);
        if (schedule.IsFailure)
        {
            return schedule.Error!;
        }

        if (schedule.Value.StartsAt - MinimumNotice < now)
        {
            return OrderErrors.StartsTooSoon;
        }

        Schedule = schedule.Value;
        RescheduleCount++;
        Raise(new OrderRescheduled(Id, CustomerId, ProviderId, RescheduleCount));
        return Result.Success();
    }

    public Result Cancel(OrderParty by, CancellationReason reason, string? details, IClock clock)
    {
        var now = clock.UtcNow;
        if (!CancellationReasons.Allows(by, reason))
        {
            return OrderErrors.InvalidCancellationReason;
        }

        var allowed = by switch
        {
            OrderParty.Provider => Status == OrderStatus.Scheduled,
            _ => IsOpen
        };
        if (!allowed)
        {
            return OrderErrors.CannotCancel;
        }

        var normalizedDetails = FreeText.Normalize(details);
        if (normalizedDetails.Length > MaxReasonDetailsLength || (reason == Orders.CancellationReason.Other && normalizedDetails.Length == 0))
        {
            return OrderErrors.CancellationDetailsRequired;
        }

        Status = OrderStatus.Cancelled;
        CancelledAt = now;
        CancelledBy = by;
        CancellationReason = reason;
        CancellationDetails = normalizedDetails.Length > 0 ? normalizedDetails : null;
        ClosedAt = now;
        Raise(new OrderCancelled(Id, CustomerId, ProviderId, by, reason, WasCancelledLate));
        return Result.Success();
    }

    public Result Complete(OrderParty by, IClock clock)
    {
        var now = clock.UtcNow;
        if (Status != OrderStatus.Scheduled || Schedule is null)
        {
            return OrderErrors.CannotComplete;
        }

        if (now < Schedule.StartsAt)
        {
            return OrderErrors.CompleteBeforeStart;
        }

        MarkCompleted(by, now);
        return Result.Success();
    }

    public bool CompleteForgotten(IClock clock)
    {
        var now = clock.UtcNow;
        if (Status != OrderStatus.Scheduled || Schedule is null || now < Schedule.EndsAt + AutomaticCompletionAfter)
        {
            return false;
        }

        MarkCompleted(OrderParty.System, now);
        return true;
    }

    public bool Expire(IClock clock)
    {
        var now = clock.UtcNow;
        if (Status == OrderStatus.Sent && now >= RespondBy)
        {
            Status = OrderStatus.Expired;
            ClosedAt = now;
            Raise(new OrderExpired(Id, CustomerId, ProviderId));
            return true;
        }

        if (Status == OrderStatus.Quoted && now >= QuoteValidUntil)
        {
            Status = OrderStatus.QuoteExpired;
            ClosedAt = now;
            Raise(new QuoteExpired(Id, CustomerId, ProviderId));
            return true;
        }

        return false;
    }

    public bool WasCancelledLate =>
        CancelledBy == OrderParty.Provider && Schedule is not null && CancelledAt is { } at && Schedule.StartsAt - at < LateCancellationWindow;

    public bool IsContactVisibleToProvider(DateTimeOffset now) =>
        IsContactVisible(AddressReleasedAt, Status, PersonalDataErasedAt, Schedule?.EndsAt, now);

    public static bool IsContactVisible(
        DateTimeOffset? addressReleasedAt,
        OrderStatus status,
        DateTimeOffset? personalDataErasedAt,
        DateTimeOffset? scheduledEnd,
        DateTimeOffset now) =>
        addressReleasedAt is not null
        && status != OrderStatus.Cancelled
        && personalDataErasedAt is null
        && scheduledEnd is { } end
        && now < end + ContactWindowAfterService;

    public DateTimeOffset? ContactVisibleUntil =>
        AddressReleasedAt is not null && Schedule is not null ? Schedule.EndsAt + ContactWindowAfterService : null;

    public bool CanErasePersonalData(DateTimeOffset now) =>
        PersonalDataErasedAt is null && !IsOpen && ServiceEndedAt is { } endedAt && now >= endedAt + PersonalDataKeptFor;

    public IReadOnlyList<string> ErasePersonalData(IClock clock)
    {
        var now = clock.UtcNow;
        if (!CanErasePersonalData(now))
        {
            return [];
        }

        var storedPhotos = _photos.Where(photo => photo.ObjectKey is not null).Select(photo => photo.ObjectKey!).ToList();
        foreach (var photo in _photos)
        {
            photo.MarkRemoved();
        }

        Address = null;
        Contact = null;
        PersonalDataErasedAt = now;
        return storedPhotos;
    }

    public Result CompletePhoto(Guid photoId, string objectKey)
    {
        if (_photos.SingleOrDefault(photo => photo.Id == photoId) is not { Status: OrderPhotoStatus.Processing } photo)
        {
            return OrderErrors.PhotoNotFound;
        }

        photo.MarkReady(objectKey);
        return Result.Success();
    }

    public Result FailPhoto(Guid photoId)
    {
        if (_photos.SingleOrDefault(photo => photo.Id == photoId) is not { Status: OrderPhotoStatus.Processing } photo)
        {
            return OrderErrors.PhotoNotFound;
        }

        photo.MarkFailed();
        return Result.Success();
    }

    public OrderState State => new(Status, RespondBy, QuoteValidUntil, Schedule?.StartsAt, RescheduleCount);

    public IReadOnlyList<OrderAction> AllowedActionsFor(OrderParty party, DateTimeOffset now) => State.AllowedActionsFor(party, now);

    private void MarkCompleted(OrderParty by, DateTimeOffset now)
    {
        Status = OrderStatus.Completed;
        CompletedAt = now;
        CompletedBy = by;
        Raise(new OrderCompleted(Id, CustomerId, ProviderId, by));
    }
}
