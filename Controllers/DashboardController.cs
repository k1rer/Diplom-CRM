using Diplom_CRM.Models.View;
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
        var metrics = await _analyticsService.GetDashboardMetricsAsync();
        var funnel = await _analyticsService.GetSalesFunnelDataAsync();

        var viewModel = new DashboardViewModel
        {
            Metrics = metrics,
            Funnel = funnel
        };

        return View(viewModel);
    }
}