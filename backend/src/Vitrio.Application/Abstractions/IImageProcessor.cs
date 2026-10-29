using Vitrio.Shared.Results;

namespace Vitrio.Application.Abstractions;

public interface IImageProcessor
{
    Result<ProcessedPhoto> ProcessPhoto(Stream image, string? declaredContentType);

    Result<byte[]> ProcessAvatar(Stream image, string? declaredContentType);
}
