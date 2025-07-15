using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RigaMetro.Web.Models.ViewModels.Account;
using System.Security.Claims;
using Microsoft.Extensions.Localization;

namespace RigaMetro.Web.Controllers;

public class AccountController : Controller {
    private readonly IConfiguration _configuration;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IConfiguration configuration, ILogger<AccountController> logger) {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string returnUrl = null) {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null) {
        _logger.LogInformation("Login attempt for user {User}", model.Username);
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid) {
            _logger.LogWarning("ModelState is not valid for user {User}", model.Username);
            return View(model);
        }

        var adminUsername = _configuration["AdminCredentials:Username"];
        var adminPassword = _configuration["AdminCredentials:Password"];

        if (model.Username == adminUsername && model.Password == adminPassword) {
            _logger.LogInformation("User {User} authenticated successfully", model.Username);

            var claims = new List<Claim> {
                new(ClaimTypes.Name, model.Username),
                new(ClaimTypes.Role, "Admin"),
                new("IsAdmin", "true")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(30)
                    : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Admin");
        }

        _logger.LogWarning("Login failed for user {User}", model.Username ?? "unknown");

        ModelState.AddModelError(string.Empty, "Incorrect username or password");

        return View(model);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Logout() {
        _logger.LogInformation("User {User} logged out", User?.Identity?.Name ?? "unknown");
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}