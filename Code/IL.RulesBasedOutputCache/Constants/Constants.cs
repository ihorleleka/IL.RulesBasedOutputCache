namespace IL.RulesBasedOutputCache.Constants;

internal static class Constants
{
    public const string ConfigurationSection = "RulesBasedOutputCache";
    public const string AdminPanelUrlPath = "/rulesBasedCache/adminPanel";
    public const string AdminPanelApiUrlBasePath = "api/rulesBasedCache";
    public const string OutputCacheSharedTag = nameof(OutputCacheSharedTag);
    public const string AdminPanelViewPath = "/Views/OutputCache/AdminPanel.cshtml";

    public const char MatchingExtensionsSeparator = '|';
}