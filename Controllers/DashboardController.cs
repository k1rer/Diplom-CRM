using Diplom_CRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Diplom_CRM.Controllers;

[Authorize]
public class DashboardController(
    IAnalyticsService _analyticsService) : Controller
{
    // GET: Dashboard/Index
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var viewModel = await _analyticsService.GetDashboardViewModelAsync();
        return View(viewModel);
    }
}