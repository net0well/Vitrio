using Vitrio.Domain.Common;

namespace Vitrio.Domain.Providers;

public sealed class PortfolioPhoto
{
    public const int CaptionMaxLength = 60;

    private PortfolioPhoto()
    {
    }

    public Guid Id { get; private set; }

    public Guid ProviderId { get; private set; }

    public string? UploadKey { get; private set; }

    public string? ObjectKey { get; private set; }

    public string? ThumbnailKey { get; private set; }

    public string? Caption { get; private set; }

    public int SortOrder { get; private set; }

    public PhotoStatus Status { get; private set; }

    public string? RejectionReason { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    internal static string? NormalizeCaption(string? caption, out bool valid)
    {
        var normalized = FreeText.Normalize(caption);
        valid = normalized.Length <= CaptionMaxLength && !FreeText.ContainsContactInformation(normalized);
        return normalized.Length == 0 ? null : normalized;
    }

    internal static PortfolioPhoto Create(Guid providerId, string uploadKey, string? caption, int sortOrder, DateTimeOffset now) => new()
    {
        Id = Guid.CreateVersion7(now),
        ProviderId = providerId,
        UploadKey = uploadKey,
        Caption = caption,
        SortOrder = sortOrder,
        Status = PhotoStatus.Processing,
        CreatedAt = now
    };

    internal void Activate(string objectKey, string thumbnailKey)
    {
        ObjectKey = objectKey;
        ThumbnailKey = thumbnailKey;
        UploadKey = null;
        Status = PhotoStatus.Active;
    }

    internal void Reject(string reason)
    {
        UploadKey = null;
        RejectionReason = reason;
        Status = PhotoStatus.Rejected;
    }

    internal void Rename(string? caption) => Caption = caption;

    internal void MoveTo(int sortOrder) => SortOrder = sortOrder;
}
