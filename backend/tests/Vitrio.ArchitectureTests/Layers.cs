using System.Reflection;
using Vitrio.Api.Controllers;
using Vitrio.Domain.Common;

namespace Vitrio.ArchitectureTests;

internal static class Layers
{
    public static readonly Assembly Domain = typeof(Result).Assembly;
    public static readonly Assembly Application = typeof(Vitrio.Application.DependencyInjection).Assembly;
    public static readonly Assembly Infrastructure = typeof(Vitrio.Infrastructure.DependencyInjection).Assembly;
    public static readonly Assembly Api = typeof(ApiControllerBase).Assembly;
}
