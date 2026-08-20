using System.Security.Claims;
using Diplom_CRM.Extensions;
using Diplom_CRM.Models.DTO;
using Diplom_CRM.Models.View;
using Diplom_CRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Diplom_CRM.Controllers;

[Authorize]
public class ActivityController(
    IActivityService activityService,
    IClientService clientService,
    IDealService dealService) : Controller
{
    // GET: Activity/Index
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] ActivityFilterViewModel filter)
    {
        filter.FromDate = filter.FromDate?.ToUniversalTime();
        filter.ToDate = filter.ToDate?.ToUniversalTime();
        filter.Status ??= "All";
        filter.SortBy ??= "date_desc";

        var companiesTask = clientService.GetCompaniesSelectListAsync();
        var dealsTask = dealService.GetDealsSelectListAsync();

        await Task.WhenAll(companiesTask, dealsTask);

        ViewBag.Companies = await companiesTask;
        ViewBag.Deals = await dealsTask;
        ViewBag.Users = Enumerable.Empty<SelectListItem>();

        var activities = await activityService.GetFilteredActivitiesAsync(filter);

        if (Request.IsHtmxRequest())
            return PartialView("_ActivityListPartial", activities);

        return View(activities);
    }

    // GET: Activity/GetPendingTasks
    [HttpGet]
    public async Task<IActionResult> GetPendingTasks()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var tasks = await activityService.GetPendingTasksForUserAsync(userId);
        return PartialView("_PendingTasksPartial", tasks);
    }

    // POST: Activity/ToggleTask
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleTask(int activityId, int companyId)
    {
        await activityService.ToggleTaskCompletionAsync(activityId);
        var activities = await activityService.GetActivitiesByCompanyIdAsync(companyId);
        return PartialView("~/Views/Company/_TimelinePartial.cshtml", activities);
    }

    // DELETE: Activity/Delete
    [HttpDelete]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int activityId, int companyId)
    {
        await activityService.DeleteActivityAsync(activityId);
        var activities = await activityService.GetActivitiesByCompanyIdAsync(companyId);
        return PartialView("~/Views/Company/_TimelinePartial.cshtml", activities);
    }

    // POST: Activity/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ActivityDTO dto)
    {
        if (!ModelState.IsValid)
        {
            Response.StatusCode = 400;
            var currentActivities = await activityService.GetActivitiesByCompanyIdAsync(dto.CompanyId);
            return PartialView("~/Views/Company/_TimelinePartial.cshtml", currentActivities);
        }

        await activityService.CreateActivityAsync(dto);

        var activities = await activityService.GetActivitiesByCompanyIdAsync(dto.CompanyId);
        return PartialView("~/Views/Company/_TimelinePartial.cshtml", activities);
    }

    // POST: Activity/ToggleTaskCard
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleTaskCard(int activityId)
    {
        await activityService.ToggleTaskCompletionAsync(activityId);
        var activity = await activityService.GetActivityByIdAsync(activityId);
        if (activity == null)
            return NotFound();

        return PartialView("_ActivityCardPartial", activity);
    }

    // DELETE: Activity/DeleteActivity
    [HttpDelete]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteActivity(int activityId, [FromQuery] ActivityFilterViewModel filter)
    {
        await activityService.DeleteActivityAsync(activityId);

        filter.FromDate = filter.FromDate?.ToUniversalTime();
        filter.ToDate = filter.ToDate?.ToUniversalTime();
        filter.Status ??= "All";
        filter.SortBy ??= "date_desc";

        var activities = await activityService.GetFilteredActivitiesAsync(filter);
        return PartialView("_ActivityListPartial", activities);
    }
}