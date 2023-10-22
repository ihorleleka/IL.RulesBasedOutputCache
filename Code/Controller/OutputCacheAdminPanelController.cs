using IL.RulesBasedOutputCache.Persistence.Rules.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace IL.RulesBasedOutputCache.Controller;

[Route(Constants.Constants.AdminPanelUrlBasePath)]
public sealed class OutputCacheAdminPanelController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly IRulesRepository _rulesRepository;

    public OutputCacheAdminPanelController(IRulesRepository rulesRepository, IOutputCacheStore cacheStore)
    {
        _rulesRepository = rulesRepository;
    }

    [HttpGet]
    [Route(nameof(AdminPanel))]
    public async Task<IActionResult> AdminPanel()
    {
        var rules = await _rulesRepository.GetAll();
        return View("/Views/OutputCache/AdminPanel.cshtml", rules);
    }
}