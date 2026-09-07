using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.GeneralSettings;

// ═══════════════════════════════════════════════════════════════
// GENERAL SETTINGS — singleton
// GET /GeneralSettings   → the settings document (or {} when unset)
// PUT /GeneralSettings   → replaces it, creating the row on first save
// ═══════════════════════════════════════════════════════════════
public class GeneralSettingsController : MyController
{
    private readonly IGeneralSettingRepository _repository;

    public GeneralSettingsController(IGeneralSettingRepository repository)
    {
        _repository = repository;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Get()
    {
        var item = _repository.GetAll().OrderBy(x => x.Id).FirstOrDefault();
        // Return the stored document verbatim; the screen merges it over its
        // own defaults, so an empty object is a valid first-run response.
        return Content(item?.Settings ?? "{}", "application/json");
    }

    [AllowAnonymous]
    [HttpPut]
    public IActionResult Save([FromBody] JsonElement body)
    {
        if (body.ValueKind != JsonValueKind.Object)
            return BadRequest("Expected a JSON object of settings.");

        var json = body.GetRawText();

        var item = _repository.GetAll().OrderBy(x => x.Id).FirstOrDefault();
        if (item == null)
        {
            item = new GeneralSetting
            {
                Settings  = json,
                CreatedAt = DateTime.UtcNow,
                InActive  = false,
            };
            _repository.Add(item);
        }
        else
        {
            item.Settings  = json;
            item.UpdatedAt = DateTime.UtcNow;
            _repository.Update(item);
        }

        _repository.Commit();
        return Content(json, "application/json");
    }
}
