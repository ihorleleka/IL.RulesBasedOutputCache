namespace IL.RulesBasedOutputCache.Models;

internal class SerializableOutputCacheEntry
{
    internal DateTimeOffset Created { get; set; }
    internal int StatusCode { get; set; }
    internal Dictionary<string, string?[]> Headers { get; set; } = default!;
    internal List<byte[]> Body { get; set; } = default!;
    internal string[] Tags { get; set; } = default!;
}