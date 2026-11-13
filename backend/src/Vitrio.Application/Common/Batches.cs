namespace Vitrio.Application.Common;

public static class Batches
{
    // The batch query only returns what is still due, so each finished item leaves it; the ones that could not change are
    // excluded from the next query instead, which makes the loop end even when an item keeps failing.
    public static async Task DrainAsync(
        Func<IReadOnlyCollection<Guid>, CancellationToken, Task<IReadOnlyList<Guid>>> nextBatch,
        Func<Guid, CancellationToken, Task<bool>> process,
        CancellationToken cancellationToken)
    {
        var skipped = new HashSet<Guid>();
        while (true)
        {
            var batch = await nextBatch(skipped, cancellationToken);
            if (batch.Count == 0)
            {
                return;
            }

            foreach (var id in batch)
            {
                if (!await process(id, cancellationToken))
                {
                    skipped.Add(id);
                }
            }
        }
    }
}
