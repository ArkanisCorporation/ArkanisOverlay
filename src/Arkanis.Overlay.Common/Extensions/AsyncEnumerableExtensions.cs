namespace Arkanis.Overlay.Common.Extensions;

using System.Runtime.CompilerServices;

public static class AsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<T[]> Batch<T>(
        this IAsyncEnumerable<T> source,
        int batchSize,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        await using var enumerator = source.GetAsyncEnumerator(cancellationToken);
        var currentBatch = new List<T>(batchSize);
        while (await enumerator.MoveNextAsync())
        {
            if (currentBatch.Count >= batchSize)
            {
                yield return currentBatch.ToArray();
                currentBatch.Clear();
            }

            currentBatch.Add(enumerator.Current);
        }

        if (currentBatch.Count <= 0)
        {
            yield break;
        }

        yield return currentBatch.ToArray();
    }
}
