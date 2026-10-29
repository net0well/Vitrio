using Mediator;
using Vitrio.Application.Providers.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.VerifyMei;

public sealed record VerifyMeiCommand(string Cnpj) : ICommand<Result<MeiVerificationResponse>>;
