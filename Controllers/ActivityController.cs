using Diplom_CRM.Extensions;
using Diplom_CRM.Models.DTO;
using Diplom_CRM.Services;
using Microsoft.AspNetCore.Mvc;

namespace Diplom_CRM.Controllers;

public class ActivityController : Controller
{
    private readonly IActivityService _activityService;

    public ActivityController(IActivityService activityService)
    {
        _activityService = activityService;
    }

    // GET: Activity/GetPendingTasks
    [HttpGet]
    public async Task<IActionResult> GetPendingTasks()
    {
        var tasks = await _activityService.GetPendingTasksForUserAsync("current-user");
        return PartialView("_PendingTasksPartial", tasks);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleTask([FromForm] int activityId, [FromForm] int companyId)
    {
        await _activityService.ToggleTaskCompletionAsync(activityId);
        var activities = await _activityService.GetActivitiesByCompanyIdAsync(companyId);
        return PartialView("~/Views/Company/_TimelinePartial.cshtml", activities);
    }

    [HttpDelete]
    [HttpPost]
    public async Task<IActionResult> Delete(int activityId, int companyId)
    {
        await _activityService.DeleteActivityAsync(activityId);
        var activities = await _activityService.GetActivitiesByCompanyIdAsync(companyId);
        return PartialView("~/Views/Company/_TimelinePartial.cshtml", activities);
    }

    // POST: Activity/Create
    [HttpPost]
    public async Task<IActionResult> Create(ActivityDTO dto)
    {
        if (!ModelState.IsValid)
        {
            var currentActivities = await _activityService.GetActivitiesByCompanyIdAsync(dto.CompanyId);
            return PartialView("~/Views/Company/_TimelinePartial.cshtml", currentActivities);
        }

        await _activityService.CreateActivityAsync(dto);

        var activities = await _activityService.GetActivitiesByCompanyIdAsync(dto.CompanyId);
        return PartialView("~/Views/Company/_TimelinePartial.cshtml", activities);
    }
}