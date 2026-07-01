using Diplom_CRM.Extensions;
using Diplom_CRM.Models.DTO;
using Diplom_CRM.Models.View;
using Diplom_CRM.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Diplom_CRM.Controllers;

public class ActivityController : Controller
{
    private readonly IActivityService _activityService;
    private readonly IClientService _clientService;
    private readonly IDealService _dealService;

    public ActivityController(IActivityService activityService, 
                              IClientService clientService, 
                              IDealService dealService)
    {
        _activityService = activityService;
        _clientService = clientService;
        _dealService = dealService;
    }

    // GET: Activity/Index
    [HttpGet]
    public async Task<IActionResult> Index(
        string? type, int? companyId, int? dealId, string? status, string? userId,
        DateTime? fromDate, DateTime? toDate, string? search, string? sortBy)
    {
        if (fromDate.HasValue)
            fromDate = DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc);
        if (toDate.HasValue)
            toDate = DateTime.SpecifyKind(toDate.Value, DateTimeKind.Utc);

        var filter = new ActivityFilterViewModel
        {
            Type = type,
            CompanyId = companyId,
            DealId = dealId,
            Status = status ?? "All",
            UserId = userId,
            FromDate = fromDate,
            ToDate = toDate,
            Search = search,
            SortBy = sortBy ?? "date_desc"
        };

        var activities = await _activityService.GetFilteredActivitiesAsync(filter);

        ViewBag.Companies = await _clientService.GetCompaniesSelectListAsync();
        ViewBag.Deals = await _dealService.GetDealsSelectListAsync();
        ViewBag.Users = new List<SelectListItem>();

        if (Request.IsHtmxRequest())
            return PartialView("_ActivityListPartial", activities);

        return View(activities);
    }

    // GET: Activity/GetPendingTasks
    [HttpGet]
    public async Task<IActionResult> GetPendingTasks()
    {
        var tasks = await _activityService.GetPendingTasksForUserAsync("current-user");
        return PartialView("_PendingTasksPartial", tasks);
    }

    // POST: Activity/ToggleTask
    [HttpPost]
    public async Task<IActionResult> ToggleTask([FromForm] int activityId, [FromForm] int companyId)
    {
        await _activityService.ToggleTaskCompletionAsync(activityId);
        var activities = await _activityService.GetActivitiesByCompanyIdAsync(companyId);
        return PartialView("~/Views/Company/_TimelinePartial.cshtml", activities);
    }

    // POST/DELETE: Activity/Delete
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

    // POST: Activity/ToggleTaskCard?activityId=...
    [HttpPost]
    public async Task<IActionResult> ToggleTaskCard(int activityId)
    {
        await _activityService.ToggleTaskCompletionAsync(activityId);
        var activity = await _activityService.GetActivityByIdAsync(activityId);
        if (activity == null)
            return NotFound();

        return PartialView("_ActivityCardPartial", activity);
    }

    // DELETE: Activity/DeleteActivity?activityId=...&type=...&companyId=...&...
    [HttpDelete("Activity/DeleteActivity")]
    public async Task<IActionResult> DeleteActivity(
        int activityId,
        string? type, int? companyId, int? dealId, string? status, string? userId,
        DateTime? fromDate, DateTime? toDate, string? search, string? sortBy)
    {
        await _activityService.DeleteActivityAsync(activityId);

        var filter = new ActivityFilterViewModel
        {
            Type = type,
            CompanyId = companyId,
            DealId = dealId,
            Status = status ?? "All",
            UserId = userId,
            FromDate = fromDate,
            ToDate = toDate,
            Search = search,
            SortBy = sortBy ?? "date_desc"
        };

        var activities = await _activityService.GetFilteredActivitiesAsync(filter);

        return PartialView("_ActivityListPartial", activities);
    }
}