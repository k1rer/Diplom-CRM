// === ФАЙЛ: Controllers/DealController.cs ===
using Diplom_CRM.Data;
using Diplom_CRM.Extensions;
using Diplom_CRM.Models.DTO;
using Diplom_CRM.Models.View;
using Diplom_CRM.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Diplom_CRM.Controllers;

public class DealController : Controller
{
    private readonly IDealService _dealService;
    private readonly ApplicationDbContext _db;

    public DealController(IDealService dealService, ApplicationDbContext db)
    {
        _dealService = dealService;
        _db = db;
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

    // POST: Deal/ChangeStage?dealId=5&newStage=InProgress
    [HttpPost]
    public async Task<IActionResult> ChangeStage(int dealId, string newStage)
    {
        if (string.IsNullOrWhiteSpace(newStage))
            return BadRequest("Статус не указан.");

        await _dealService.ChangeDealStageAsync(dealId, newStage);

        var dealEntity = await _db.Deals
            .AsNoTracking()
            .Include(d => d.Contact)
            .FirstOrDefaultAsync(d => d.Id == dealId);

        if (dealEntity == null)
            return NotFound();

        var dealDto = new DealKanbanDTO
        {
            Id = dealEntity.Id,
            Name = dealEntity.Name,
            Amount = dealEntity.Amount,
            ContactName = $"{dealEntity.Contact.FirstName} {dealEntity.Contact.LastName}".Trim(),
            CompanyName = dealEntity.Contact.Company,
            ExpectedCloseDate = dealEntity.ExpectedCloseDate
        };

        if (Request.IsHtmxRequest())
            return PartialView("_DealCardPartial", dealDto);

        return RedirectToAction(nameof(Index));
    }
}