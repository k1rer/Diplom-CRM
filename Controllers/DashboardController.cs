using Diplom_CRM.Services;
using Microsoft.AspNetCore.Mvc;

namespace Diplom_CRM.Controllers;

public class DashboardController : Controller
{
    private readonly IAnalyticsService _analyticsService;

    public DashboardController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    // GET: Dashboard/Index
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var viewModel = await _analyticsService.GetDashboardViewModelAsync();
        return View(viewModel);
    }
}