using System.Reflection;
using Vitrio.Api.Controllers;
using Vitrio.Domain.Common;
using Vitrio.Shared.Results;

namespace Vitrio.ArchitectureTests;

internal static class Layers
{
    public static readonly Assembly Shared = typeof(Result).Assembly;
    public static readonly Assembly Domain = typeof(AggregateRoot).Assembly;
    public static readonly Assembly Application = typeof(Vitrio.Application.DependencyInjection).Assembly;
    public static readonly Assembly Infrastructure = typeof(Vitrio.Infrastructure.DependencyInjection).Assembly;
    public static readonly Assembly Api = typeof(ApiControllerBase).Assembly;
}
