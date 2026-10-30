using Vitrio.Domain.Providers;

namespace Vitrio.Application.Providers.Common;

public sealed record PhotoAcceptedResponse(Guid Id, PhotoStatus Status);
