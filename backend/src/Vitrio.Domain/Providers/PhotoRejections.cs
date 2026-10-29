namespace Vitrio.Domain.Providers;

public static class PhotoRejections
{
    public const string NotAnImage = "photo.not_an_image";
    public const string TooLarge = "photo.too_large";

    public static string MessageFor(string? code) => code switch
    {
        TooLarge => "A foto passa de 10 MB ou de 8000 pixels. Envie uma menor.",
        _ => "O arquivo não é uma foto JPEG, PNG ou WebP."
    };
}
