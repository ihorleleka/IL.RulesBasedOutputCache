namespace IL.RulesBasedOutputCache.Models;

internal sealed class SerializableOutputCacheEntry
{
    internal DateTimeOffset Created { get; set; }
    internal int StatusCode { get; set; }
    internal Dictionary<string, string?[]> Headers { get; set; } = null!;
    internal List<byte[]> Body { get; set; } = null!;
    internal string[] Tags { get; set; } = null!;
}