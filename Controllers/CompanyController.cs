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
    private readonly IActivityService _activityService;

    public CompanyController(IClientService clientService, IActivityService activityService)
    {
        _clientService = clientService;
        _activityService = activityService;
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

    // GET: Company/Create
    [HttpGet]
    public IActionResult Create()
    {
        return PartialView("_CreateCompanyPartial", new CompanyDTO());
    }

    // POST: Company/Create
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

    // GET: Company/Details/{id}
    [HttpGet("Company/Details/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var companyDto = await _clientService.GetCompanyByIdAsync(id);
        if (companyDto == null)
            return NotFound();

        var contacts = await _clientService.GetContactsByCompanyIdAsync(id);

        var activities = await _activityService.GetActivitiesByCompanyIdAsync(id);

        var viewModel = new CompanyDetailsViewModel
        {
            Company = companyDto,
            Contacts = contacts,
            Activities = activities
        };

        return View(viewModel);
    }

    // POST: Company/AddContact
    [HttpPost]
    public async Task<IActionResult> AddContact(ContactDTO dto)
    {
        if (!ModelState.IsValid)
        {
            Response.Headers["HX-Retarget"] = "#addContactModal .modal-body";
            return PartialView("_ContactFormPartial", dto);
        }

        await _clientService.AddContactToCompanyAsync(dto.CompanyId, dto);

        Response.Headers["HX-Trigger"] = "closeContactModal";

        var contacts = await _clientService.GetContactsByCompanyIdAsync(dto.CompanyId);

        return PartialView("_ContactListPartial", contacts);
    }

    // DELETE/POST: Company/DeleteContact/{id}
    [HttpDelete]
    [HttpPost]
    public async Task<IActionResult> DeleteContact(int id, [FromForm] int companyId)
    {
        bool hasActivities = await _clientService.ContactHasActivitiesAsync(id);

        await _clientService.DeleteContactAsync(id);

        var contacts = await _clientService.GetContactsByCompanyIdAsync(companyId);

        if (hasActivities)
        {
            Response.Headers["HX-Trigger"] = "{\"refreshTimeline\": \"\"}";
        }

        return PartialView("_ContactListPartial", contacts);
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

    // GET: Company/EditContact/{id}
    [HttpGet("Company/EditContact/{id:int}")]
    public async Task<IActionResult> EditContact(int id)
    {
        var contactDto = await _clientService.GetContactByIdAsync(id);
        if (contactDto == null)
            return NotFound();

        return PartialView("_EditContactPartial", contactDto);
    }

    // POST: Company/EditContact
    [HttpPost]
    public async Task<IActionResult> EditContact(ContactDTO dto)
    {
        if (!ModelState.IsValid)
        {
            Response.Headers["HX-Retarget"] = "#editContactModal .modal-body";
            return PartialView("_EditContactPartial", dto);
        }

        await _clientService.UpdateContactAsync(dto);

        Response.Headers["HX-Trigger"] = "closeEditModal";

        var contacts = await _clientService.GetContactsByCompanyIdAsync(dto.CompanyId);
        return PartialView("_ContactListPartial", contacts);
    }

    // GET: Company/CheckContactBeforeDelete/{id}
    [HttpGet("Company/CheckContactBeforeDelete/{id:int}")]
    public async Task<IActionResult> CheckContactBeforeDelete(int id)
    {
        var contactDto = await _clientService.GetContactByIdAsync(id);
        if (contactDto == null) return NotFound();

        var hasActivities = await _clientService.ContactHasActivitiesAsync(id);
        var viewModel = new DeleteContactConfirmationViewModel
        {
            Contact = contactDto,
            HasActivities = hasActivities
        };
        return PartialView("_DeleteContactConfirmationPartial", viewModel);
    }

    // GET: Company/GetTimeline?companyId=...
    [HttpGet]
    public async Task<IActionResult> GetTimeline(int companyId)
    {
        var activities = await _activityService.GetActivitiesByCompanyIdAsync(companyId);
        return PartialView("_TimelinePartial", activities);
    }
}