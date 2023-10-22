using IL.RulesBasedOutputCache.Models;
using IL.RulesBasedOutputCache.Persistence.Rules.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace IL.RulesBasedOutputCache.Controller;

[ApiController]
[Route(Constants.Constants.AdminPanelApiUrlBasePath)]
public sealed class OutputCacheAdminPanelApiController : ControllerBase
{
    private readonly IRulesRepository _rulesRepository;
    private readonly IOutputCacheStore _cacheStore;

    public OutputCacheAdminPanelApiController(IRulesRepository rulesRepository, IOutputCacheStore cacheStore)
    {
        _rulesRepository = rulesRepository;
        _cacheStore = cacheStore;
    }

    [HttpGet]
    [Route(nameof(GetAll))]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _rulesRepository.GetAll());
    }

    [HttpGet]
    [Route(nameof(EvictAllCacheEntries))]
    public async Task<IActionResult> EvictAllCacheEntries()
    {
        await _cacheStore.EvictByTagAsync(Constants.Constants.OutputCacheSharedTag, HttpContext.RequestAborted);
        return Ok("All cache entries are evicted!");
    }

    [HttpPost]
    [Route(nameof(AddRule))]
    public async Task<IActionResult> AddRule([FromForm] CachingRule rule)
    {
        await _rulesRepository.AddRule(rule);
        return Redirect($"/{Constants.Constants.AdminPanelUrlBasePath}/{nameof(OutputCacheAdminPanelController.AdminPanel)}");
    }

    [HttpPost]
    [Route(nameof(DeleteRule))]
    public async Task<IActionResult> DeleteRule([FromForm] Guid ruleId)
    {
        await _rulesRepository.DeleteRuleById(ruleId);
        return Redirect($"/{Constants.Constants.AdminPanelUrlBasePath}/{nameof(OutputCacheAdminPanelController.AdminPanel)}");
    }
}