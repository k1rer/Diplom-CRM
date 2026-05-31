using Diplom_CRM.Extensions;
using Diplom_CRM.Models.DTO;
using Diplom_CRM.Models.View;
using Diplom_CRM.Services;
using Microsoft.AspNetCore.Mvc;

namespace Diplom_CRM.Controllers;

public class CompanyController : Controller
{
    private readonly IClientService _clientService;

    public CompanyController(IClientService clientService)
    {
        _clientService = clientService;
    }

    // GET: Company?page=1&searchTerm=...
    [HttpGet]
    public async Task<IActionResult> Index(int page = 1, string? searchTerm = null)
    {
        const int pageSize = 10;
        var pagedResult = await _clientService.GetPagedCompaniesAsync(page, pageSize, searchTerm);

        var viewModel = new CompanyIndexViewModel
        {
            Companies = pagedResult,
            SearchTerm = searchTerm,
            CurrentPage = page
        };

        if (Request.IsHtmxRequest())
            return PartialView("_CompanyTablePartial", viewModel);

        return View(viewModel);
    }

    // POST: Company/Create
    [HttpPost]
    public async Task<IActionResult> Create(CompanyDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return Request.IsHtmxRequest()
                ? PartialView("_CompanyFormPartial", dto)
                : View(dto);
        }

        await _clientService.CreateCompanyAsync(dto);

        if (Request.IsHtmxRequest())
            return RedirectToAction(nameof(Index));

        return RedirectToAction(nameof(Index));
    }
}