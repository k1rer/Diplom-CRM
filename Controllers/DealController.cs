using Diplom_CRM.Data;
using Diplom_CRM.Models.DTO;
using Diplom_CRM.Models.View;
using Diplom_CRM.Services;
using Diplom_CRM.Services.Implementations;
using Microsoft.AspNetCore.Mvc;

namespace Diplom_CRM.Controllers;

public class DealController : Controller
{
    private readonly IDealService _dealService;
    private readonly IClientService _clientService;

    public DealController(IDealService dealService, IClientService clientService)
    {
        _dealService = dealService;
        _clientService = clientService;
    }

    // GET: Deal/Index
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var kanbanData = await _dealService.GetKanbanBoardAsync();
        var viewModel = new KanbanViewModel
        {
            Columns = kanbanData
        };
        return View(viewModel);
    }

    // POST: Deal/UpdateStage
    [HttpPost]
    public async Task<IActionResult> UpdateStage([FromForm] int id, [FromForm] string newStage)
    {
        if (string.IsNullOrWhiteSpace(newStage))
            return BadRequest("Статус не указан.");

        await _dealService.ChangeDealStageAsync(id, newStage);
        var updatedDeal = await _dealService.GetDealKanbanByIdAsync(id);
        return PartialView("_DealCardPartial", updatedDeal);
    }

    // GET: Deal/Details/{id}?from=kanban|dashboard
    [HttpGet("Deal/Details/{id:int}")]
    public async Task<IActionResult> Details(int id, string? from = null)
    {
        var viewModel = await _dealService.GetDealDetailsAsync(id);
        if (viewModel == null)
            return NotFound();

        viewModel.From = from;
        return View(viewModel);
    }

    // GET: Deal/Create?companyId=...
    [HttpGet("Deal/Create")]
    public async Task<IActionResult> Create(int companyId)
    {
        var contacts = await _clientService.GetContactsByCompanyIdAsync(companyId);
        var viewModel = new CreateDealViewModel
        {
            Deal = new DealDTO { CompanyId = companyId },
            Contacts = contacts
        };
        return PartialView("_CreateDealPartial", viewModel);
    }

    // POST: Deal/Create
    [HttpPost("Deal/Create")]
    public async Task<IActionResult> Create(DealDTO dto)
    {
        if (!ModelState.IsValid)
        {
            Response.Headers["HX-Retarget"] = "#dealModalBody";
            var contacts = await _clientService.GetContactsByCompanyIdAsync(dto.CompanyId);
            var viewModel = new CreateDealViewModel { Deal = dto, Contacts = contacts };
            return PartialView("_CreateDealPartial", viewModel);
        }

        await _dealService.CreateDealAsync(dto);
        Response.Headers["HX-Trigger"] = "closeDealModal, refreshTimeline";
        return Ok();
    }

}