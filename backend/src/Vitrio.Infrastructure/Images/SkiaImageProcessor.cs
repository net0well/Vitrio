using SkiaSharp;
using Vitrio.Application.Abstractions;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Results;

namespace Vitrio.Infrastructure.Images;

internal sealed class SkiaImageProcessor : IImageProcessor
{
    private const int MaxSourcePixels = 8000;
    private const int FullSize = 2048;
    private const int ThumbnailSize = 480;
    private const int AvatarSize = 512;
    private const int ThumbnailMaxBytes = 60 * 1024;
    private const int Quality = 82;
    private const int MinThumbnailQuality = 40;

    private static readonly Dictionary<SKEncodedImageFormat, string> AcceptedFormats = new()
    {
        [SKEncodedImageFormat.Jpeg] = "image/jpeg",
        [SKEncodedImageFormat.Png] = "image/png",
        [SKEncodedImageFormat.Webp] = "image/webp"
    };

    public Result<ProcessedPhoto> ProcessPhoto(Stream image, string? declaredContentType)
    {
        var decoded = Decode(image, declaredContentType);
        if (decoded.IsFailure)
        {
            return decoded.Error!;
        }

        using var source = decoded.Value;
        using var full = Fit(source, FullSize);
        using var thumbnail = Fit(source, ThumbnailSize);
        return new ProcessedPhoto(Encode(full, Quality), EncodeWithin(thumbnail, ThumbnailMaxBytes));
    }

    public Result<byte[]> ProcessAvatar(Stream image, string? declaredContentType)
    {
        var decoded = Decode(image, declaredContentType);
        if (decoded.IsFailure)
        {
            return decoded.Error!;
        }

        using var source = decoded.Value;
        using var square = CropSquare(source, AvatarSize);
        return Encode(square, Quality);
    }

    private static Result<SKBitmap> Decode(Stream image, string? declaredContentType)
    {
        using var data = SKData.Create(image);
        using var codec = data is null ? null : SKCodec.Create(data);
        if (codec is null
            || !AcceptedFormats.TryGetValue(codec.EncodedFormat, out var detected)
            || !string.Equals(detected, declaredContentType, StringComparison.OrdinalIgnoreCase))
        {
            return Rejected(PhotoRejections.NotAnImage);
        }

        if (codec.Info.Width > MaxSourcePixels || codec.Info.Height > MaxSourcePixels)
        {
            return Rejected(PhotoRejections.TooLarge);
        }

        var bitmap = SKBitmap.Decode(codec);
        if (bitmap is null)
        {
            return Rejected(PhotoRejections.NotAnImage);
        }

        return Orient(bitmap, codec.EncodedOrigin);
    }

    private static Error Rejected(string code) => Error.Validation(code, PhotoRejections.MessageFor(code));

    private static SKBitmap Orient(SKBitmap bitmap, SKEncodedOrigin origin)
    {
        if (origin == SKEncodedOrigin.TopLeft)
        {
            return bitmap;
        }

        var swaps = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
        var oriented = new SKBitmap(swaps ? bitmap.Height : bitmap.Width, swaps ? bitmap.Width : bitmap.Height);
        using (var canvas = new SKCanvas(oriented))
        {
            canvas.SetMatrix(origin switch
            {
                SKEncodedOrigin.TopRight => SKMatrix.CreateScale(-1, 1, bitmap.Width / 2f, 0),
                SKEncodedOrigin.BottomRight => SKMatrix.CreateRotationDegrees(180, bitmap.Width / 2f, bitmap.Height / 2f),
                SKEncodedOrigin.BottomLeft => SKMatrix.CreateScale(1, -1, 0, bitmap.Height / 2f),
                SKEncodedOrigin.LeftTop => SKMatrix.CreateScale(-1, 1).PostConcat(SKMatrix.CreateRotationDegrees(90)).PostConcat(SKMatrix.CreateTranslation(bitmap.Height, bitmap.Width)),
                SKEncodedOrigin.RightTop => SKMatrix.CreateRotationDegrees(90).PostConcat(SKMatrix.CreateTranslation(bitmap.Height, 0)),
                SKEncodedOrigin.RightBottom => SKMatrix.CreateScale(-1, 1).PostConcat(SKMatrix.CreateRotationDegrees(-90)),
                SKEncodedOrigin.LeftBottom => SKMatrix.CreateRotationDegrees(-90).PostConcat(SKMatrix.CreateTranslation(0, bitmap.Width)),
                _ => SKMatrix.Identity
            });
            using var image = SKImage.FromBitmap(bitmap);
            canvas.DrawImage(image, 0, 0, new SKSamplingOptions(SKFilterMode.Linear));
        }

        bitmap.Dispose();
        return oriented;
    }

    private static SKBitmap Fit(SKBitmap source, int maxSide)
    {
        var scale = Math.Min(1, (double)maxSide / Math.Max(source.Width, source.Height));
        var info = new SKImageInfo(Math.Max(1, (int)Math.Round(source.Width * scale)), Math.Max(1, (int)Math.Round(source.Height * scale)));
        return source.Resize(info, new SKSamplingOptions(SKCubicResampler.Mitchell));
    }

    private static SKBitmap CropSquare(SKBitmap source, int size)
    {
        var side = Math.Min(source.Width, source.Height);
        var square = new SKBitmap(size, size);
        using var canvas = new SKCanvas(square);
        using var image = SKImage.FromBitmap(source);
        var crop = SKRect.Create((source.Width - side) / 2f, (source.Height - side) / 2f, side, side);
        canvas.DrawImage(image, crop, SKRect.Create(size, size), new SKSamplingOptions(SKCubicResampler.Mitchell));
        return square;
    }

    private static byte[] Encode(SKBitmap bitmap, int quality)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Webp, quality);
        return data.ToArray();
    }

    private static byte[] EncodeWithin(SKBitmap bitmap, int maxBytes)
    {
        var quality = Quality;
        var encoded = Encode(bitmap, quality);
        while (encoded.Length > maxBytes && quality > MinThumbnailQuality)
        {
            quality -= 10;
            encoded = Encode(bitmap, quality);
        }

        return encoded;
    }
}
