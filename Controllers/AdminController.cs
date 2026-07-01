using Diplom_CRM.Extensions;
using Diplom_CRM.Models.View;
using Diplom_CRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Diplom_CRM.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // GET: Admin/Users
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _adminService.GetUsersAsync();

            if (Request.IsHtmxRequest())
                return PartialView("_UsersListPartial", users);

            return View(users);
        }

        // POST: Admin/ChangeRole
        [HttpPost]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            await _adminService.ChangeRoleAsync(userId, newRole);
            var userDto = await _adminService.GetUserByIdAsync(userId);
            return PartialView("_UserRowPartial", userDto);
        }

        // POST: Admin/ToggleLock
        [HttpPost]
        public async Task<IActionResult> ToggleLock(string userId)
        {
            await _adminService.ToggleLockoutAsync(userId);
            var userDto = await _adminService.GetUserByIdAsync(userId);
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
        public async Task<IActionResult> CreateUser(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                Response.Headers["HX-Retarget"] = "#createUserModal .modal-body";
                return PartialView("_CreateUserPartial", model);
            }

            try
            {
                await _adminService.CreateUserAsync(model, "Manager");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                Response.Headers["HX-Retarget"] = "#createUserModal .modal-body";
                return PartialView("_CreateUserPartial", model);
            }

            Response.Headers["HX-Trigger"] = "{\"closeCreateUserModal\": {}, \"refreshUsersList\": {}}";
            return Ok();
        }
    }
}