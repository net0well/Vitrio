namespace Vitrio.Application.Abstractions;

public interface IRefreshTokenFactory
{
    RefreshTokenValue Create();

    string Hash(string value);
}
