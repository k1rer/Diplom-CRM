using Diplom_CRM.Models;
using Diplom_CRM.Models.View;
using Diplom_CRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Diplom_CRM.Controllers;

[AllowAnonymous]
public class AccountController(
    SignInManager<AppUser> signInManager,
    UserManager<AppUser> userManager,
    IUserService userService) : Controller
{
    // GET: Account/Login
    [HttpGet]
    public async Task<IActionResult> Login()
    {
        var model = new LoginViewModel
        {
            AllowRegistration = !await userService.AnyUsersExistAsync()
        };
        return View(model);
    }

    // POST: Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AllowRegistration = !await userService.AnyUsersExistAsync();
            return View(model);
        }

        var user = await userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Неверный Email или пароль.");
            model.AllowRegistration = !await userService.AnyUsersExistAsync();
            return View(model);
        }

        var result = await signInManager.PasswordSignInAsync(
            user, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Учетная запись заблокирована из-за частых неудачных попыток.");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Неверный Email или пароль.");
        }

        model.AllowRegistration = !await userService.AnyUsersExistAsync();
        return View(model);
    }

    // POST: Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }

    // GET: Account/Register
    [HttpGet]
    public async Task<IActionResult> Register()
    {
        if (await userService.AnyUsersExistAsync())
            return RedirectToAction(nameof(Login));

        return View(new RegisterViewModel { AllowRegistration = true });
    }

    // POST: Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (await userService.AnyUsersExistAsync())
            return RedirectToAction(nameof(Login));

        if (!ModelState.IsValid)
            return View(model);

        var user = new AppUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName
        };

        var result = await userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            // Назначаем роль первого администратора
            await userService.AssignAdminRoleAsync(user);

            await signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Dashboard");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }
}