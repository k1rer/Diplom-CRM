using Diplom_CRM.Exceptions;
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
    [HttpGet]
    public IActionResult Create()
    {
        return PartialView("_CreateCompanyPartial", new CompanyDTO());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CompanyDTO dto)
    {
        if (!ModelState.IsValid)
        {
            Response.Headers["HX-Retarget"] = "#companyModal .modal-body";
            return PartialView("_CreateCompanyPartial", dto);
        }

        try
        {
            await _clientService.CreateCompanyAsync(dto);
        }
        catch (DuplicateEntityException ex)
        {
            ModelState.AddModelError("Phone", ex.Message);
            Response.Headers["HX-Retarget"] = "#companyModal .modal-body";
            return PartialView("_CreateCompanyPartial", dto);
        }

        var updatedTable = await GetCompanyTablePartialView();
        Response.Headers["HX-Trigger"] = "closeModal";
        return updatedTable;
    }

    private async Task<PartialViewResult> GetCompanyTablePartialView()
    {
        const int pageSize = 10;
        var paged = await _clientService.GetPagedCompaniesAsync(1, pageSize, null);
        var viewModel = new CompanyIndexViewModel
        {
            Companies = paged,
            SearchTerm = null,
            CurrentPage = 1
        };
        return PartialView("_CompanyTablePartial", viewModel);
    }
}