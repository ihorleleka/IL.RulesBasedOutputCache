[![NuGet version (IL.RulesBasedOutputCache)](https://img.shields.io/nuget/v/IL.RulesBasedOutputCache.svg?style=flat-square)](https://www.nuget.org/packages/IL.RulesBasedOutputCache/)
# Output cache middleware

* Rules based output cache middleware - inspired by Microsoft output cache

# Setup

## v5 startup files structure (Program.cs and Startup.cs)
```
ConfigureServices(IServiceCollection services)
services.AddRulesBasedOutputCache();

...

app.UseRulesBasedOutputCache();

```
## v6+ startup files structure (Single Progam.cs files)
```
var builder = WebApplication.CreateBuilder(args);
builder.AddRulesBasedOutputCache();

...
var app = builder.Build();
app.UseRulesBasedOutputCache();

```

## Extra configurations

`.AddRulesBasedOutputCache()` has optional parameter for inline service configuration.

Another invariant is providing configuration from `appsettings.json` file.
Library comes with [schema file](https://github.com/lelekaihor/IL.RulesBasedOutputCache/blob/main/Code/appsettings.outputcache.schema.json), that you will be able to select as schema in your own `appsettings.json` file.

![image](https://github.com/lelekaihor/IL.RulesBasedOutputCache/assets/67684686/73181010-30d2-4228-96eb-c3d2d9530ecb)

### Configuration parameters:
- **OutputCacheEnabled** - allows to disable module completely
- **DefaultCacheTimeout** - default expiration time for your cache entries, if not set up on the rule itself
- **CachingRules** - array of caching rules the system will have on application startup
- **SqlConnectionStringName** - optional string parameter, if provided will replace in-memory storage for rules with SQL based. You need to provide only the NAME of connection string, not the connection string itself.
- **OutputCacheAdminPanelEnabled** - enables Admin panel api and access to admin panel page, which is available by url `/rulesBasedCache/adminPanel`

![image](https://github.com/lelekaihor/IL.RulesBasedOutputCache/assets/67684686/57d762a9-cce0-4cbe-a3e0-e135eca25153)

### Caching rule parameters:

- **RuleTemplate** - template to be matched in order for rule to become active. **Examples**:
    - `/test` for `ExactPath` match rule
    - `/test/*` for `Regex` match rule
    - `.js` for `FileExtension` match rule
- **RuleAction** - `Allow` or `Dissalow`
- **RuleType** - `ExactPath`, `Regex` or `FileExtension`
- **VaryByQueryString** - includes query string to cache key
- **ResponseExpirationTimeSpan** - specific expiration time for rule. TimeSpan format ("00:00:00")

### Rules order

Rules have enforced order based on their `RuleAction` and `RuleType`. <br/>
Main ordering parameter is `RuleAction` where `Dissallow` has more priority than `Allow`. <br/>
Then rules are ordered by `RuleType` where priority goes as follows(low to high): `FileExtension`, `ExactPath`, `Regex`. <br/>
Only rule with **highest priority** will be resolved per request.