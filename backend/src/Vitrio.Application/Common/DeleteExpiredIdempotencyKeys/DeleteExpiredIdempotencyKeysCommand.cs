using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Common.DeleteExpiredIdempotencyKeys;

public sealed record DeleteExpiredIdempotencyKeysCommand : ICommand<Result>;
