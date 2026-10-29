namespace Vitrio.Application.Abstractions;

public interface IPublicLinks
{
    string PlayStore { get; }

    string AndroidPackage { get; }

    IReadOnlyList<string> AndroidCertificateFingerprints { get; }

    string ProviderProfile(Guid providerId);
}
