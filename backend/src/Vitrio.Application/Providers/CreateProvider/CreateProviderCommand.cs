using Mediator;
using Vitrio.Application.Providers.Common;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Providers.CreateProvider;

public sealed record CreateProviderCommand(string DisplayName, string Headline) : ICommand<Result<ProviderPrivateResponse>>;
