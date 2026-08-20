using System.Security.Claims;
using Diplom_CRM.Extensions;
using Diplom_CRM.Models.View;
using Diplom_CRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Diplom_CRM.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(IAdminService adminService) : Controller
{
    // GET: Admin/Users
    [HttpGet]
    public async Task<IActionResult> Users()
    {
        var users = await adminService.GetUsersAsync();

        if (Request.IsHtmxRequest())
            return PartialView("_UsersListPartial", users);

        return View(users);
    }

    // POST: Admin/ChangeRole
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(string userId, string newRole)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == currentUserId)
        {
            var message = Uri.EscapeDataString("Нельзя изменить роль самому себе.");
            Response.Headers["HX-Trigger"] = $"{{\"showToast\": {{\"message\": \"{message}\", \"type\": \"error\"}}}}";
            var currentUserDto = await adminService.GetUserByIdAsync(userId);
            return PartialView("_UserRowPartial", currentUserDto);
        }

        await adminService.ChangeRoleAsync(userId, newRole);
        var userDto = await adminService.GetUserByIdAsync(userId);

        if (userDto == null)
            return NotFound();

        return PartialView("_UserRowPartial", userDto);
    }

    // POST: Admin/ToggleLock
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLock(string userId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == currentUserId)
        {
            var message = Uri.EscapeDataString("Нельзя заблокировать самого себя.");
            Response.Headers["HX-Trigger"] = $"{{\"showToast\": {{\"message\": \"{message}\", \"type\": \"error\"}}}}";
            var currentUserDto = await adminService.GetUserByIdAsync(userId);
            return PartialView("_UserRowPartial", currentUserDto);
        }

        await adminService.ToggleLockoutAsync(userId);
        var userDto = await adminService.GetUserByIdAsync(userId);

        if (userDto == null)
            return NotFound();

        return PartialView("_UserRowPartial", userDto);
    }

    // GET: Admin/CreateUser
    [HttpGet]
    public IActionResult CreateUser()
    {
        return PartialView("_CreateUserPartial", new RegisterViewModel());
    }

    // POST: Admin/CreateUser
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            Response.Headers["HX-Retarget"] = "#createUserModal .modal-body";
            return PartialView("_CreateUserPartial", model);
        }

        try
        {
            await adminService.CreateUserAsync(model, "Manager");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            Response.Headers["HX-Retarget"] = "#createUserModal .modal-body";
            return PartialView("_CreateUserPartial", model);
        }

        Response.Headers["HX-Trigger"] = "{\"closeCreateUserModal\": {}, \"refreshUsersList\": {}}";
        return Ok();
    }
}