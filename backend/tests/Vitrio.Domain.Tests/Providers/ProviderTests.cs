using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;
using Vitrio.Domain.Locations;
using Vitrio.Domain.Providers;

namespace Vitrio.Domain.Tests.Providers;

public sealed class ProviderTests
{
    private const string About = "Eletricista há 12 anos, faço instalações e reparos residenciais com garantia.";

    private static readonly DisplayName Name = DisplayName.Create("Carlos Eletricista").Value;
    private static readonly PhoneNumber Phone = PhoneNumber.Create("16 99123-4567").Value;
    private static readonly Coordinates Jaboticabal = new(-21.2547, -48.3221);

    private readonly FixedClock _clock = new(new DateTimeOffset(2026, 10, 29, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public void Create_StartsAsADraftWithTheAccountPhone()
    {
        var provider = Provider.Create(Guid.NewGuid(), Name, "Eletricista", Phone, _clock).Value;

        Assert.Equal(ProviderStatus.Draft, provider.Status);
        Assert.Equal("+5516991234567", provider.ContactPhone);
        Assert.Empty(provider.DomainEvents);
    }

    [Fact]
    public void Create_WithContactInTheHeadline_Fails()
    {
        var result = Provider.Create(Guid.NewGuid(), Name, "Eletricista 16 99123-4567", Phone, _clock);

        Assert.Equal(ProviderErrors.ContactInText, result.Error);
    }

    [Fact]
    public void Publish_WithoutAboutAndServices_ListsWhatIsMissing()
    {
        var provider = Provider.Create(Guid.NewGuid(), Name, "Eletricista", Phone, _clock).Value;
        provider.SetCategories([1], 1, _clock);
        provider.SetServiceArea(Address(), Jaboticabal, false, 10, [], _clock);

        var result = provider.Publish(_clock);

        var error = Assert.IsType<MissingFieldsError>(result.Error);
        Assert.Equal([PublishChecklist.About, PublishChecklist.Services], error.Missing);
        Assert.Equal(ProviderStatus.Draft, provider.Status);
    }

    [Fact]
    public void Publish_WhenComplete_PublishesAndRaisesTheEvent()
    {
        var provider = CompleteDraft();

        var result = provider.Publish(_clock);

        Assert.True(result.IsSuccess);
        Assert.Equal(ProviderStatus.Published, provider.Status);
        Assert.IsType<ProviderPublished>(Assert.Single(provider.DomainEvents));
    }

    [Fact]
    public void Publish_AfterPause_PublishesAgain()
    {
        var provider = CompleteDraft();
        provider.Publish(_clock);
        provider.Pause(_clock);
        provider.ClearDomainEvents();

        var result = provider.Publish(_clock);

        Assert.True(result.IsSuccess);
        Assert.Equal(ProviderStatus.Published, provider.Status);
        Assert.IsType<ProviderPublished>(Assert.Single(provider.DomainEvents));
    }

    [Fact]
    public void UpdateProfile_WithPhoneInAbout_Fails()
    {
        var provider = Provider.Create(Guid.NewGuid(), Name, "Eletricista", Phone, _clock).Value;

        var result = provider.UpdateProfile(Changes(about: "Me chame no zap 16 99123-4567 para orçamento rápido e sem compromisso."), _clock);

        Assert.Equal(ProviderErrors.ContactInText, result.Error);
        Assert.Null(provider.About);
    }

    [Fact]
    public void UpdateProfile_WhenPublished_RaisesProfileUpdated()
    {
        var provider = CompleteDraft();
        provider.Publish(_clock);
        provider.ClearDomainEvents();

        provider.UpdateProfile(Changes(about: About + " Atendo também condomínios."), _clock);

        Assert.IsType<ProviderProfileUpdated>(Assert.Single(provider.DomainEvents));
    }

    [Theory]
    [InlineData(new[] { 1, 2, 3, 4 }, 1)]
    [InlineData(new[] { 1, 2 }, 3)]
    [InlineData(new[] { 1, 1 }, 1)]
    [InlineData(new int[0], 1)]
    public void SetCategories_OutsideTheRules_Fails(int[] ids, int primary)
    {
        var provider = Provider.Create(Guid.NewGuid(), Name, "Eletricista", Phone, _clock).Value;

        var result = provider.SetCategories(ids, primary, _clock);

        Assert.Equal(ProviderErrors.InvalidCategories, result.Error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public void SetServiceArea_WithRadiusOutOfRange_Fails(int radiusKm)
    {
        var provider = Provider.Create(Guid.NewGuid(), Name, "Eletricista", Phone, _clock).Value;

        var result = provider.SetServiceArea(Address(), Jaboticabal, false, radiusKm, [], _clock);

        Assert.Equal(ProviderErrors.InvalidRadius, result.Error);
    }

    [Fact]
    public void SetServiceArea_KeepsOnlyThePublicCellOutsideTheEncryptedBase()
    {
        var provider = Provider.Create(Guid.NewGuid(), Name, "Eletricista", Phone, _clock).Value;

        provider.SetServiceArea(Address(), Jaboticabal, false, 15, ["Centro", "centro", "Jardim Nova Aparecida"], _clock);

        var cell = Jaboticabal.ToPublicCell();
        Assert.Equal(cell.Latitude, provider.PublicLatitude);
        Assert.Equal(cell.Longitude, provider.PublicLongitude);
        Assert.NotEqual(Jaboticabal.Latitude, provider.PublicLatitude);
        Assert.Equal(15_000, provider.ServiceRadiusMeters);
        Assert.Equal(["Centro", "Jardim Nova Aparecida"], provider.Neighborhoods.Select(neighborhood => neighborhood.Name));
    }

    [Fact]
    public void SetServices_WithPriceAndOnQuoteTogether_Fails()
    {
        var provider = Provider.Create(Guid.NewGuid(), Name, "Eletricista", Phone, _clock).Value;

        var result = provider.SetServices([new ServiceDraft("Troca de chuveiro", null, new Money(8000), PriceUnit.PerService, true)], _clock);

        Assert.Equal(ProviderErrors.InvalidPrice, result.Error);
    }

    [Theory]
    [InlineData(99)]
    [InlineData(10_000_001)]
    public void SetServices_WithPriceOutOfRange_Fails(long cents)
    {
        var provider = Provider.Create(Guid.NewGuid(), Name, "Eletricista", Phone, _clock).Value;

        var result = provider.SetServices([new ServiceDraft("Troca de chuveiro", null, new Money(cents), PriceUnit.PerService, false)], _clock);

        Assert.Equal(ProviderErrors.InvalidPrice, result.Error);
    }

    [Fact]
    public void SetAvailability_WithOverlappingSlots_Fails()
    {
        var provider = Provider.Create(Guid.NewGuid(), Name, "Eletricista", Phone, _clock).Value;

        var result = provider.SetAvailability(
            [new AvailabilityDraft(DayOfWeek.Monday, 480, 720), new AvailabilityDraft(DayOfWeek.Monday, 690, 1080)],
            false,
            _clock);

        Assert.Equal(ProviderErrors.InvalidAvailability, result.Error);
    }

    [Fact]
    public void SetAvailability_AcceptsUntilMidnightOnTheHalfHourGrid()
    {
        var provider = Provider.Create(Guid.NewGuid(), Name, "Eletricista", Phone, _clock).Value;

        var result = provider.SetAvailability(
            [new AvailabilityDraft(DayOfWeek.Friday, 1080, 1440), new AvailabilityDraft(DayOfWeek.Friday, 480, 720)],
            true,
            _clock);

        Assert.True(result.IsSuccess);
        Assert.Equal([480, 1080], provider.Availability.Select(slot => slot.StartMinute));
        Assert.True(provider.HandlesEmergencies);
    }

    [Fact]
    public void SetAvailableToday_OnADraft_Fails()
    {
        var provider = Provider.Create(Guid.NewGuid(), Name, "Eletricista", Phone, _clock).Value;

        Assert.Equal(ProviderErrors.NotPublished, provider.SetAvailableToday(true, _clock).Error);
    }

    [Fact]
    public void Pause_TurnsOffAvailableTodayAndRaisesTheEvent()
    {
        var provider = CompleteDraft();
        provider.Publish(_clock);
        provider.SetAvailableToday(true, _clock);
        provider.ClearDomainEvents();

        provider.Pause(_clock);

        Assert.Equal(ProviderStatus.Paused, provider.Status);
        Assert.False(provider.AvailableToday);
        Assert.IsType<ProviderPaused>(Assert.Single(provider.DomainEvents));
    }

    private Provider CompleteDraft()
    {
        var provider = Provider.Create(Guid.NewGuid(), Name, "Eletricista", Phone, _clock).Value;
        provider.UpdateProfile(Changes(about: About), _clock);
        provider.SetCategories([1, 2], 1, _clock);
        provider.SetServiceArea(Address(), Jaboticabal, false, 10, ["Centro"], _clock);
        provider.SetServices([new ServiceDraft("Troca de chuveiro", null, new Money(8000), PriceUnit.PerService, false)], _clock);
        return provider;
    }

    private static BaseAddress Address() => new("14870000", "Rua Rui Barbosa", "120", null, "Centro", 3524303);

    private static ProfileChanges Changes(string? about = null) => new(null, null, about, null, null, null, null);

    private sealed class FixedClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow => now;
    }
}
