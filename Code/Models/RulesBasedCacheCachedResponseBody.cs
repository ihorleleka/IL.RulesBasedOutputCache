using System.IO.Pipelines;

namespace IL.RulesBasedOutputCache.Models;

internal sealed class RulesBasedCacheCachedResponseBody
{
    internal RulesBasedCacheCachedResponseBody(List<byte[]> segments, long length)
    {
        ArgumentNullException.ThrowIfNull(segments);

        Segments = segments;
        Length = length;
    }

    /// <summary>
    /// Gets the segments of the body.
    /// </summary>
    internal List<byte[]> Segments { get; }

    /// <summary>
    /// Gets the length of the body.
    /// </summary>
    internal long Length { get; }

    /// <summary>
    /// Copies the body to a <see cref="PipeWriter"/>.
    /// </summary>
    /// <param name="destination">The destination</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    internal async Task CopyToAsync(PipeWriter destination, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(destination);

        foreach (var segment in Segments)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await destination.WriteAsync(segment, cancellationToken);
        }
    }
}