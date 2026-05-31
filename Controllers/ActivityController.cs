using Diplom_CRM.Extensions;
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

    // POST: Activity/CompleteTask?activityId=10
    [HttpPost]
    public async Task<IActionResult> CompleteTask(int activityId)
    {
        await _activityService.SetTaskAsCompletedAsync(activityId);

        if (Request.IsHtmxRequest())
        {
            Response.Headers["HX-Trigger"] = "taskCompleted";
            return new EmptyResult();
        }

        return RedirectToAction("Index", "Home");
    }
}