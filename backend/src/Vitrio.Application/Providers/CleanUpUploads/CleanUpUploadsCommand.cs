using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.CleanUpUploads;

public sealed record CleanUpUploadsCommand : ICommand<Result>;
