# Output cache middleware

* Rules based output cache middleware - inspired by Microsoft output cache

# Setup

## v5 startup files structure (Program.cs and Startup.cs files)
```
ConfigureServices(IServiceCollection services)
services.AddRulesBasedOutputCache();

...

app.UseRulesBasedOutputCache();
```

## v6+ startup files structure (Single Progam.cs file)

```
var builder = WebApplication.CreateBuilder(args);
builder.AddRulesBasedOutputCache();

...
var app = builder.Build();
app.UseRulesBasedOutputCache();
```

## Tags management

In order to tag cache entries there are few extension methods available (`IL.RulesBasedOutputCache.Helpers` namespace):
- `void ApplyCustomCacheTagToCurrentRequest(this HttpContext context, string tag)`
- `ApplyCustomCacheTagsToCurrentRequest(this HttpContext context, HashSet<string> tags)`

which you can use to apply one or multiple tags to current request/potential cache entry.

Tags can be used for cache invalidation.

## Cache invalidation

As library heavily relies on microsoft implementation of output cache you can use `IOutputCacheStore` for eviction of cached entries by their tags.

```
await _store.EvictByTagAsync("CacheTag", HttpContext.RequestAborted);
```

## Extra configurations

`.AddRulesBasedOutputCache()` has optional parameter for inline service configuration.

Another invariant is providing configuration from `appsettings.json` file.
Library comes with [schema file](https://github.com/lelekaihor/IL.RulesBasedOutputCache/blob/main/Code/appsettings.outputcache.schema.json), that you will be able to select as schema in your own `appsettings.json` file.

### Configuration parameters:
- **OutputCacheEnabled** - allows to disable module completely
- **DefaultCacheTimeout** - default expiration time for your cache entries, if not set up on the rule itself
- **CachingRules** - array of caching rules the system will have on application startup
- **SqlConnectionStringName** - optional string parameter, if provided will replace in-memory storage for rules with SQL based. You need to provide only the NAME of connection string, not the connection string itself. Database and/or required table will br created automatically, all the needed migrations will be automatically applied with new versions of library.
- **OutputCacheAdminPanelEnabled** - enables Admin panel api and access to admin panel page, which is available by url `/rulesBasedCache/adminPanel`

### Caching rule parameters:

- **RuleTemplate** - template to be matched in order for rule to become active. **Examples**:
    - `/test` for `ExactPath` match rule
    - `/test/*` for `Regex` match rule
    - `.js` for `FileExtension` match rule
- **RuleAction** - `Allow` or `Dissalow`
- **RuleType** - `ExactPath`, `Regex` or `FileExtension`
- **VaryByQueryString** - includes query string to cache key
- **VaryByUser** - includes HttpContext.User.Identity.Name to cache key
- **VaryByHost** - includes host string to cache key
- **VaryByCulture** - includes both CultureInfo.CurrentCulture and CultureInfo.CurrentUICulture to cache key
- **ResponseExpirationTimeSpan** - specific expiration time for rule. TimeSpan format ("00:00:00")

### Rules order

Rules have enforced order based on their `RuleAction` and `RuleType`. <br/>
Main ordering parameter is `RuleAction` where `Dissallow` has more priority than `Allow`. <br/>
Then rules are ordered by `RuleType` where priority goes as follows(low to high): `FileExtension`, `ExactPath`, `Regex`. <br/>
Only rule with **highest priority** will be resolved per request.
