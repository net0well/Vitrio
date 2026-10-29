using Vitrio.Domain.Common;

namespace Vitrio.Domain.Accounts;

public sealed class Address
{
    public const int MaxPerUser = 10;
    public const int LabelMaxLength = 40;
    public const int NumberMaxLength = 10;
    public const int ComplementMaxLength = 60;

    private Address()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string Label { get; private set; } = string.Empty;

    public string Cep { get; private set; } = string.Empty;

    public string Street { get; private set; } = string.Empty;

    public string Number { get; private set; } = string.Empty;

    public string? Complement { get; private set; }

    public string Neighborhood { get; private set; } = string.Empty;

    public string City { get; private set; } = string.Empty;

    public string State { get; private set; } = string.Empty;

    public int CityIbgeCode { get; private set; }

    public double? Latitude { get; private set; }

    public double? Longitude { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static Result<Address> Create(
        Guid userId,
        string? label,
        Cep cep,
        PostalLocation location,
        string? number,
        string? complement,
        IClock clock)
    {
        var now = clock.UtcNow;
        var address = new Address { Id = Guid.CreateVersion7(now), UserId = userId, CreatedAt = now };
        var update = address.Update(label, cep, location, number, complement, clock);
        return update.IsSuccess ? address : update.Error;
    }

    public Result Update(string? label, Cep cep, PostalLocation location, string? number, string? complement, IClock clock)
    {
        var normalizedLabel = FreeText.Normalize(label);
        if (normalizedLabel.Length is 0 or > LabelMaxLength || FreeText.ContainsContactInformation(normalizedLabel))
        {
            return AccountErrors.InvalidAddressLabel;
        }

        var normalizedNumber = FreeText.Normalize(number);
        if (normalizedNumber.Length is 0 or > NumberMaxLength)
        {
            return AccountErrors.InvalidAddressNumber;
        }

        var normalizedComplement = FreeText.Normalize(complement);
        if (normalizedComplement.Length > ComplementMaxLength || FreeText.ContainsContactInformation(normalizedComplement))
        {
            return AccountErrors.InvalidAddressComplement;
        }

        Label = normalizedLabel;
        Cep = cep.Value;
        Street = location.Street;
        Number = normalizedNumber;
        Complement = normalizedComplement.Length == 0 ? null : normalizedComplement;
        Neighborhood = location.Neighborhood;
        City = location.City;
        State = location.State;
        CityIbgeCode = location.CityIbgeCode;
        Latitude = location.Latitude;
        Longitude = location.Longitude;
        UpdatedAt = clock.UtcNow;
        return Result.Success();
    }
}
