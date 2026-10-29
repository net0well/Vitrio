using SkiaSharp;

namespace Vitrio.Api.IntegrationTests.Infrastructure;

public static class TestImages
{
    public static byte[] Jpeg(int width = 1200, int height = 800) => Encode(width, height, SKEncodedImageFormat.Jpeg);

    public static byte[] Png(int width = 900, int height = 600) => Encode(width, height, SKEncodedImageFormat.Png);

    public static byte[] JpegWithGpsExif()
    {
        var jpeg = Jpeg();
        var exif = GpsExifSegment();
        return [.. jpeg[..2], .. exif, .. jpeg[2..]];
    }

    private static byte[] Encode(int width, int height, SKEncodedImageFormat format)
    {
        using var bitmap = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(new SKColor(0x3D, 0x1F, 0x56));
            using var paint = new SKPaint { Color = new SKColor(0xC6, 0xF0, 0x5A) };
            canvas.DrawCircle(width / 2f, height / 2f, Math.Min(width, height) / 3f, paint);
        }

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(format, 90);
        return data.ToArray();
    }

    private static byte[] GpsExifSegment()
    {
        var tiff = new List<byte> { (byte)'I', (byte)'I', 42, 0, 8, 0, 0, 0 };
        AddShort(tiff, 1);
        AddEntry(tiff, 0x8825, 4, 1, 26);
        AddInt(tiff, 0);
        AddShort(tiff, 2);
        AddEntry(tiff, 0x0001, 2, 2, BitConverter.ToUInt32([(byte)'S', 0, 0, 0]));
        AddEntry(tiff, 0x0002, 5, 3, 56);
        AddInt(tiff, 0);
        foreach (var (numerator, denominator) in new[] { (21u, 1u), (15u, 1u), (1692u, 100u) })
        {
            AddInt(tiff, numerator);
            AddInt(tiff, denominator);
        }

        var payload = new List<byte> { (byte)'E', (byte)'x', (byte)'i', (byte)'f', 0, 0 };
        payload.AddRange(tiff);
        var length = payload.Count + 2;
        return [0xFF, 0xE1, (byte)(length >> 8), (byte)length, .. payload];
    }

    private static void AddEntry(List<byte> bytes, ushort tag, ushort type, uint count, uint value)
    {
        AddShort(bytes, tag);
        AddShort(bytes, type);
        AddInt(bytes, count);
        AddInt(bytes, value);
    }

    private static void AddShort(List<byte> bytes, ushort value) => bytes.AddRange(BitConverter.GetBytes(value));

    private static void AddInt(List<byte> bytes, uint value) => bytes.AddRange(BitConverter.GetBytes(value));
}
