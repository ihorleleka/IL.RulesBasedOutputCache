using System.Globalization;
using System.Text;
using IL.Misc.Concurrency;
using IL.RulesBasedOutputCache.Extensions;
using IL.RulesBasedOutputCache.Helpers;
using IL.RulesBasedOutputCache.Models;
using IL.RulesBasedOutputCache.Models.Interfaces;
using IL.RulesBasedOutputCache.Persistence.Rules.Interfaces;
using IL.RulesBasedOutputCache.Settings;
using IL.RulesBasedOutputCache.StreamExt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace IL.RulesBasedOutputCache.Middleware;

internal sealed class RulesBasedOutputCacheMiddleware
{
    // see https://tools.ietf.org/html/rfc7232#section-4.1
    private static readonly string[] HeadersToIncludeIn304 =
        ["Cache-Control", "Content-Location", "Date", "ETag", "Expires", "Vary"];

    private static int BodySegmentSize { get; set; } = 81920;

    private readonly RequestDelegate _next;
    private readonly IOutputCacheStore _store;
    private readonly IServiceProvider _serviceProvider;
    private readonly RulesBasedOutputCacheConfiguration _cacheConfiguration;

    public RulesBasedOutputCacheMiddleware(
        RequestDelegate next,
        IOutputCacheStore store,
        IServiceProvider serviceProvider,
        IOptions<RulesBasedOutputCacheConfiguration> cacheConfiguration)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(cacheConfiguration.Value);

        _next = next;
        _store = store;
        _serviceProvider = serviceProvider;
        _cacheConfiguration = cacheConfiguration.Value;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        if (!_cacheConfiguration.OutputCacheEnabled)
        {
            await _next(httpContext);
            return;
        }

        var context = new RulesBasedOutputCacheContext { HttpContext = httpContext };
        AddOutputCacheFeature(context);
        await FindMatchingCachingRuleAndSetVariables(context);

        if (!context.UseOutputCaching)
        {
            await _next(context.HttpContext);
            return;
        }

        //prevent admin panel from being cached
        if (!string.IsNullOrEmpty(context.HttpContext.Request.Path.Value)
            && context.HttpContext.Request.Path.Value.StartsWith(_cacheConfiguration.AdminPanel.AdminPanelUrl))
        {
            await _next(context.HttpContext);
            return;
        }

        try
        {
            using (await LockManager.GetLockAsync(context.CacheKey))
            {
                if (await TryServeFromCacheAsync(context))
                {
                    return;
                }

                InterceptResponseStream(context);
                await _next(httpContext);
                try
                {
                    await FinalizeCacheBodyAsync(context);
                }
                catch
                {
                    //ignore
                }

                RestoreResponseStream(context);
            }
        }
        finally
        {
            RemoveOutputCacheFeature(httpContext);
        }
    }

    private static void AddOutputCacheFeature(RulesBasedOutputCacheContext context)
    {
        if (context.HttpContext.Features.Get<IRulesBasedOutputCacheFeature>() != null)
        {
            throw new InvalidOperationException(
                $"Another instance of {nameof(RulesBasedOutputCacheContext)} already exists. Only one instance of {nameof(RulesBasedOutputCacheContext)} can be configured for an application.");
        }

        context.HttpContext.Features.Set<IRulesBasedOutputCacheFeature>(context);
    }

    private async Task FindMatchingCachingRuleAndSetVariables(RulesBasedOutputCacheContext context)
    {
        List<CachingRule> rules;
        using (var scope = _serviceProvider.CreateScope())
        {
            var rulesRepository = scope.ServiceProvider.GetRequiredService<IRulesRepository>();
            rules = await rulesRepository.GetAll();
        }

        var matchingRule = rules.FirstOrDefault(x => x.MatchesCurrentRequest(context.HttpContext.Request.Path.Value!));
        if (matchingRule == null || matchingRule.RuleAction == RuleAction.Disallow)
        {
            return;
        }

        if (!string.IsNullOrEmpty(matchingRule.ResponseExpirationTimeSpan))
        {
            context.ResponseExpirationTimeSpan = TimeSpan.Parse(matchingRule.ResponseExpirationTimeSpan);
        }

        CreateCacheKey(context, matchingRule);
        context.UseOutputCaching = !string.IsNullOrEmpty(context.CacheKey);
    }

    private async Task<bool> TryServeFromCacheAsync(RulesBasedOutputCacheContext cacheContext)
    {
        var cacheEntry = await RulesBasedOutputCacheEntrySerializer.GetAsync(cacheContext.CacheKey, _store,
            cacheContext.HttpContext.RequestAborted);

        if (await TryServeCachedResponseAsync(cacheContext, cacheEntry))
        {
            return true;
        }

        if (HeaderUtilities.ContainsCacheDirective(cacheContext.HttpContext.Request.Headers.CacheControl,
                CacheControlHeaderValue.OnlyIfCachedString))
        {
            cacheContext.HttpContext.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
            return true;
        }

        return false;
    }

    private void InterceptResponseStream(RulesBasedOutputCacheContext context)
    {
        // Shim response stream
        context.OriginalResponseStream = context.HttpContext.Response.Body;
        context.OutputCacheStream = new RulesBasedOutputCacheStream(
            context.OriginalResponseStream,
            _cacheConfiguration.MaximumBodySize,
            BodySegmentSize,
            () => StartResponse(context));
        context.HttpContext.Response.Body = context.OutputCacheStream;
    }

    /// <summary>
    /// Stores the response body
    /// </summary>
    private async ValueTask FinalizeCacheBodyAsync(RulesBasedOutputCacheContext context)
    {
        //Add to cache only not empty 200 OK requests
        if (context.HttpContext.Response.StatusCode == StatusCodes.Status200OK
            && context.OutputCacheStream.BufferingEnabled)
        {
            var contentLength = context.HttpContext.Response.ContentLength;
            var cachedResponseBody = context.OutputCacheStream.GetCachedResponseBody();

            if (!contentLength.HasValue || contentLength == cachedResponseBody.Length
                                        || (cachedResponseBody.Length == 0
                                            && HttpMethods.IsHead(context.HttpContext.Request.Method)))
            {
                var response = context.HttpContext.Response;
                // Add a content-length if required
                if (!response.ContentLength.HasValue && StringValues.IsNullOrEmpty(response.Headers.TransferEncoding))
                {
                    context.CachedResponse!.Headers ??= new HeaderDictionary();
                    context.CachedResponse.Headers.ContentLength = cachedResponseBody.Length;
                    if (_cacheConfiguration.OutputCustomHeader)
                    {
                        context.CachedResponse.Headers[_cacheConfiguration.CustomHeaderKey] = "1";
                    }
                }

                context.CachedResponse!.Body = cachedResponseBody;

                await RulesBasedOutputCacheEntrySerializer.StoreAsync(context.CacheKey,
                    context.CachedResponse,
                    context.CachedResponseValidFor,
                    _store,
                    context.HttpContext.RequestAborted);
            }
        }
    }

    private static bool OnStartResponse(RulesBasedOutputCacheContext context)
    {
        if (context.ResponseStarted)
        {
            return false;
        }

        context.ResponseStarted = true;
        context.ResponseTime = DateTimeOffset.UtcNow;

        return true;
    }

    private void StartResponse(RulesBasedOutputCacheContext context)
    {
        if (OnStartResponse(context))
        {
            FinalizeCacheHeaders(context);
        }
    }

    private void FinalizeCacheHeaders(RulesBasedOutputCacheContext context)
    {
        if (context.UseOutputCaching)
        {
            // Create the cache entry now
            var response = context.HttpContext.Response;
            var headers = response.Headers;
            context.CachedResponseValidFor =
                context.ResponseExpirationTimeSpan ?? _cacheConfiguration.DefaultCacheTimeout;

            // Setting the date on the raw response headers.
            headers.Date = HeaderUtilities.FormatDate(context.ResponseTime!.Value);

            context.Tags.Add(Constants.Constants.OutputCacheSharedTag);
            // Store the response on the state
            context.CachedResponse = new RulesBasedOutputCacheEntry
            {
                Created = context.ResponseTime!.Value,
                StatusCode = response.StatusCode,
                Tags = context.Tags.ToArray()
            };

            foreach (var header in headers)
            {
                context.CachedResponse.Headers ??= new HeaderDictionary();

                if (!string.Equals(header.Key, HeaderNames.Age, StringComparison.OrdinalIgnoreCase))
                {
                    context.CachedResponse.Headers[header.Key] = header.Value;
                }
            }

            return;
        }

        context.OutputCacheStream.DisableBuffering();
    }

    private static void RestoreResponseStream(RulesBasedOutputCacheContext context)
    {
        // Unshim response stream
        context.HttpContext.Response.Body = context.OriginalResponseStream;

        // Remove IOutputCachingFeature
        RemoveOutputCacheFeature(context.HttpContext);
    }

    private static async Task<bool> TryServeCachedResponseAsync(RulesBasedOutputCacheContext context,
        RulesBasedOutputCacheEntry? cacheEntry)
    {
        if (cacheEntry == null)
        {
            return false;
        }

        context.CachedResponse = cacheEntry;
        context.ResponseTime = DateTimeOffset.UtcNow;
        var cacheEntryAge = context.ResponseTime.Value - context.CachedResponse.Created;
        context.CachedEntryAge = cacheEntryAge > TimeSpan.Zero ? cacheEntryAge : TimeSpan.Zero;
        context.IsCacheEntryFresh = true;

        // Validate expiration
        if (context.CachedEntryAge <= TimeSpan.Zero)
        {
            context.IsCacheEntryFresh = false;
        }

        if (context.IsCacheEntryFresh)
        {
            var cachedResponseHeaders = context.CachedResponse.Headers;

            // Check conditional request rules
            if (ContentIsNotModified(context))
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status304NotModified;

                if (cachedResponseHeaders != null)
                {
                    foreach (var key in HeadersToIncludeIn304)
                    {
                        if (cachedResponseHeaders.TryGetValue(key, out var values))
                        {
                            context.HttpContext.Response.Headers[key] = values;
                        }
                    }
                }
            }
            else
            {
                var response = context.HttpContext.Response;
                // Copy the cached status code and response headers
                response.StatusCode = context.CachedResponse.StatusCode;

                if (context.CachedResponse.Headers != null)
                {
                    foreach (var header in context.CachedResponse.Headers)
                    {
                        response.Headers[header.Key] = header.Value;
                    }
                }

                // Note: int64 division truncates result and errors may be up to 1 second. This reduction in
                // accuracy of age calculation is considered appropriate since it is small compared to clock
                // skews and the "Age" header is an estimate of the real age of cached content.
                response.Headers.Age =
                    HeaderUtilities.FormatNonNegativeInt64(context.CachedEntryAge.Ticks / TimeSpan.TicksPerSecond);

                // Copy the cached response body
                var body = context.CachedResponse.Body;

                if (body is { Length: > 0 })
                {
                    try
                    {
                        await body.CopyToAsync(response.BodyWriter, context.HttpContext.RequestAborted);
                    }
                    catch (OperationCanceledException)
                    {
                        context.HttpContext.Abort();
                    }
                }
            }

            return true;
        }

        return false;
    }

    private static void CreateCacheKey(RulesBasedOutputCacheContext context, CachingRule matchingRule)
    {
        if (!string.IsNullOrEmpty(context.CacheKey))
        {
            return;
        }

        var sb = new StringBuilder();
        sb.Append(context.HttpContext.Request.Path.Value);
        if (matchingRule.VaryByQueryString)
        {
            sb.Append(context.HttpContext.Request.QueryString.Value);
        }

        if (matchingRule.VaryByUser)
        {
            sb.Append(context.HttpContext.User.Identity?.Name ?? string.Empty);
        }

        if (matchingRule.VaryByHost)
        {
            sb.Append(context.HttpContext.Request.Host.Value);
        }

        if (matchingRule.VaryByCulture)
        {
            sb.Append(CultureInfo.CurrentCulture.TwoLetterISOLanguageName +
                      CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
        }

        context.CacheKey = sb.ToString();
    }

    private static void RemoveOutputCacheFeature(HttpContext context) =>
        context.Features.Set<IRulesBasedOutputCacheFeature?>(null);

    private static bool ContentIsNotModified(RulesBasedOutputCacheContext context)
    {
        var cachedResponseHeaders = context.CachedResponse!.Headers;
        var ifNoneMatchHeader = context.HttpContext.Request.Headers.IfNoneMatch;

        if (!StringValues.IsNullOrEmpty(ifNoneMatchHeader))
        {
            if (ifNoneMatchHeader.Count == 1 && StringSegment.Equals(ifNoneMatchHeader[0], EntityTagHeaderValue.Any.Tag,
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (cachedResponseHeaders == null || StringValues.IsNullOrEmpty(cachedResponseHeaders[HeaderNames.ETag])
                                              || !EntityTagHeaderValue.TryParse(
                                                  cachedResponseHeaders[HeaderNames.ETag].ToString(), out var eTag)
                                              || !EntityTagHeaderValue.TryParseList(ifNoneMatchHeader,
                                                  out var ifNoneMatchETags))
            {
                return false;
            }

            for (var i = 0; i < ifNoneMatchETags?.Count; i++)
            {
                var requestETag = ifNoneMatchETags[i];
                if (eTag.Compare(requestETag, useStrongComparison: false))
                {
                    return true;
                }
            }
        }
        else
        {
            var ifModifiedSince = context.HttpContext.Request.Headers.IfModifiedSince;
            if (StringValues.IsNullOrEmpty(ifModifiedSince))
            {
                return false;
            }

            if (cachedResponseHeaders == null)
            {
                return false;
            }

            if (!HeaderUtilities.TryParseDate(cachedResponseHeaders[HeaderNames.LastModified].ToString(),
                    out var modified) &&
                !HeaderUtilities.TryParseDate(cachedResponseHeaders[HeaderNames.Date].ToString(), out modified))
            {
                return false;
            }

            if (HeaderUtilities.TryParseDate(ifModifiedSince.ToString(), out var modifiedSince) &&
                modified <= modifiedSince)
            {
                return true;
            }
        }

        return false;
    }
}